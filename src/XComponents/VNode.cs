using System.Text.Json;

using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace XComponents {

    public record VNodeAttribute(string Name, string Value);
    public record VNodeProperty(string Name, object Value);
    public record VNodeEvent(string Name, string Handler, object[] arguments);
    public record VNodeOption(string Name, object Value);

    public class VNode(string tag, int index) {

        public string Tag { get; set; } = tag;
        public List<VNodeAttribute> Attributes { get; set; } = new();
        public List<VNodeProperty> Properties { get; set; } = new();
        public List<VNodeEvent> Events { get; set; } = new();
        public List<VNodeOption> Options { get; set; } = new();
        public VNode[]? Childrens { get; set; }
        public string? InnerText { get; set; }


        public static VNode Create(string tag, int index) {
            return new VNode(tag, index);
        }
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
    }
    

} 