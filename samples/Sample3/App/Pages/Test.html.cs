

using XComponents;
using XComponents.Attributes;

namespace Sample3.App.Pages {


    [PageRoute("/Test/{*Id}")]
    [PageLayout("layout-advanced")]
    public partial class Test : XPage {

        // props
        [State][FromRoute] public string Id { get; set; } 


    }

}
