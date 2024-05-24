
using XComponents;
using XComponents.Attributes;

namespace Sample3.App.Pages {


    public partial class Default : XPage {


        // props
        [FromQuery] [State] public string Var { get; set; }
        [FromHeader("User-Agent")] public string UserAgent { get; set; }
        [FromServices] public Configuration Configuration { get; set; }


         
        // rpc
        [Rpc] 
        public int DoSomething(int a, int b) {
            return a + b;
        }
        [Rpc]
        public async Task<int> DoSomethingAsync(int a, int b) {
            return await Task.FromResult(a + b);
        }
         
    }

}
