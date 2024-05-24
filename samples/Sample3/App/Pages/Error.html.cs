

using XComponents;
using XComponents.Attributes;

namespace Sample3.App.Pages {


    public partial class Error : XPage {

        // props
        [State] public int StatusCode { get; set; }
        [State] public string Message { get; set; } = "";

        // methods
        public override void OnLoad(XContext context) {
            StatusCode = context.HttpContext.Response.StatusCode;
            Message = context.HttpContext.Response.StatusCode switch {
                404 => "Page not found",
                500 => "Server error",
                _ => "Sorry, an error occurred."
            };
        } 

    }

}
