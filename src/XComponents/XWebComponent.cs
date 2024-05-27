namespace XComponents {

    public abstract class XWebComponent : XComponent {

        // props
        public Dictionary<string, string> Attributes { get;  } = new();

        // methods
        public virtual VNode[] RenderVNodes(XContext __context) {
            throw new NotImplementedException();
        }
        public virtual string GetWebComponentJavaScript() {
            return "";
        }

        

    } 


}

