using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace XComponents.Middlewares {

    public class XJsonRpcMiddleware(RequestDelegate next, Configuration configuration, IServiceProvider services, Router router, ILogger<XRenderMiddleware> logger)  {


        // inner class
        private class JsonRcpRequest {
            public string JsonRpc { get; set; } = "1.0";
            public string Method { get; set; } = "";
            public object[] Params { get; set; } = [];
            public int Id { get; set; } = 0;
        }


        // invoke
        public async Task InvokeAsync(HttpContext httpContext) {
            // json-rpc
            if (httpContext.Request.Method == "POST") {
                if (httpContext.Request.ContentType == "application/json-rpc") {
                    httpContext.Request.ContentType = "application/json";
                    var jsonRpcRequest = await httpContext.Request.ReadFromJsonAsync<JsonRcpRequest>();
                    if (jsonRpcRequest != null) {
                        var textWriter = new StringWriter();
                        var context = new XContext(httpContext, configuration, textWriter, logger, services);
                        // get page instance from route
                        XBase? target = router.Resolve(context);
                        if (target != null && jsonRpcRequest.Method.IndexOf(":") != -1) {
                            //componentName/method
                            var componentName = jsonRpcRequest.Method.Substring(0, jsonRpcRequest.Method.IndexOf(":"));
                            jsonRpcRequest.Method = jsonRpcRequest.Method.Substring(jsonRpcRequest.Method.IndexOf(":") + 1);
                            target = services.GetKeyedService<XComponent>(componentName)!;
                            if (target != null) {
                                target.OnInit(context, null);
                                target.OnLoad(context);
                            }
                        }
                        //not found
                        if (target == null) {
                            await httpContext.Response.WriteAsJsonAsync(new {
                                Jsonrpc = "2.0",
                                Error = new {
                                    Code = 404,
                                    Message = "Component not found"
                                },
                                jsonRpcRequest.Id
                            });
                            return;
                        }
                        //invoke method
                        httpContext.Response.ContentType = "application/json-rpc";
                        try {
                            var result = await target.OnRpcAsync(context, jsonRpcRequest.Method, jsonRpcRequest.Params);
                            await httpContext.Response.WriteAsJsonAsync(new {
                                Jsonrpc = "2.0",
                                Result = result,
                                jsonRpcRequest.Id
                            });
                        } catch (Exception ex) {
                            await httpContext.Response.WriteAsJsonAsync(new {
                                Jsonrpc = "2.0",
                                Error = new {
                                    Code = 500,
                                    Message = ex.Message
                                },
                                jsonRpcRequest.Id
                            });

                        }
                    }
                    return;
                }
            }
            //next
            await next.Invoke(httpContext);
        }

    }

} 