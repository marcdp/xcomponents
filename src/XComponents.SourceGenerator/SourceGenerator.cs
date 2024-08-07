﻿using System;
using System.Linq;
using System.Text;

using HtmlAgilityPack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XComponents.SourceGenerator {

    public class SourceGenerator {


        // inner class
        public class Definition {
            public string Name { get; set; } = "";
            public string Namespace { get; set; } = "";
            public string TypePath { get; set; } = "";
            public string Extends { get; set; } = "";
            public bool IsPage { get; set; } = false;
            public bool IsWebComponent { get; set; } = false;
            public string Template { get; set; } = "";
            public string TemplatePath { get; set; } = "";
            public string TemplateEngine { get; set; } = "x";
            public string ComponentName { get; set; } = "";
            public string? LayoutName { get; set; }
            public StateProperty[] Properties { get; set; } = [];
            public RpcMethod[] RpcMethods { get; set; } = [];            
        }
        public class StateProperty {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public bool Readonly { get; set; } = false;
            public bool StateAttribute { get; set; } = false;
            public string? FromAttribute { get; set; }
            public string? FromQuery { get; set; }
            public string? FromRoute { get; set; } 
            public string? FromHeader { get; set; }
            public string? FromServices { get; set; }
        }
        public class RpcMethodParam {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
        }
        public class RpcMethod {
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public RpcMethodParam[] Params { get; set; } = []; 
        }
        public class Diagnostics(GeneratorExecutionContext context) {
            public void Report(DiagnosticDescriptor descriptor, string templatePath, int line, int column, int position, string message) {
                var location = Location.Create(templatePath, new TextSpan(position, 1), new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column + 1)));
                context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
            }
            public void Report(DiagnosticDescriptor descriptor, HtmlNode node, string templatePath, string message) {
                var line = node.Line - 1;
                var column = node.LinePosition;
                var position = node.StreamPosition;
                var location = Location.Create(templatePath, new TextSpan(position, 1), new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column + 1)));
                context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
            }
            public void Report(DiagnosticDescriptor descriptor, HtmlAttribute attribute, string templatePath, string message) {
                var line = attribute.Line - 1;
                var column = attribute.LinePosition;
                var position = attribute.StreamPosition;
                var location = Location.Create(templatePath, new TextSpan(position, 1), new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column + 1)));
                context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
            }
        }


        //methods
        public string Generate(Definition definition, Diagnostics diagnostics) {
            var code = new StringBuilder();          
            // Parse html
            var html = definition.Template;
            var document = new HtmlDocument();
            document.LoadHtml(html);
            document.ParseErrors.ToList().ForEach(error => {
                var (line, column) = Utils.GetTextPosition(html, error.Line, error.LinePosition, 0);
                diagnostics.Report(DiagnosticDescriptors.XC1001__HtmlParseError, definition.TemplatePath, line, column, error.StreamPosition, error.Reason);
            });
            var slots = document.DocumentNode.SelectNodes("//slot")?.ToList().Select(x => x.GetAttributeValue("name", "")).ToList();
            // Create code
            //int sequence = 0;
            code.AppendLine($"using XComponents;");
            code.AppendLine($"using System.Threading.Tasks;");
            code.AppendLine();
            code.AppendLine($"namespace " + definition.Namespace + " {");  
            code.AppendLine();
            code.AppendLine($"    public partial class {definition.Name} {definition.Extends} {{");
            code.AppendLine($"        #nullable enable");
            code.AppendLine($"        #pragma warning disable");
            code.AppendLine();
            // State
            code.AppendLine($"        // Inner class");
            code.AppendLine($"        public class State({definition.Name} component) {{");
            foreach (var property in definition.Properties) {
                if (property.StateAttribute) {
                    code.AppendLine($"            public {property.Type} {property.Name} {{ get; }} = component.{property.Name};");
                }
            }
            code.AppendLine($"        }}");
            code.AppendLine();
            //// Vars
            //code.AppendLine($"        // Vars");
            //slots?.ToList().ForEach(slot => {
            //    code.AppendLine($"        private Func<XContext, Task> _slot{slot};");
            //});
            //code.AppendLine();
            // OnInit
            code.AppendLine($"        // Set methods");
            code.AppendLine($"        public override void OnInit(XContext context, XBase? parent) {{");
            code.AppendLine($"            base.OnInit(context, parent);");
            var index = 0;
            foreach (var property in definition.Properties) {
                if (property.FromQuery != null) {
                    code.AppendLine($"            if (context.HttpContext.Request.Query.TryGetValue(\"{property.FromQuery}\", out var value{index})) SetFromQuery(\"{property.FromQuery}\", value{index++}.ToString());");
                }
                if (property.FromHeader != null) {
                    code.AppendLine($"            if (context.HttpContext.Request.Headers.TryGetValue(\"{property.FromHeader}\", out var value{index})) SetFromHeader(\"{property.FromHeader}\", value{index++}.ToString());");
                }
                if (property.FromServices != null) {
                    if (property.FromServices.Length == 0) {
                        code.AppendLine($"            SetFromProperty(\"{property.Name}\", context.Services.GetRequiredService<{property.Type}>());");
                    } else {
                        code.AppendLine($"            SetFromProperty(\"{property.Name}\", context.Services.GetRequiredKeyedService<{property.Type}>(\"{property.FromServices}\"));");
                    }
                }
            }
            code.AppendLine($"        }}");
            // SetFromAttribute
            code.AppendLine($"        public override bool SetFromAttribute(string name, string value) {{");
            if (definition.IsWebComponent) {
                code.AppendLine($"            this.Attributes[name] = value;");
            }
            foreach (var property in definition.Properties) {
                if (property.FromAttribute != null) {
                    if (!property.Readonly) {
                        code.AppendLine($"            if (name.Equals(\"{property.FromAttribute}\")) {{{property.Name} = ({property.Type}) XComponents.Converter.To<{property.Type}>(value); return true;}}");
                    }
                }
            }
            code.AppendLine($"            return false;");
            code.AppendLine($"        }}");
            // SetFromProperty
            code.AppendLine($"        public override bool SetFromProperty(string name, object? value) {{");
            foreach (var property in definition.Properties) {
                if (!property.Readonly) code.AppendLine($"            if (name.Equals(\"{property.Name}\", System.StringComparison.OrdinalIgnoreCase)) {{{property.Name} = (value == null ? default : ({property.Type}) XComponents.Converter.To<{property.Type}>(value))!; return true;}}");
            }
            code.AppendLine($"            return false;");
            code.AppendLine($"        }}");
            //// SetFromSlot
            //code.AppendLine($"        public override bool SetFromSlot(string name, Func<XContext, Task> handler) {{");
            //document.DocumentNode.SelectNodes("//slot")?.ToList().ForEach(node => {
            //    var slotName = node.GetAttributeValue("name", "");
            //    code.AppendLine($"            if (name.Equals(\"{slotName}\")) {{_slot{slotName} = handler; return true;}}");
            //});
            //code.AppendLine($"            return false;");
            //code.AppendLine($"        }}");
            // SetFromRoute
            code.AppendLine($"        public override bool SetFromRoute(string name, string value) {{");
            foreach (var property in definition.Properties) {
                if (property.FromRoute != null) {
                    if (!property.Readonly) {
                        code.AppendLine($"            if (name.Equals(\"{property.FromRoute}\", System.StringComparison.OrdinalIgnoreCase)) {{{property.Name} = ({property.Type}) XComponents.Converter.To<{property.Type}>(value); return true;}}");
                    }
                }
            }
            code.AppendLine($"            return false;");
            code.AppendLine($"        }}");
            // SetFromQuery
            code.AppendLine($"        public override bool SetFromQuery(string name, string value) {{");
            foreach (var property in definition.Properties) {
                if (property.FromQuery != null) {
                    if (!property.Readonly) {
                        code.AppendLine($"            if (name.Equals(\"{property.FromQuery}\", System.StringComparison.OrdinalIgnoreCase)) {{{property.Name} = ({property.Type}) XComponents.Converter.To<{property.Type}>(value); return true;}}");
                    }
                }
            }
            code.AppendLine($"            return false;");
            code.AppendLine($"        }}");
            // SetFromHeader
            code.AppendLine($"        public override bool SetFromHeader(string name, string value) {{");
            foreach (var property in definition.Properties) {
                if (property.FromHeader != null) {
                    if (!property.Readonly) {
                        code.AppendLine($"            if (name.Equals(\"{property.FromHeader}\", System.StringComparison.OrdinalIgnoreCase)) {{{property.Name} = ({property.Type}) XComponents.Converter.To<{property.Type}>(value); return true;}}");
                    }
                }
            }
            code.AppendLine($"            return false;");
            code.AppendLine($"        }}");
            // Rpc
            code.AppendLine("");
            code.AppendLine($"        // Rpc");
            code.AppendLine($"        public override async Task<object?> OnRpcAsync(XContext context, string method, object[] parameters) {{");
            foreach (var rpcMethod in definition.RpcMethods) {
                var isAsync = rpcMethod.Type.IndexOf(".Task<") != -1;
                code.AppendLine($"            if (method.Equals(\"{rpcMethod.Name}\", System.StringComparison.OrdinalIgnoreCase)) {{");
                code.Append($"                return ");
                if (!isAsync) {
                    code.Append("");
                } else {
                    code.Append("await ");
                }
                code.Append($"{rpcMethod.Name}(");
                var paramIndex = 0;
                foreach ( var rpcMethodParam in rpcMethod.Params) {
                    code.Append((paramIndex>0 ? ", " : "") + $"XComponents.Converter.To<{rpcMethodParam.Type}>(parameters[{paramIndex}])");
                    paramIndex++;
                }
                code.Append($")");
                if (!isAsync) {
                    code.AppendLine($";");
                } else {
                    code.AppendLine($";");
                }
                code.AppendLine($"            }}");
            }
            code.AppendLine($"            throw new Exception(\"Unable to execute RPC call: method not found: \" + method.Replace(\"<\", \"&lt;\"));");
            code.AppendLine($"        }}");            
            code.AppendLine("");
            //// RenderSlot
            //code.AppendLine($"        public override Task RenderSlotAsync(string slot, XContext context) {{");
            //slots?.ToList().ForEach(slot => {
            //    code.AppendLine($"            if (slot.Equals(\"{slot}\") && _slot{slot} != null) return _slot{slot}(context);");
            //});
            //code.AppendLine($"            return Task.CompletedTask;");
            //code.AppendLine($"        }}");
            // Render
            code.AppendLine($"        // Render");
            //if (definition.IsPage) {
            //    var layoutName = definition.LayoutName ?? "layout-default";
            //    code.AppendLine($"        public override async Task RenderPageAsync(XContext context) {{");
            //    if (definition.LayoutName != null) {
            //        code.AppendLine($"            var layoutName = \"{layoutName}\";");
            //    } else { 
            //        code.AppendLine($"            var layoutName = context.Configuration.LayoutDefault;");
            //    }
            //    code.AppendLine($"            var layout = context.Services.GetKeyedService<XLayout>(layoutName);");
            //    code.AppendLine($"            if (layout == null) throw new Exception($\"XLayout not found: {{layoutName}}\");");
            //    code.AppendLine($"            layout.OnInit(context);");
            //    //document.DocumentNode.ChildNodes.Where(x => x.Attributes.Contains("slot")).ToList().ForEach(node => {
            //    //    var slotName = node.GetAttributeValue("slot", "");
            //    //    code.AppendLine($"            layout.SetFromSlot(\"{slotName}\", async (__context) => await RenderAsync(\"{slotName}\", __context, this));");
            //    //});
            //    //code.AppendLine($"            layout.SetFromSlot(\"\", (context) => RenderAsync(\"\", context, this));");
            //    code.AppendLine($"            await layout.RenderAsync(\"\", context, this);");
            //    code.AppendLine($"        }}");
            //}
            // Render
            var cs = (new RenderFunctionXWebComponent()).CreateRenderServer(definition, diagnostics, document);
            code.AppendLine(cs);
            //if (true || definition.IsWebComponent) {
            //    var cs = (new RenderFunctionXWebComponent()).CreateRenderServer(definition, diagnostics, document);
            //    code.AppendLine(cs);
            //} else {
            //    var cs = (new RenderFunctionX()).CreateRenderServer(definition, diagnostics, document);
            //    code.AppendLine(cs);
            //}
            // RenderClient
            if (definition.IsWebComponent) {
                HtmlNode? script = document.DocumentNode.SelectSingleNode("script[@type='module']");
                code.AppendLine($"        public override string RenderJsWebComponent() {{");
                if (script != null) {
                    var js = (new RenderFunctionXWebComponent()).CreateRenderClient(definition, diagnostics, script.InnerHtml, document);
                    code.AppendLine($"            return ");
                    code.AppendLine($"\"\"\"");
                    code.AppendLine(js.ToString());
                    code.AppendLine($"\"\"\";");
                } else {
                    var js = (new RenderFunctionXWebComponent()).CreateRenderClient(definition, diagnostics, "", document);
                    code.AppendLine($"            return ");
                    code.AppendLine($"\"\"\"");
                    code.AppendLine(js.ToString());
                    code.AppendLine($"\"\"\";");
                }
                code.AppendLine($"        }}");
            }
            // Footer
            code.AppendLine($"    }}");
            code.AppendLine($"}}");
            // Return code
            return code.ToString();
        }
    } 


}







