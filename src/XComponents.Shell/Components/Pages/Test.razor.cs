using Microsoft.AspNetCore.Components;

namespace XShell.Components.Pages {


    public partial class Test : XPage {

        [Parameter]
        public string pageRoute { get; set; }

        //vars
        private string _title;

        //methods
        protected override void OnInitialized() {
            base.OnInitialized();
            _title = "TEst";
        }

    }

}