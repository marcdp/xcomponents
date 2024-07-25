
using XComponents;
using XComponents.Attributes;

namespace Sample3.App.WebComponents  {


    public partial class XCounter2 : XWebComponent {

        //inner class
        public class Entry(string name) {
            public string name { get; set; } = name;
        }
        public class Person(string name, string surname);

        // props
        [State] public string Word { get; set; } = "my world"; 
        [State][FromAttribute] public int Value { get; set; } = 123;
        [State] public Entry[] Items { get; set; } = new Entry[] { new("hello"), new("world") };
        [State] public Person Persona { get; set; } = new Person("pepe", "lucas");

        // methods
        public override void OnLoad(XContext context) {
            base.OnLoad(context);
        }
         
    }
    
}
