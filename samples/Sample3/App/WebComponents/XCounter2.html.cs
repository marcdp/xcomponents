
using XComponents;
using XComponents.Attributes;

namespace Sample3.App.WebComponents  {


    public partial class XCounter2 : XWebComponent {

        // props
        [State][FromAttribute] public int Value { get; set; } = 123;
        [State] public string[] Items { get; set; } = new string[] { "hello", "world"};

        // methods
        public override void OnLoad(XContext context) {
            base.OnLoad(context);
        }
         
    }
    
}
