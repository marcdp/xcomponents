
using System.Text;

namespace XComponents.Components {


    public partial class XSuspense : XComponent {

        // var
        private Func<XContext, Task>? mSlot;
        private Func<XContext, Task>? mSlotFallback;

        
        // Render methods
        public override bool SetFromSlot(string name, Func<XContext, Task> handler) {
            if (name.Equals("")) { mSlot = handler; return true; }
            if (name.Equals("fallback")) { mSlotFallback = handler; return true; }
            return false;
        }
        public override async Task RenderAsync(string slot, XContext context) {
            var elementId = context.GetFreeElementId();
            if (context.Configuration.RenderStreaming) {
                // Render streaming
                context.Write("<span id='" + elementId + "_slot' class='loading'>");
                if (mSlotFallback != null) {
                    await mSlotFallback(context);
                } else {
                    context.Write("Loading ...");
                }
                context.Write("</span>");
                if (mSlot != null) {
                    var task = Task.Run<string>(async () => {
                        var subWriter = new StringWriter();
                        var subContext = new XContext(context.HttpContext, context.Configuration, subWriter, context.Logger, context.Services);
                        subContext.Write("<template id='" + elementId + "_template'>");
                        await mSlot(subContext);
                        subContext.Write("</template>");
                        subContext.Write("<script>document.getElementById('" + elementId + "_slot').replaceWith(document.getElementById('" + elementId + "_template').content);</script>");
                        var subHtml = subWriter.ToString();
                        return subHtml;
                    });
                    context.StreamingRenderResults.Add(task);
                }
            } else {
                // No Render streaming
                if (mSlot != null) {
                    await mSlot(context);
                }
            }
        }

    }
   
}
