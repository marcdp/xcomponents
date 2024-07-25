using System.Data;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace XComponents {

    public record VNodeAttribute(string Name, string Value);
    public record VNodeProperty(string Name, object Value);
    public record VNodeEvent(string Name, string Handler, object[] arguments);
    public record VNodeOption(string Name, object Value);
    public record VNodeSlot(string Name, VNode[] Children);

    public class VNode {

        //props
        public string Tag { get; set; }
        public int Index { get; set; }
        public List<VNodeAttribute> Attributes { get; set; } = new();
        public List<VNodeProperty> Properties { get; set; } = new();
        public List<VNodeEvent> Events { get; set; } = new();
        public List<VNodeOption> Options { get; set; } = new();
        public List<VNodeSlot> Slots { get; set; } = new();
        public VNode[]? Childrens { get; set; }
        public string? InnerText { get; set; }


        //constructor
        public VNode(string tag, int index) {
            Tag = tag;
            Index = index;
        }


        //methods
        public VNode Attribute(string name, string value) {
            Attributes.Add(new(name, value));
            return this;
        }
        public VNode Property(string name, object value) {
            Properties.Add(new(name, value));
            return this;
        }
        public VNode Event(string name, string handler, params object[] arguments) {
            Events.Add(new(name, handler, arguments));
            return this;
        }
        public VNode Option(string name, object value) {
            Options.Add(new(name, value));
            return this;
        }
        public VNode Slot(string name, VNode[] children) {
            Slots.Add(new(name, children));
            return this;
        }
        public VNode Children(VNode[] children) {
            Childrens = children;
            return this;
        }
        public VNode Text(string text) {
            InnerText = text;
            return this;
        }
        public VNode Text(object text) {
            InnerText = text?.ToString() ?? "";
            return this;
        }


        //methods
        public async Task RenderAsync(XContext context, XBase component, Func<string, XContext, Task<bool>> renderSlotCallback) {
            if (Tag.Equals("#text")) {
                //#text
                context.WriteHtml(InnerText);
            } else if (Tag.Equals("#comment")) {
                //#comment
                context.Write(InnerText);
            } else if (Tag.Equals("template") && Attributes.FirstOrDefault(a => a.Name == "slot") != null) {
                //template
                if (Childrens != null) {
                    foreach (var children in Childrens) {
                        await children.RenderAsync(context, component, (slot, context) => {
                            return Task.FromResult(false);
                        });
                    }
                }
            } else if (Tag.Equals("slot")) {
                //slot
                if (renderSlotCallback != null) {
                    var name = Attributes.FirstOrDefault(a => a.Name == "name")?.Value ?? "";
                    await renderSlotCallback(name, context);
                }
            } else if (Tag.IndexOf("-")!=-1) {
                // SSR
                var instance = context.Services.GetKeyedService<XComponent>(Tag);
                if (instance == null) throw new Exception($"XComponent not found: {Tag}");
                instance.OnInit(context, component);
                foreach (var attribute in Attributes) {
                    instance.SetFromAttribute(attribute.Name, attribute.Value);
                }
                foreach (var property in Properties) {
                    instance.SetFromProperty(property.Name, property.Value);
                }
                await instance.OnLoadAsync(context);
                await instance.RenderAsync("", context, async (slotName, context) => {
                    //slots: search for slots in current node, otherwise should ask to the parent component
                    var slotsFound = 0;
                    foreach(var slot in Slots) {
                        if (slot.Name.Equals(slotName)) {
                            foreach (var child in slot.Children) {
                                await child.RenderAsync(context, component, renderSlotCallback);
                            }
                            slotsFound++;
                        }
                    }
                    if (slotsFound == 0 && slotName.Equals("") && Childrens != null) { 
                        foreach (var child in Childrens) {
                            await child.RenderAsync(context, component, renderSlotCallback);
                        }
                        slotsFound++;
                    }
                    if (slotsFound == 0 && !slotName.Equals("") && renderSlotCallback != null && await renderSlotCallback(slotName, context)) {
                        slotsFound++;
                    }
                    return (slotsFound == 0);
                });

            } else {
                //html node
                context.Write("<" + Tag);
                foreach(var attribute in Attributes) {
                    context.WriteHtmlAttribute(attribute.Name, attribute.Value);
                }
                context.Write(">");
                if (Childrens != null) {
                    foreach (var children in Childrens) {
                        await children.RenderAsync(context, component, renderSlotCallback);
                    }
                }
                context.Write("</" + Tag + ">");
            }
        }
    }

} 