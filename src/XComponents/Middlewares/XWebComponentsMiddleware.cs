using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace XComponents.Middlewares {

    public class XWebComponentsMiddleware(RequestDelegate next, IServiceProvider services)  {
 

        // invoke
        public async Task InvokeAsync(HttpContext httpContext) {

            // Return web component js
            if (httpContext.Request.Path.StartsWithSegments("/_webcomponents", out PathString remaining) && remaining.HasValue) {
                var name = remaining.Value.Substring(1);
                var component = services.GetKeyedService<XComponent>(name);
                if (component is XWebComponent webComponent) {
                    var js = webComponent.GetWebComponentJavaScript();
                    httpContext.Response.ContentType = "text/javascript";
                    await httpContext.Response.WriteAsync(js);
                    return;
                }
            }

            //next
            await next.Invoke(httpContext);
        }

    }

} 