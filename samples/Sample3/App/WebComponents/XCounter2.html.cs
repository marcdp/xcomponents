
using XComponents;
using XComponents.Attributes;

namespace Sample3.App.WebComponents  {


    public partial class XCounter2 : XWebComponent {

        //inner class
        public class Entry(string name) {
            public string name { get; set; } = name;
        } 

        // props
        [State][FromAttribute] public int Value { get; set; } = 123;
        [State] public Entry[] Items { get; set; } = new Entry[] { new("hello"), new("world") };

        // methods
        public override void OnLoad(XContext context) {
            base.OnLoad(context);
        }
         
    }
    
}
