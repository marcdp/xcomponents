namespace XComponents {

    public abstract class XWebComponent : XComponent {

        // props
        public Dictionary<string, string> Attributes { get;  } = new();

        // methods
        
        public virtual string RenderJsWebComponent() {
            return "";
        }

        

    } 


}

