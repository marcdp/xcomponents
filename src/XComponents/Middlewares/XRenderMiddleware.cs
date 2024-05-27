
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace XComponents.Middlewares {

    public class XRenderMiddleware(RequestDelegate next, Configuration configuration, IServiceProvider services, Router router, ILogger<XRenderMiddleware> logger)  {
 

        // invoke
        public async Task InvokeAsync(HttpContext httpContext) {

            // Prepare
            var textWriter = new StringWriter();
            var context = new XContext(httpContext, configuration, textWriter, logger, services);

            // Pre Html
            context.PreHtml.AppendLine($"    <script type=\"importmap\">");
            context.PreHtml.AppendLine($"        {{");
            context.PreHtml.AppendLine($"            \"imports\": {{");
            context.PreHtml.AppendLine($"                \"x-components\":\"/_content/XComponents/x-components.js?v={configuration.Version}\",");
            context.PreHtml.AppendLine($"                \"x-web-component\":\"/_content/XComponents/x-web-component.js?v={configuration.Version}\"");
            context.PreHtml.AppendLine($"            }}");
            context.PreHtml.AppendLine($"        }}");
            context.PreHtml.AppendLine($"    </script>");
            context.PreHtml.AppendLine($"    <script type=\"module\">");
            context.PreHtml.AppendLine($"        import XComponents from 'x-components';");
            context.PreHtml.AppendLine($"        XComponents.config((options) => {{");
            context.PreHtml.AppendLine($"            options.version = \"{configuration.Version}\";");
            context.PreHtml.AppendLine($"        }});");
            context.PreHtml.AppendLine($"        XComponents.start();");
            context.PreHtml.AppendLine($"    </script>");

            // get page instance, from route
            var page = router.Resolve(context);
            if (page == null) {
                await next(httpContext);
                return;
            }

            // OnInit
            page.OnInit(context);
            
            // OnLoad
            await page.OnLoadAsync(context);

            // Render page
            await page.RenderPageAsync(context);
            var html = textWriter.ToString();

            // Inject PreHtml
            if (context.PreHtml.Length > 0) {
                var i = html.IndexOf("</head");
                if (i != -1) {
                    html = html.Insert(i, context.PreHtml.ToString());
                } else {
                    logger.LogWarning("Unable to inject PreHtml, </head> not found in html");
                }
            }

            // Inject PostScript
            string? htmlFooter = null;
            var iFooter = html.IndexOf("</body");
            if (iFooter != -1) {
                htmlFooter = html.Substring(iFooter);
                html = html.Substring(0, iFooter);
            }

            // Wite html
            httpContext.Response.ContentType = "text/html";
            await httpContext.Response.WriteAsync(html);

            // Write postHtml
            if (context.PostHtml.Length > 0) {
                var i = html.IndexOf("</head");
                if (i != -1) {
                    await httpContext.Response.WriteAsync(context.PostHtml.ToString());
                } else {
                    logger.LogWarning("Unable to inject PostHtml, </head> not found in html");
                }
            }

            // Write pending renders (render streaming)
            while (context.StreamingRenderResults.Count > 0) { 
                var result = await Task.WhenAny(context.StreamingRenderResults);
                context.StreamingRenderResults.Remove(result);
                await httpContext.Response.WriteAsync("<!-- XComponents Stream Render -->\n" + result.Result);
            }

            // Write footer
            if (!string.IsNullOrEmpty(htmlFooter)) {
                await httpContext.Response.WriteAsync(htmlFooter);
            }
        }

    }

} 