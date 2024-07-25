using System.Text;
using System.Web;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace XComponents {

    public class XContext(HttpContext httpContext, Configuration configuration, TextWriter writer, ILogger logger, IServiceProvider services) {


        // vars
        public int mId;


        // props
        public IServiceProvider Services { get; } = services;
        public Configuration Configuration { get; } = configuration;
        public HttpContext HttpContext { get; } = httpContext;
        public ILogger Logger { get; } = logger;
        public CancellationToken CancellationToken { get; } = httpContext.RequestAborted;

        public StringBuilder PreHtml { get; } = new();
        public StringBuilder PostHtml { get; } = new();

        public List<Task<string>> StreamingRenderResults { get; } = new();


        // methods
        public int GetFreeId() {
            return ++mId;
        }
        public string GetFreeElementId() {
            return "__element" + (++mId);
        }
        

        // writers
        public void Write(string? text) {

            writer.Write(text);
        }
        public void Write(object? text) {
            if (text != null) Write(text.ToString());
        }
        public void WriteHtml(string? text) {
            writer.Write(HttpUtility.HtmlEncode(text));
        }
        public void WriteHtml(object? text) {
            if (text != null) WriteHtml(text.ToString());
        }
        public void WriteHtmlAttribute(string? text) {
            writer.Write(HttpUtility.HtmlAttributeEncode(text));
        }
        public void WriteHtmlAttribute(object? text) {
            if (text != null) WriteHtmlAttribute(text.ToString());
        }
        public void WriteHtmlAttribute(string name, string? text) {
            writer.Write(" " + name + "=\"" + HttpUtility.HtmlAttributeEncode(text) + "\"");
        }
        public void WriteHtmlAttribute(string name, object? text) {
            if (text != null) WriteHtmlAttribute(name, text.ToString());
        }


    }
}
