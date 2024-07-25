 
namespace XComponents {


    public abstract class XBase {
        
        // Vars
        public XBase? Parent { get; private set; }

        // Init methods
        public virtual void OnInit(XContext context, XBase? parent) {
            Parent = parent;
        }

        // Load methods
        public virtual void OnLoad(XContext context) { }
        public virtual Task OnLoadAsync(XContext context) {
            OnLoad(context);
            return Task.CompletedTask;
        }
         
        // RPC methods 
        public virtual Task<object?> OnRpcAsync(XContext context, string method, object[] parameters) {
            return Task.FromResult<object?>(null);
        }

        // Set methods
        public virtual bool SetFromAttribute(string name, string value) => false;
        public virtual bool SetFromProperty(string name, object? value) => false;
        public virtual bool SetFromRoute(string name, string value) => false;
        public virtual bool SetFromHeader(string name, string value) => false;
        public virtual bool SetFromQuery(string name, string value) => false;

        // Render methods
        protected virtual VNode[]? RenderVNodes(string slot, XContext context) {
            return [];
        }
        public virtual async Task<bool> RenderAsync(string slot, XContext context, Func<string, XContext, Task<bool>> renderSlotCallback) {
            var vNodes = RenderVNodes(slot, context); 
            if (vNodes == null) return false;
            foreach (var vNode in vNodes) {
                await vNode.RenderAsync(context, this, renderSlotCallback);
            }
            return true;
        }
        
    }

}

