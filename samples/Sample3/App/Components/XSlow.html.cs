using XComponents;
using XComponents.Attributes;

namespace Sample3.App.Components {

    public partial class XSlow : XComponent {

        public override async Task OnLoadAsync(XContext context) {
            await Task.Delay(2000);
        }
    }
   
}
