
using XComponents;
using XComponents.Attributes;

namespace Sample3.App.WebComponents  {


    public partial class XDate : XWebComponent {

        // props
        [State] public DateTime Value { get; set; } = new(); 
        
    }
    
}
