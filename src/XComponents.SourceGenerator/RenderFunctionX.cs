using System;
using System.Linq;
using System.Text;

using HtmlAgilityPack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XComponents.SourceGenerator {

    public class RenderFunctionX {


        // Vars
        private readonly string[] HTML_SELFCLOSING_TAGS = ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr", "", ""];

        // Methods
        public void CreateRenderFunction(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, StringBuilder code, HtmlNodeCollection nodes, int level, ref int sequence) {
            CreateRenderFunctionRecursive(definition, diagnostics, code, nodes, level, ref sequence);
        }
        public void CreateRenderFunctionRecursive(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, StringBuilder code, HtmlNodeCollection nodes, int level, ref int sequence) {
            var baseLevel = 4;
            var indent = new String(' ', (level + baseLevel) * 4);
            var ifName = "";
            var index = 0;
            foreach (var node in nodes) { 
                //inc sequence
                sequence++;
                //process
                if (node.NodeType == HtmlNodeType.Text) {
                    //#text
                    var text = node.InnerText;
                    var i = 0;
                    var j = text.IndexOf("{{", i);
                    code.AppendLine($"{indent}//#text");
                    while (j != -1) {
                        //pre text
                        var pre = text.Substring(i, j - i);
                        code.AppendLine($"{indent}__context.Write({Utils.EscapeCsString(pre)});");
                        var k = text.IndexOf("}}", j);
                        if (k == -1) {
                            //error
                            diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, []);
                            break;
                        }
                        //expression
                        var expression = text.Substring(j + 2, k - j - 2);
                        if (definition.ComponentName == "x-counter-2" && expression == "item") {
                            int kkk = 123;
                        }
                        var (startLine, startColumn) = Utils.GetTextPosition(text, node.Line, node.LinePosition, j);
                        var (endLine, endColumn) = Utils.GetTextPosition(text, node.Line, node.LinePosition, k);
                        code.AppendLine($"{indent}#line ({startLine},{startColumn + 2 + 1})-({endLine},{endColumn}) 1 \"{definition.TemplatePath}\"");
                        //write value
                        //code.AppendLine($"{indent}__context.WriteHtml(\"<!-- x:text -->\");");
                        code.AppendLine($"{indent}__context.WriteHtml({expression});");
                        //code.AppendLine($"{indent}__context.WriteHtml(\"<!-- /x:text -->\");");
                        //line hidden
                        code.AppendLine($"{indent}#line hidden");
                        i = k + 2;
                        j = text.IndexOf("{{", i);
                    }
                    //post text
                    var post = text.Substring(i);
                    code.AppendLine($"{indent}__context.Write({Utils.EscapeCsString(post)});");

                } else if (node.NodeType == HtmlNodeType.Comment) {
                    //#comment
                    code.AppendLine($"{indent}//#comment");
                    code.AppendLine($"{indent}__context.Write({Utils.EscapeCsString(node.InnerHtml)});");

                } else if (node.NodeType == HtmlNodeType.Element && node.Name.Equals("x:text")) {
                    //<x:text></x:text>
                    var length = 8;
                    code.AppendLine($"{indent}//<x:text>");
                    code.AppendLine($"{indent}#line ({node.Line},{node.LinePosition + length + 1})-({node.Line},{node.LinePosition + length + node.InnerHtml.Length}) 1 \"{definition.TemplatePath}\"");
                    code.AppendLine($"{indent}__context.WriteHtml({node.InnerText});");
                    code.AppendLine($"{indent}#line hidden");

                } else if (node.NodeType == HtmlNodeType.Element && node.Name.Equals("x:json")) {
                    //<x:json></x:json>
                    var length = 8;
                    code.AppendLine($"{indent}//<x:json>");
                    code.AppendLine($"{indent}#line ({node.Line},{node.LinePosition + length + 1})-({node.Line},{node.LinePosition + length + node.InnerHtml.Length}) 1 \"{definition.TemplatePath}\"");
                    code.AppendLine($"{indent}__context.Write(System.Text.Json.JsonSerializer.Serialize({node.InnerText}));");
                    code.AppendLine($"{indent}#line hidden");

                } else if (node.NodeType == HtmlNodeType.Element && node.Name.Equals("x:debugger")) {
                    //<x:debugger></x:debugger>
                    code.AppendLine($"{indent}//<x:debugger></x:debugger>");
                    code.AppendLine($"{indent}#line ({node.Line},{node.LinePosition + 1})-({node.Line},{node.LinePosition + 1 + node.OuterHtml.Length}) 1 \"{definition.TemplatePath}\"");
                    code.AppendLine($"{indent}System.Diagnostics.Debugger.Break();");
                    code.AppendLine($"{indent}#line hidden");

                } else if (node.NodeType == HtmlNodeType.Element && node.Name.StartsWith("x:")) {
                    //error
                    diagnostics.Report(DiagnosticDescriptors.XC1002__HtmlInvalidNode, node, definition.TemplatePath, []);

                } else if (node.NodeType == HtmlNodeType.Element) {
                    //<element>
                    code.AppendLine($"{indent}//{node.Name}");
                    var insideBlock = 0;
                    var isSelfClosing = HTML_SELFCLOSING_TAGS.Contains(node.Name);

                    // x-if start
                    if (node.Attributes.Contains("x-if")) {
                        //...<span x-if="state.value > 0"> bigger than 0</span>...
                        var xAttribute = node.Attributes["x-if"];
                        ifName = $"__i{sequence}";
                        code.AppendLine($"{indent}#line ({xAttribute.Line},{xAttribute.LinePosition + 1 + xAttribute.Name.Length + 2})-({xAttribute.Line},{xAttribute.LinePosition + 1 + xAttribute.Name.Length + xAttribute.Value.Length + 2 - 1}) 1 \"{definition.TemplatePath}\"");
                        code.AppendLine($"{indent}var {ifName} = XComponents.Converter.ToBoolean({xAttribute.DeEntitizeValue});");
                        code.AppendLine($"{indent}#line hidden");
                        code.AppendLine($"{indent}if ({ifName}) {{");
                        level++;
                        insideBlock++;
                        indent = new String(' ', (level + baseLevel) * 4);
                        node.Attributes.Remove(xAttribute);

                    } else if (node.Attributes.Contains("x-elseif")) {
                        //...<span x-if="state.value > 0"> less than 0</span>...
                        var xAttribute = node.Attributes["x-elseif"];
                        code.AppendLine($"{indent}if (!{ifName}) {{");
                        code.AppendLine($"{indent}    #line ({xAttribute.Line},{xAttribute.LinePosition + 1 + xAttribute.Name.Length + 2})-({xAttribute.Line},{xAttribute.LinePosition + 1 + xAttribute.Name.Length + xAttribute.Value.Length + 2 - 1}) 1 \"{definition.TemplatePath}\"");
                        code.AppendLine($"{indent}    {ifName} = XComponents.Converter.ToBoolean({xAttribute.DeEntitizeValue});");
                        code.AppendLine($"{indent}    #line hidden");
                        code.AppendLine($"{indent}    if ({ifName}) {{");
                        level += 2;
                        insideBlock += 2;
                        indent = new String(' ', (level + baseLevel) * 4);
                        node.Attributes.Remove(xAttribute);

                    } else if (node.Attributes.Contains("x-else")) {
                        //...<span x-else>else</span>...
                        var xAttribute = node.Attributes["x-else"];
                        code.AppendLine($"{indent}if (!{ifName}) {{");
                        level++;
                        insideBlock++;
                        indent = new String(' ', (level + baseLevel) * 4);
                        node.Attributes.Remove(xAttribute);
                    }

                    // x-for start
                    if (node.Attributes.Contains("x-for")) {
                        //...<li x-for="item in state.items" ...
                        //...<li x-for="(item,index) in state.items" ...
                        //...<li x-for="item in 10" ...
                        //...<li x-for="(item,index) in 10" ...
                        //...<li x-for="key in state.dictionary" ...
                        //...<li x-for="(key, index) in state.dictionary" ...
                        //...<li x-for="(key, index, value) in state.dictionary" ...
                        var xAttribute = node.Attributes["x-for"];
                        var forName = $"__for{sequence}";
                        var forArray = xAttribute.DeEntitizeValue.Replace(" in ", "#").Trim().Split(['#'], StringSplitOptions.RemoveEmptyEntries);
                        if (forArray.Length != 2) {
                            diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, [$"Error compiling component {definition.ComponentName}: invalid x-for expression ${xAttribute.DeEntitizeValue}"]);
                        } else {
                            var forItem = forArray[0];
                            var forIndex = forName + "_index";
                            var forValue = forName + "_value";
                            var forExpression = forArray[forArray.Length - 1];
                            if (forItem.StartsWith("(") && forItem.EndsWith(")")) {
                                var forVariableArray = forItem.Substring(1, forItem.Length - 2).Split(',');
                                forItem = forVariableArray[0].Trim();
                                if (forVariableArray.Length > 1) forIndex = forVariableArray[1].Trim();
                                if (forVariableArray.Length > 2) forValue = forVariableArray[2].Trim();
                            }
                            code.AppendLine($"{indent}#line ({xAttribute.Line},{xAttribute.LinePosition + 1 + xAttribute.Name.Length + 2})-({xAttribute.Line},{xAttribute.LinePosition + 1 + xAttribute.Name.Length + xAttribute.Value.Length + 2 - 1}) 1 \"{definition.TemplatePath}\"");
                            code.AppendLine($"{indent}var {forName} = XComponents.Converter.ToEnumerable({forExpression});");
                            code.AppendLine($"{indent}#line hidden");
                            code.AppendLine($"{indent}foreach (var ({forItem}, {forIndex}, {forValue}) in {forName}) {{");
                            level++;
                            insideBlock++;
                            indent = new String(' ', (level + baseLevel) * 4);
                            node.Attributes.Remove(xAttribute);
                        }
                    }

                    // x-text
                    if (node.Attributes.Contains("x-text")) {
                        //...<span x-text="state.value"></span>...
                        var xAttribute = node.Attributes["x-text"];
                        code.AppendLine($"{indent}__context.WriteHtml({xAttribute.DeEntitizeValue});");
                        node.Attributes.Remove(xAttribute);
                    }

                    // x-html
                    if (node.Attributes.Contains("x-html")) {
                        //...<span x-html="state.value"></span>...
                        var xAttribute = node.Attributes["x-html"];
                        code.AppendLine($"{indent}__context.Write({xAttribute.DeEntitizeValue});");
                        node.Attributes.Remove(xAttribute);
                    }
                    // x-model
                    if (node.Attributes.Contains("x-model")) {
                        //...<input type="text" x-model="state.name"/>... 
                        // ignores it in SSR
                        var xAttribute = node.Attributes["x-model"];
                        node.Attributes.Remove(xAttribute);
                    }

                    // x-once
                    if (node.Attributes.Contains("x-once")) {
                        //...<input type="text" x-model="state.name"/>... 
                        var xAttribute = node.Attributes["x-once"];
                        node.Attributes.Remove(xAttribute);
                    }

                    // x-on
                    foreach (var attribute in node.Attributes.ToArray()) {
                        if (attribute.Name.StartsWith("x-on:") || attribute.Name.StartsWith("@")) {
                            //...<button x-on:click="this.onIncrement(event)">+1</button>...
                            //...<button x-on:click="onIncrement">+1</button>...
                            //...<button @click="onIncrement">+1</button>...
                            node.Attributes.Remove(attribute);
                        }
                    }

                    // node start
                    
                    if (node.Attributes.Contains("x-pre")) {
                        // x-pre: skip children
                        var xAttribute = node.Attributes["x-pre"];
                        node.Attributes.Remove(xAttribute);

                    } else if (node.Name.Equals("slot")) {
                        //slot
                        var slotName = node.GetAttributeValue("name", "");
                        var slotVariableName = "__slot" + sequence;
                        code.AppendLine($"{indent}await __component.RenderSlotAsync(\"{slotName}\", __context);");

                    } else if (node.Name.IndexOf("-") != -1) {
                        // XComponent
                        var compName = "__component" + sequence;
                        code.AppendLine($"{indent}var {compName} = __context.Services.GetKeyedService<XComponent>(\"{node.Name}\");");
                        code.AppendLine($"{indent}if ({compName} == null) throw new Exception($\"XComponent not found: {node.Name}\");");
                        code.AppendLine($"{indent}{compName}.OnInit(__context);");
                        // set attributes  and properties
                        foreach (var attribute in node.Attributes.ToArray()) {
                            if (attribute.Name.StartsWith("x-attr:") || attribute.Name.StartsWith(":")) {
                                // ...<my-component x-attr:title="state.value"></my-component>...
                                // ...<my-component :title="state.value"></my-component>...
                                var attrName = attribute.Name.Substring(attribute.Name.IndexOf(":") + 1);
                                var attrValue = attribute.DeEntitizeValue;
                                code.AppendLine($"{indent}if (!{compName}.SetFromAttribute({Utils.EscapeCsString(attrName)},{attrValue})) __context.Logger.LogWarning(\"Unable to set attribute: {node.Name} has no attribute named: {attrName}\");");
                                node.Attributes.Remove(attribute);
                            } else if (attribute.Name.StartsWith("x-prop:") || attribute.Name.StartsWith(".")) {
                                // ...<my-component x-prop:value="state.value"></my-component>...
                                // ...<my-component .value="state.value"></my-component>...
                                var propName = Utils.KebabToPascalCase((attribute.Name.StartsWith(".") ? attribute.Name.Substring(1) : attribute.Name.Substring(attribute.Name.IndexOf(":") + 1)));
                                var propValue = attribute.DeEntitizeValue;
                                code.AppendLine($"{indent}if (!{compName}.SetFromProperty({Utils.EscapeCsString(propName)}, {propValue.Replace("&quot;", "\"")})) __context.Logger.LogWarning(\"Unable to set attribute: {node.Name} has no property named: {propName}\");");
                                node.Attributes.Remove(attribute);
                            } else if (attribute.Name.Equals("x-show")) {
                                // ...<span x-show="state.value > 0"> bigger than 0</span>...
                                diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, [$"Error compiling component {definition.ComponentName}: invalid node attribute ${attribute.Name} in a server XComponent: not implemented"]);
                            } else {
                                // ...<my-component title="this is a title"></my-component>...
                                code.AppendLine($"{indent}if (!{compName}.SetFromAttribute({Utils.EscapeCsString(attribute.Name)}, {Utils.EscapeCsString(attribute.DeEntitizeValue)})) __context.Logger.LogWarning(\"Unable to set attribute: {node.Name} has no attribute named: {attribute.Name}\");");
                                node.Attributes.Remove(attribute);
                            }
                        }
                        // set named slots
                        foreach (var childNode in node.ChildNodes.ToArray()) {
                            if (childNode.NodeType == HtmlNodeType.Element && childNode.Attributes.Contains("slot")) {
                                // ...<* slot="header">...
                                var slotName = childNode.GetAttributeValue("slot", "");
                                code.AppendLine($"{indent}if (!{compName}.SetFromSlot({Utils.EscapeCsString(slotName)}, async (__context) => {{");
                                CreateRenderFunctionRecursive(definition, diagnostics, code, childNode.ChildNodes, level + 1, ref sequence);
                                code.AppendLine($"{indent}}})) __context.Logger.LogWarning(\"Unable to set slot: {node.Name} has no slot named: {slotName}\");");
                                node.ChildNodes.Remove(childNode);
                            }
                        };
                        // set default slot
                        if (node.ChildNodes.Count > 0) {
                            code.AppendLine($"{indent}if (!{compName}.SetFromSlot(\"\", async (__context) => {{");
                            CreateRenderFunctionRecursive(definition, diagnostics, code, node.ChildNodes, level + 1, ref sequence);
                            code.AppendLine($"{indent}}})) __context.Logger.LogWarning(\"Unable to set slot: {node.Name} has no default slot\");");
                        }
                        // get
                        code.AppendLine($"{indent}await {compName}.OnLoadAsync(__context);");
                        // render
                        code.AppendLine($"{indent}await {compName}.RenderAsync(\"\", __context);");

                    } else if (node.Attributes.Count == 0) {
                        // html element
                        if (isSelfClosing) {
                            // open and close
                            code.AppendLine($"{indent}__context.Write(\"<{node.Name}/>\");");
                        } else {
                            // open element
                            code.AppendLine($"{indent}__context.Write(\"<{node.Name}>\");");
                            // children
                            CreateRenderFunctionRecursive(definition, diagnostics, code, node.ChildNodes, level, ref sequence);
                            // node end
                            code.AppendLine($"{indent}__context.Write(\"</{node.Name}>\");");
                        }

                    } else {
                        // html element
                        code.AppendLine($"{indent}__context.Write(\"<{node.Name}\");");

                        // attributes
                        foreach (var attribute in node.Attributes.ToArray()) {
                            if (attribute.Name.StartsWith("x-attr:") || attribute.Name.StartsWith(":")) {
                                // ...<span x-attr:title="state.value"></span>...
                                // ...<span :title="state.value"></span>...
                                var attrName = attribute.Name.Substring(attribute.Name.IndexOf(":") + 1);
                                code.AppendLine($"{indent}__context.Write(\" {attrName}=\\\"\");");
                                code.AppendLine($"{indent}__context.WriteHtmlAttribute({attribute.Value});");
                                code.AppendLine($"{indent}__context.Write(\"\\\"\");");
                                node.Attributes.Remove(attribute);

                            } else if (attribute.Name.StartsWith("x-prop:") || attribute.Name.StartsWith(".")) {
                                // ...<input x-prop:value="state.value"></input>...
                                //...<input .value="state.value"></input>...
                                node.Attributes.Remove(attribute);

                            } else if (attribute.Name.Equals("x-show")) {
                                // ...<span x-show="state.value > 0"> bigger than 0</span>...
                                var showName = $"__show{sequence}";
                                code.AppendLine($"{indent}#line ({attribute.Line},{attribute.LinePosition + 1 + attribute.Name.Length + 2})-({attribute.Line},{attribute.LinePosition + 1 + attribute.Name.Length + attribute.Value.Length + 2 - 1}) 1 \"{definition.TemplatePath}\"");
                                code.AppendLine($"{indent}var {showName} = (!XComponents.Converter.ToBoolean({attribute.DeEntitizeValue}));");
                                code.AppendLine($"{indent}#line hidden");
                                code.AppendLine($"{indent}if ({showName}) {{");
                                code.AppendLine($"{indent}    __context.Write(\" style=\\\"display:none;\\\"\");");
                                code.AppendLine($"{indent}}}");
                                node.Attributes.Remove(attribute);

                            } else {
                                // ...<span title="this is a title"></span>...
                                code.AppendLine($"{indent}__context.Write(\" {attribute.Name}=\\\"{attribute.DeEntitizeValue.Replace("\"", "&quot;")}\\\"\");");
                            }
                        }

                        // node start end
                        if (isSelfClosing) {
                            code.AppendLine($"{indent}__context.Write(\" />\");");
                        } else {
                            code.AppendLine($"{indent}__context.Write(\">\");");
                            // add children
                            if (node.Attributes.Contains("x-pre")) {
                                code.AppendLine($"{indent}__context.Write({Utils.EscapeCsString(node.InnerHtml)});");
                            } else {
                                CreateRenderFunctionRecursive(definition, diagnostics, code, node.ChildNodes, level, ref sequence);
                            }

                            // node end
                            code.AppendLine($"{indent}__context.Write(\"</{node.Name}>\");");
                        }
                    }

                    // block end
                    while (insideBlock > 0) {
                        insideBlock--;
                        level--;
                        indent = new String(' ', (level + baseLevel) * 4);
                        code.AppendLine($"{indent}}}");
                    }

                    // check if is there a remaining invalid x attribute
                    if (!node.Attributes.Contains("x-pre")) {
                        foreach (var attributeName in node.Attributes.Select(x => x.Name)) {
                            if (attributeName.StartsWith("x-")) {
                                diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, [$"Error compiling component {definition.ComponentName}: invalid node attribute ${attributeName}: not implemented"]);
                            }
                        }
                    }
                }
                // inc index
                index++;
            }
            
        } 
        public void ReportError(GeneratorExecutionContext context, HtmlNode node, string templatePath, DiagnosticDescriptor diagnosticDescriptor, string message) {
            var line = node.Line - 1;
            var column = node.LinePosition;
            var position = node.StreamPosition;
            var location = Location.Create(templatePath, new TextSpan(position, 1), new LinePositionSpan(new LinePosition(line, column), new LinePosition(line, column + 1)));
            context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, location, templatePath, message));

        }
    } 


}







