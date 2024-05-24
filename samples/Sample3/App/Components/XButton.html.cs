
using XComponents;
using XComponents.Attributes;

namespace Sample3.App.Components  {


    public partial class XButton : XComponent {

        // props
        [State][FromAttribute] public string Label { get; set; } = "";
        [State][FromAttribute] public string Color { get; set; } = "";
        [State][FromAttribute] public string MyColor { get; set; } = "";
        [State][FromAttribute] public DateTime MyDateTime { get; set; } = default!;
        [State][FromAttribute] public IDictionary<string, string>? MyDictionary { get; set; }


    }
    
}
