using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Permissions;
using System.Text;

using HtmlAgilityPack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XComponents.SourceGenerator {

    public class RenderFunctionXClient {


        // Vars
        private readonly string[] HTML_SELFCLOSING_TAGS = ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr", "", ""];

        // Methods
        public string CreateRenderFunction(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, string js, HtmlDocument document) {
            var code = new StringBuilder();
            code.AppendLine("    render(state, __context) {");
            code.AppendLine("            var _ifs = {};");
            code.AppendLine("            return [");
            var index = 0;
            foreach (var node in document.DocumentNode.ChildNodes) {
                index += CreateRenderFunctionRecursive(definition, diagnostics, "", node, index, code, 0, new());
            }
            code.AppendLine("            ];");
            code.AppendLine("        }");
            return js.Insert(js.LastIndexOf("})"), code.ToString() + "    ");
        }
        public int CreateRenderFunctionRecursive(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, string name, HtmlNode node, int index, StringBuilder js, int level, List<string> eventVariables) {
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
                    js.AppendLine(indent + "{tag:\"#text\", index:" + (index + inc++) + ", children:" + Utils.EscapeCsString(pre) + "},");
                    var k = text.IndexOf("}}", j);
                    if (k == -1) {
                        //error
                        diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid node {node.Name}: not implemented");
                        break;
                    }
                    //expression
                    var expression = text.Substring(j + 2, k - j - 2);
                    //write value
                    js.AppendLine(indent + "{tag:\"#comment\", index:" + (index + inc++) + ", children:\"x:text\"},");
                    js.AppendLine(indent + "{tag:\"#text\", index:" + (index + inc++) + ", children:" + expression + "},");
                    js.AppendLine(indent + "{tag:\"#comment\", index:" + (index + inc++) + ", children:\"/x:text\"},");
                    i = k + 2;
                    j = text.IndexOf("{{", i);
                }
                //post text
                var post = text.Substring(i);
                js.AppendLine(indent + "{tag:\"#text\", index:" + (index + inc++) + ", children:" + Utils.EscapeCsString(post) + "},");
            } else if (node.NodeType == HtmlNodeType.Comment) {
                //#comment
                var textContent = node.InnerText;
                var jsLine = new StringBuilder();
                jsLine.Append(indent);
                jsLine.Append("{tag:\"#comment\", index:" + (index + inc++) + ", children:\"aaaa\" + " + Utils.EscapeCsString(textContent) + "},");
                js.AppendLine(jsLine.ToString());
            } else if (node.Name == "x:text") {
                //x:text
                var jsLine = new StringBuilder();
                jsLine.Append(indent);
                jsLine.Append("{tag:\"#text\", index:" + (index + inc++) + ", children: \"\" + " + node.InnerText + "},");
                js.AppendLine(jsLine.ToString());
            } else if (node.Name.StartsWith("x:")) {
                //error
                diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid node {node.Name}: not implemented");
            } else if (node.Name == "script" && node.GetAttributeValue("type","")=="module") {
                //module script (ignores it)                
            } else {
                //element
                var jsLine = new List<string>();
                jsLine.Add(indent);
                jsLine.Add("{tag:\"" + node.Name.ToLower() + "\"");
                var jsPostLine = new List<string>();
                var jsPost = new List<string>();
                var attrs = new List<string>();
                var props = new List<string>();
                var events = new List<string>();
                var options = new List<string>();
                var text = "";
                var wrapChildNodes = false;
                var eventVariable = "";
                options.Add("index:" + index);
                foreach(var attr in node.GetAttributes()) {                    
                    if (attr.Name == "x-text") {
                        //...<span x-text="state.value"></span>...
                        if (node.ChildNodes.Count > 0) diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid x-text node: non empty");
                        text = "\"\" + " + attr.Value;
                    } else if (attr.Name == "x-html") {
                        //...<span x-html="state.value"></span>...
                        if (node.ChildNodes.Count > 0) diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling component {name}: invalid x-html node: non empty");
                        options.Append("format:\"html\"");
                        text = "\"\" + " + attr.Value;
                    } else if (attr.Name == "x-attr" || attr.Name == ":") {
                        //...<span x-attr="state.value"></span>...
                        //...<span :="state.value"></span>...
                        var attrValue = attr.Value;
                        attrs.Add("...__context.toObject(" + attrValue + ")");
                    } else if (attr.Name.StartsWith("x-attr:") || attr.Name.StartsWith(":")) {
                        //...<span x-attr:title="state.value"></span>...
                        var attrName = (attr.Name.StartsWith(":") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1));
                        var attrValue = attr.Value;
                        if (attrName.StartsWith("[") && attrName.EndsWith("]")) {
                            attrs.Add("...__context.toDynamicArgument(" + attrName.Substring(1, attrName.Length - 1) + ", " + attrValue + ")");
                        } else {
                            attrs.Add('"' + attrName + "\":" + attrValue);
                        }
                    } else if (attr.Name == "x-prop" || attr.Name == ".") {
                        //...<span x-prop="state.value"></span>...
                        //...<span .="state.value"></span>...
                        var propValue = attr.Value;
                        props.Add("..." + propValue + "");
                    } else if (attr.Name.StartsWith("x-prop:") || attr.Name.StartsWith(".")) {
                        //...<input x-prop:value="state.value"></input>...
                        //...<input .value="state.value"></input>...
                        var propName = Utils.KebabToCamalCase((attr.Name.StartsWith(".") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1)));
                        var propValue = attr.Value;
                        if (propName.StartsWith("[") && propName.EndsWith("]")) {
                            props.Add("...__context.toDynamicProperty(" + propName.Substring(1, propName.Length - 1) + ", " + propValue + ")");
                        } else {
                            props.Add(propName + ":" + propValue);
                        }
                    } else if (attr.Name.StartsWith("x-on:") || attr.Name.StartsWith("@")) {
                        //...<button x-on:click="this.onIncrement(event)">+1</button>...
                        //...<button x-on:click="onIncrement">+1</button>...
                        //...<button @click="onIncrement">+1</button>...
                        var eventName = (attr.Name.StartsWith("@") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1));
                        var eventHandler = attr.Value;
                        //events.Add("'" + eventName + "': (event) => this." + eventHandler + "(event" + string.Join("", eventVariables.Select(x => ", " + x)) + ")");
                        events.Add("'" + eventName + "': ['" + eventHandler + "'" + string.Join("", eventVariables.Select(x => ", " + x)) + "]");
                    } else if (attr.Name == "x-if") {
                        //...<span x-if="state.value > 0">greather than 0</span>...
                        jsLine[0] = indent + "//x-if " + attr.Value + "\n" + indent + "...((_ifs.c" + level + " = (" + attr.Value + ")) ? [";
                        jsPostLine.Add("] : [{tag:\"#comment\", index:" + (index) + ", children:\"x-if " + attr.Value + "\"}]),");
                    } else if (attr.Name == "x-elseif") {
                        //...<span x-elseif="state.value < 0">less than 0</span>...
                        jsLine[0] = indent + "//x-elseif " + attr.Value + "\n" + indent + "...(_ifs.c" + level + " ? [{tag:\"#comment\", index:" + (index) + ", children:\"x-elseif " + attr.Value + "\"}] : (_ifs.c" + level + " = (" + attr.Value + ")) ? [";
                        jsPostLine.Add("] : [{tag:\"#comment\", index:" + (index) + ", children:\"x-elseif " + attr.Value + "\"}]),");
                    } else if (attr.Name == "x-else") {
                        //...<span x-else>is 0</span>...
                        jsLine[0] = indent + "//x-else\n" + indent + "...(!_ifs.c" + level + " ? [";
                        jsPostLine.Add("] : [{tag:\"#comment\", index:" + (index) + ", children:\"x-else\"}]),");
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
                        js.AppendLine(indent + "//x-for\n" + indent + "{tag:\"#comment\", index:" + (index) + ", forType:'" + forType + "', children:'x-for-start'},");
                        //add loop
                        var parts = attr.Value.Replace(" in ","|").Split('|');
                        if (parts.Length != 2) diagnostics.Report(DiagnosticDescriptors.XC1003__HtmlSyntaxError, node, definition.TemplatePath, $"Error compiling template: invalid x-for attribute detected: {attr.Value}");
                        var itemName = parts[0].Trim();
                        var indexName = "index";
                        if (itemName.StartsWith("(") && itemName.EndsWith(")")) {
                            var arr = itemName.Substring(1, itemName.Length - 1).Split(',');
                            itemName = arr[0].Trim();
                            indexName = arr[1].Trim();
                        }
                        var listName = parts[1].Trim();
                        jsLine[0] = indent + "...(__context.toArray(" + listName + ").map((" + itemName + ", " + indexName + ") => (";
                        jsPostLine.Add(")),");
                        if (!string.IsNullOrEmpty(keyName)) options.Add("\"key\":" + itemName + "." + keyName);
                        //creates a comment end node that indicates the end of the for loop
                        jsPost.Add(indent + "{tag:\"#comment\", index:" + (index) + ", forType:'" + forType + "', children:'x-for-end'},\n");
                        jsPost.Add(indent + "// /x-for\n");
                        wrapChildNodes = true;
                        eventVariable = itemName;
                    } else if (attr.Name == "x-key") {
                        //used in x-for
                    } else if (attr.Name == "x-show") {
                        //...<span x-show="state.value > 0">greather than 0</span>...
                        attrs.Add("...(" + (attr.Value) + " ? null : {style:'display:none'})");
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
                        props.Add(propertyName + ":" + propValue);
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
                        events.Add("'" + eventName + "': (event) => { " + propValue + " = event.target." + propertyName + "; this.invalidate(); }");

                    } else if (attr.Name == "x-once") {
                        //x-once: only render once
                        jsLine[0] = indent + "...((__context.renderCount==0) ? [";
                        jsPostLine.Add("] : [{tag:'" + node.Name.ToLower() + "', once:true}]),");
                    } else if (attr.Name == "x-pre") {
                        //v-pre: skip childs
                        options.Add("format:\"html\"");
                        text = Utils.EscapeCsString(node.InnerHtml).Replace("<x:text>", "{{").Replace("</x:text>", "}}");
                    } else if (attr.Name.StartsWith("x-")) {
                        //error
                        diagnostics.Report(DiagnosticDescriptors.XC1004__HtmlInvalidAttribute, attr, definition.TemplatePath, $"Error compiling template: invalid template attribute detected: {attr.Name}");
                    } else {
                        attrs.Add("'" + attr.Name + "':" + Utils.EscapeCsString(attr.Value));
                    }
                }
                //attrs + props + events + options
                if (attrs.Count > 0) jsLine.Add(", attrs:" +'{' + String.Join(", ", attrs.ToArray()) + '}');
                if (props.Count > 0) jsLine.Add(", props:" + '{' + String.Join(", ", props.ToArray()) + '}' );
                if (events.Count > 0) jsLine.Add(", events:" + '{' + String.Join(", ", events.ToArray()) + '}');
                if (options.Count > 0) jsLine.Add(", " + String.Join(", ", options.ToArray()));
                //add event variable
                if (!string.IsNullOrEmpty(eventVariable)) {
                    eventVariables.Add(eventVariable);
                }
                //children
                if (!string.IsNullOrEmpty(text)) {
                    jsLine.Add(", children:" + text + "");
                    jsLine.Add("}," + (jsPostLine.Count > 0 ? String.Join("", jsPostLine) : ""));
                    js.Append(string.Join("", jsLine.ToArray()));
                } else if (node.ChildNodes.Count > 0 || !string.IsNullOrEmpty(node.GetAttributeValue("x-for", ""))) {
                    jsLine.Add(", children:[\n");
                    js.Append(string.Join("", jsLine.ToArray()));
                    var subIndex = 0;
                    node.ChildNodes.ToList().ForEach((subNode) => {                        
                        subIndex += CreateRenderFunctionRecursive(definition, diagnostics, name, subNode, subIndex, js, level + 1, eventVariables);
                    });                    
                    js.AppendLine(indent + "]}" + (wrapChildNodes ? ")" : "") + "," + (jsPostLine.Count > 0 ? String.Join("", jsPostLine) : ""));
                } else {
                    jsLine.Add("}," + (jsPostLine.Count > 0 ? String.Join("", jsPostLine) : "") + "\n");
                    js.Append(string.Join("", jsLine.ToArray()));
                }
                if (jsPost.Count > 0) js.Append(string.Join("", jsPost.ToArray()));
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




// filters: state.Value | capitalize | uppercase ...


