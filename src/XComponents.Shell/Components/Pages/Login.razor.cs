using Microsoft.AspNetCore.Components;

namespace XShell.Components.Pages {


    public partial class Login : XPage {

        //cllas
        public class State {
            public string Id { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }


        //vars
        public State state { get; set; } = new();


        //methods
        protected void OnGet() {
            state.Id = "1";
            state.Value = "Hello World";
        }

    }

}