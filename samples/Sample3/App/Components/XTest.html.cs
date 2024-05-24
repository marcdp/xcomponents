using XComponents;
using XComponents.Attributes;

namespace Sample3.App.Components {


    public partial class XTest : XComponent {

        //props
        [State] [FromAttribute()] public string Word { get; set; } = "hello";
        [State] public string MyWord { get; set; } = "my hello";
        [State] public int Counter { get; set; } = 1;
        [State] public string[] Items { get; set; } = ["hello", "world", "this", "one"];
        [State] public object AObject { get; set; } = new { name = "john", age = 30 }; 
        [State] public IDictionary<string, object> Dict { get; } = new Dictionary<string, object> { { "name", "john" }, { "age", 30 } };
         
    }
   
}
