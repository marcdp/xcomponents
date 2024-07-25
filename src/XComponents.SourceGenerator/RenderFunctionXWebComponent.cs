using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Permissions;
using System.Text;

using HtmlAgilityPack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XComponents.SourceGenerator {

    public class RenderFunctionXWebComponent {


        // Methods
        public string CreateRenderServer(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, HtmlDocument document) {
            var code = new StringBuilder();
            //result
            var result = new StringBuilder();
            result.AppendLine($"        protected override VNode[]? RenderVNodes(string __slot, XContext __context) {{");
            result.AppendLine("            var state = new State(this);");
            //named slots
            var slotNames = new List<string>();
            foreach (var node in document.DocumentNode.ChildNodes) {
                if (node.NodeType == HtmlNodeType.Element && node.Attributes.Contains("slot")) {
                    var slotName = node.GetAttributeValue("slot", "");
                    if (!slotNames.Contains(slotName)) slotNames.Add(slotName);
                }
            }
            foreach (var slotName in slotNames) {
                var index = 0;
                var slotCode = new StringBuilder();
                var slotVariables = new List<string>();
                foreach (var node in document.DocumentNode.ChildNodes) {
                    if (node.NodeType == HtmlNodeType.Element && node.Attributes.Contains("slot") && node.GetAttributeValue("slot", "") == slotName) {
                        index += CreateRenderFunctionRecursive(definition, diagnostics, "", node, index, slotCode, 1, slotVariables, new());
                    }
                }
                result.AppendLine("            if (__slot.Equals(\"" + slotName + "\")) {");
                if (slotVariables.Count > 0) result.AppendLine("                " + string.Join("\n                ", slotVariables.ToArray()));
                result.AppendLine("                return [");
                result.Append(slotCode);
                result.AppendLine("                ];");
                result.AppendLine("            }");
            }
            //un-slotted content
            if (true) {
                var index = 0;
                var slotCode = new StringBuilder();
                var slotVariables = new List<string>();                
                foreach (var node in document.DocumentNode.ChildNodes) {
                    if (node.NodeType == HtmlNodeType.Element && node.Attributes.Contains("slot")) {
                    } else {
                        index += CreateRenderFunctionRecursive(definition, diagnostics, "", node, index, code, 1, slotVariables, new());
                    }
                }
                result.AppendLine("            if (__slot.Equals(\"\")) {");
                if (slotVariables.Count > 0) result.AppendLine("                " + string.Join("\n                ", slotVariables.ToArray()));
                result.AppendLine("                return [");
                result.Append(code);
                result.AppendLine("                ];");
                result.AppendLine("            }");
            }
            result.AppendLine("            return null;");
            result.AppendLine("        }");
            //return
            return result.ToString();
        }
        public string CreateRenderClient(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, string js, HtmlDocument document) {
            var code = new StringBuilder();
            var index = 0;
            var varDefinitions = new List<string>();
            var codeInternal = new StringBuilder();
            if (js.Length == 0) {
                js = $"import XWebComponent from \"x-web-component\";\nXWebComponent.define(\"{definition.ComponentName}\", class extends XWebComponent {{\n}})\n";
            }
            foreach (var node in document.DocumentNode.ChildNodes) {
                index += CreateRenderFunctionRecursive(definition, diagnostics, "", node, index, codeInternal, 0, varDefinitions, new());
            }
            code.AppendLine("    render(__context, state) {");
            if (varDefinitions.Count > 0) {
                code.AppendLine("            " + string.Join("\n            ", varDefinitions.ToArray()));
            }
            code.AppendLine("            return [");
            code.Append(codeInternal);
            code.AppendLine("            ];");
            code.AppendLine("        }");
            code = code.Replace(".. XComponents.Converter.Enumerate(", "... __context.enumerate(");
            code = code.Replace("new VNode(", "__context.vNode(");
            return js.Insert(js.LastIndexOf("})"), code.ToString() + "    ");
        }
        public int CreateRenderFunctionRecursive(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, string name, HtmlNode node, int index, StringBuilder code, int level, List<string> varDefinitions, List<string> eventVariables) {
            var baseLevel = 4;
            var indent = new String(' ', (level + baseLevel) * 4);
            var inc = 0;
            if (node.NodeType == HtmlNodeType.Text) {
                //#text
                var text = node.InnerText;
                var i = 0;
                var j = text.IndexOf("{{", i);
                while (j != -1) {
                    //pre text
                    var pre = text.Substring(i, j - i);
                    code.AppendLine(indent + "new VNode(\"#text\", " + (index + inc++) + ").Text(" + Utils.EscapeCsString(pre) + "),");
                    var k = text.IndexOf("}}", j);
                    if (k == -1) {
                        //error
                        diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid node {node.Name}: not implemented");
                        break;
                    }
                    //expression
                    var expression = text.Substring(j + 2, k - j - 2);
                    //write value
                    code.AppendLine(indent + "new VNode(\"#comment\", " + (index + inc++) + ").Text(\"<!-- x:text -->\"),");
                    code.AppendLine(indent + "new VNode(\"#text\", " + (index + inc++) + ").Text(" + expression + "),");
                    code.AppendLine(indent + "new VNode(\"#comment\", " + (index + inc++) + ").Text(\"<!-- /x:text -->\"),");
                    i = k + 2;
                    j = text.IndexOf("{{", i);
                }
                //post text
                var post = text.Substring(i);
                code.AppendLine(indent + "new VNode(\"#text\", " + (index + inc++) + ").Text(" + Utils.EscapeCsString(post) + "),");
            } else if (node.NodeType == HtmlNodeType.Comment) {
                //#comment
                var textContent = node.OuterHtml;
                var csLine = new StringBuilder();
                csLine.Append(indent);
                csLine.Append("new VNode(\"#comment\", " + (index + inc++) + ").Text(" + Utils.EscapeCsString(textContent) + "),");
                code.AppendLine(csLine.ToString());
            } else if (node.Name == "x:text") {
                //x:text
                var csLine = new StringBuilder();
                csLine.Append(indent);
                csLine.Append("new VNode(\"#text\", " + (index + inc++) + ").Text(\"\" + " + node.InnerText + "),");
                code.AppendLine(csLine.ToString());
            } else if (node.Name.StartsWith("x:")) {
                //error
                diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid node {node.Name}: not implemented");
            } else if (node.Name == "script" && node.GetAttributeValue("type","")=="module") {
                //module script (ignores it)
            } else {
                //element
                var csLine = new List<string>();
                csLine.Add(indent);
                csLine.Add("new VNode(\"" + node.Name.ToLower() + "\", " + index + ")");
                var csPost = new List<string>();
                var text = "";
                var eventVariable = "";
                foreach(var attr in node.GetAttributes()) {
                    //if (attr.Name == "slot") {
                        //...<span slot="..."></span> ...

                    if (attr.Name == "x-text") {
                        //...<span x-text="state.value"></span>...
                        if (node.ChildNodes.Count > 0) diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid x-text node: non empty");
                        text = "\"\" + " + attr.Value;

                    } else if (attr.Name == "x-html") {
                        //...<span x-html="state.value"></span>...
                        if (node.ChildNodes.Count > 0) diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid x-html node: non empty");
                        csLine.Add(".Option(\"format\",\"html\")");
                        text = "\"\" + " + attr.Value;

                    } else if (attr.Name == "x-attr" || attr.Name == ":") {
                        //...<span x-attr="state.value"></span>...
                        //...<span :="state.value"></span>...
                        var attrValue = attr.Value;
                        csLine.Add(".ExpandAttributes(" + attrValue + ")");

                    } else if (attr.Name.StartsWith("x-attr:") || attr.Name.StartsWith(":")) {
                        //...<span x-attr:title="state.value"></span>...
                        var attrName = (attr.Name.StartsWith(":") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1));
                        var attrValue = attr.Value;
                        if (attrName.StartsWith("[") && attrName.EndsWith("]")) {
                            csLine.Add(".Attribute(State." + attrName.Substring(1, attrName.Length - 1) + "," + attrValue + ")");
                        } else {
                            csLine.Add(".Attribute(\"" + attrName + "\"," + attrValue + ")");
                        }

                    } else if (attr.Name == "x-prop" || attr.Name == ".") {
                        //...<span x-prop="state.value"></span>...
                        //...<span .="state.value"></span>...
                        var propValue = attr.Value;
                        csLine.Add(".ExpandProperties(" + propValue + ")");

                    } else if (attr.Name.StartsWith("x-prop:") || attr.Name.StartsWith(".")) {
                        //...<input x-prop:value="state.value"></input>...
                        //...<input .value="state.value"></input>...
                        var propName = Utils.KebabToCamalCase((attr.Name.StartsWith(".") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1)));
                        var propValue = attr.Value;
                        if (propName.StartsWith("[") && propName.EndsWith("]")) {
                            csLine.Add(".Property(State." + propName.Substring(1, propName.Length - 1) + "," + propValue + ")");
                        } else {
                            csLine.Add(".Property(\"" + propName  + "\"," + propValue + ")");
                        }

                    } else if (attr.Name.StartsWith("x-on:") || attr.Name.StartsWith("@")) {
                        //...<button x-on:click="this.onIncrement(event)">+1</button>...
                        //...<button x-on:click="onIncrement">+1</button>...
                        //...<button @click="onIncrement">+1</button>...
                        var eventName = (attr.Name.StartsWith("@") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1));
                        var eventHandler = attr.Value;
                        csLine.Add(".Event(\"" + eventName + "\", \"" + eventHandler + "\"" + string.Join("", eventVariables.Select(x => ", " + x).Reverse()) + ")");

                    } else if (attr.Name == "x-if") {
                        //...<span x-if="state.value > 0">greather than 0</span>...
                        csLine.Insert(0, indent + "// x-if\n");
                        csLine.Insert(1, indent + "((_ifs" + level + " = (" + attr.Value + ")) ? \n"); 
                        csLine.Insert(2, "    ");
                        csPost.Add(indent + ":");
                        csPost.Add(indent + "    new VNode(\"#comment\", " + index + ").Text(\"<!-- x-if " + attr.Value + " -->\")");
                        csPost.Add(indent + "),");

                        var varDefinition = "var _ifs" + level + " = false;";
                        if (!varDefinitions.Contains(varDefinition)) varDefinitions.Add(varDefinition);
                    } else if (attr.Name == "x-elseif") {
                        //...<span x-elseif="state.value < 0">less than 0</span>...
                        csLine.Insert(0, indent + "// x-elseif\n");
                        csLine.Insert(1, indent + "((!_ifs" + level + " && (_ifs" + level + " = (" + attr.Value + "))) ? \n");
                        csLine.Insert(2, "    ");
                        csPost.Add(indent + ":");
                        csPost.Add(indent + "    new VNode(\"#comment\", " + index + ").Text(\"<!-- x-elseif " + attr.Value + " -->\")");
                        csPost.Add(indent + "),");

                    } else if (attr.Name == "x-else") {
                        //...<span x-else>is 0</span>...
                        csLine.Insert(0, indent + "// x-else\n");
                        csLine.Insert(1, indent + "(!_ifs" + level + " ? \n");
                        csLine.Insert(2, "    ");
                        csPost.Add(indent + ":");
                        csPost.Add(indent + "    new VNode(\"#comment\", " + index + ").Text(\"<!-- x-else -->\")");
                        csPost.Add(indent + "),");

                    } else if (attr.Name == "x-for") {
                        //...<li x-for="item in state.items" x-key="name">{{item.name + '(' + item.count + ') '}}</li>...
                        var keyName = node.GetAttributeValue("x-key", "");
                        var forType = "";
                        if (!string.IsNullOrEmpty(keyName)) {
                            forType = "key";
                        } else {
                            forType = "position";
                        }
                        //creates a comment virtual node that indicates the start of the for loop, and the type of the loop
                        code.AppendLine(indent + "// x-for");
                        code.AppendLine(indent + "new VNode(\"#comment\", " + index + ").Option(\"forType\", \"" + forType + "\").Text(\"<!-- x-for-start -->\"),");
                        //add loop
                        var parts = attr.Value.Replace(" in ","|").Split('|');
                        if (parts.Length != 2) diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling template: invalid x-for attribute detected: {attr.Value}");
                        var itemName = parts[0].Trim();
                        var indexName = "index";
                        var valueName = "value";
                        if (itemName.StartsWith("(") && itemName.EndsWith(")")) {
                            var arr = itemName.Substring(1, itemName.Length - 2).Split(',');
                            itemName = arr[0].Trim();
                            indexName = arr[1].Trim();
                            if (arr.Length > 2) valueName = arr[2].Trim();
                        }
                        var listName = parts[1].Trim();
                        csLine.Insert(0, indent + ".. XComponents.Converter.Enumerate(" + listName + ", (" + itemName + ", " + indexName + ", " + valueName + ") =>\n");
                        csLine.Insert(1, "    ");
                        if (!string.IsNullOrEmpty(keyName)) {
                            csLine.Add(".Option(\"key\",\"" + itemName + "." + keyName + "\")");
                        }
                        //creates a comment end node that indicates the end of the for loop
                        csPost.Add(indent + "),");
                        csPost.Add(indent + "new VNode(\"#comment\", " + index + ").Option(\"forType\", \"" + forType + "\").Text(\"<!-- x-for-end -->\"),");
                        csPost.Add(indent + "// /x-for");
                        eventVariable = itemName;

                    } else if (attr.Name == "x-key") {
                        //used in x-for 

                    } else if (attr.Name == "x-show") {
                        //...<span x-show="state.value > 0">greather than 0</span>...
                        // todo....

                    } else if (attr.Name == "x-model") {
                        //...<input type="text" x-model="state.name"/>... --> <input type="text" x-prop:value="state.name" x-on:change="state.name = event.target.value"/>
                        //prop
                        var nodeName = node.Name.ToLower();
                        var propertyName = "value";
                        var propValue = attr.Value;
                        if (nodeName == "input") { 
                            var type = node.GetAttributeValue("type", "");
                            if (type == "range") {
                                propertyName = "valueAsNumber";
                            } else if (type == "checkbox") {
                                propertyName = "checked";
                                } else if (type == "radio") {
                                propertyName = "checked";
                                propValue += "=='" + node.GetAttributeValue("value", "") + "'";
                            }
                        } else if (nodeName == "select") {
                        } else if (nodeName == "textarea") {
                        }
                        csLine.Add(".Property(\"" + propertyName + "\"," + propValue + ")");
                        //event
                        var eventName = "change";
                        if (nodeName == "input") {
                            var type = node.GetAttributeValue("type", "");
                            if (type == "number") {
                                propertyName = "valueAsNumber";
                            } else if (type == "range") {
                                propertyName = "valueAsNumber";
                            } else if (type == "checkbox") {
                                propertyName = "checked";
                                } else if (type == "radio") {
                                propValue = attr.Value;
                                propertyName = "value";
                            }
                        } else if (nodeName == "select") {
                        } else if (nodeName == "textarea") {
                        }
                        //events.Add("'" + eventName + "': (event) => { " + propValue + " = event.target." + propertyName + "; this.invalidate(); }");
                        
                    } else if (attr.Name == "x-once") {
                        //x-once: only render once
                        csLine[0] = indent + ".. ((__context.renderCount==0) ? [";
                        csPost.Add(indent + "] : [{tag:'" + node.Name.ToLower() + "', once:true}]),");
                    } else if (attr.Name == "x-pre") {
                        //v-pre: skip Childs
                        csLine.Add(".Option(\"format\",\"html\")");
                        text = Utils.EscapeCsString(node.InnerHtml).Replace("<x:text>", "{{").Replace("</x:text>", "}}");
                    } else if (attr.Name.StartsWith("x-")) {
                        //error
                        diagnostics.Report(DiagnosticDescriptors.XC1004__HtmlInvalidAttribute, attr, definition.TemplatePath, $"Error compiling template: invalid template attribute detected: {attr.Name}");
                    } else {
                        csLine.Add(".Attribute(\"" + attr.Name + "\"," + Utils.EscapeCsString(attr.Value) + ")");
                    }
                }
                //add event variable
                if (!string.IsNullOrEmpty(eventVariable)) {
                    eventVariables.Add(eventVariable);
                }
                //slots
                if (node.Name.IndexOf("-")!=-1) {
                    foreach (var subNode in node.ChildNodes.ToList()) {
                        if (subNode.Attributes.Contains("slot")) {
                            var aux = (node.ChildAttributes("x-if").Count() > 0 || node.ChildAttributes("x-elseif").Count() > 0 || node.ChildAttributes("x-else").Count()> 0 || node.ChildAttributes("x-for").Count() > 0 ? 1 : 0);
                            var slotName = subNode.GetAttributeValue("slot", "");
                            csLine.Add(".Slot(\"" + slotName + "\", [\n");
                            code.Append(string.Join("", csLine.ToArray()));
                            var subSubIndex = 0;
                            subSubIndex += CreateRenderFunctionRecursive(definition, diagnostics, name, subNode, subSubIndex, code, level + 1 + aux, varDefinitions, eventVariables);
                            code.Append(indent + (aux > 0 ? "    " : "") + "])");
                            csLine.Clear();
                            node.ChildNodes.Remove(subNode);
                        }
                    }
                }
                //children
                if (!string.IsNullOrEmpty(text)) {
                    csLine.Add(".Text(" + text + "),");
                    code.Append(string.Join("", csLine.ToArray()));
                } else if (node.ChildAttributes("x-if").Count() > 0 || node.ChildAttributes("x-elseif").Count() > 0 || node.ChildAttributes("x-else").Count() > 0) {
                    csLine.Add(".Children([\n");
                    code.Append(string.Join("", csLine.ToArray()));
                    var subIndex = 0;
                    foreach (var subNode in node.ChildNodes) {
                        subIndex += CreateRenderFunctionRecursive(definition, diagnostics, name, subNode, subIndex, code, level + 2, varDefinitions, eventVariables);
                    }
                    code.AppendLine(indent + "    ])");
                } else if (node.ChildAttributes("x-for").Count() > 0) {
                    csLine.Add(".Children([\n");
                    code.Append(string.Join("", csLine.ToArray()));
                    var subIndex = 0;
                    foreach (var subNode in node.ChildNodes) {
                        subIndex += CreateRenderFunctionRecursive(definition, diagnostics, name, subNode, subIndex, code, level + 2, varDefinitions, eventVariables);
                    }
                    code.AppendLine(indent + "    ])");
                } else if (node.ChildNodes.Count > 0) {
                    csLine.Add(".Children([\n");
                    code.Append(string.Join("", csLine.ToArray()));
                    var subIndex = 0;
                    foreach (var subNode in node.ChildNodes) {
                        subIndex += CreateRenderFunctionRecursive(definition, diagnostics, name, subNode, subIndex, code, level + 1, varDefinitions, eventVariables);
                    }
                    code.AppendLine(indent + "]),");
                } else {
                    csLine.Add(",");
                    code.AppendLine(string.Join("", csLine.ToArray()));
                }
                //post
                foreach (var postLine in csPost) {
                    code.AppendLine(postLine);
                }
                //remove event variable
                if (!string.IsNullOrEmpty(eventVariable)) {
                    eventVariables.RemoveAt(eventVariables.Count - 1);
                }
                //inc
                inc++;
            }
            return inc;
        }

    } 

}



