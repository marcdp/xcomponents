 
namespace XComponents {


    public abstract class XBase {

        // Init methods
        public virtual void OnInit(XContext context) {}

        // Load methods
        public virtual void OnLoad(XContext context) { }
        public virtual Task OnLoadAsync(XContext context) {
            OnLoad(context);
            return Task.CompletedTask;
        }

        //// Get/Post methods
        //public virtual void OnGet(XContext context) { }
        //public virtual Task OnGetAsync(XContext context) {
        //    OnGet(context);
        //    return Task.CompletedTask;
        //}
        //public virtual void OnPost(XContext context) { }
        //public virtual Task OnPostAsync(XContext context) {
        //    OnPost(context);
        //    return Task.CompletedTask;
        //}

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
        public virtual bool SetFromSlot(string name, Func<XContext, Task> handler) => false;

        // Render methods
        public virtual Task RenderAsync(string slot, XContext context) {
            return Task.CompletedTask;
        }
        
    }


}

