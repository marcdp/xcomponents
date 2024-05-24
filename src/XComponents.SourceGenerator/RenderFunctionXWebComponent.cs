using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;

using HtmlAgilityPack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XComponents.SourceGenerator {

    public class RenderFunctionXWebComponent {


        // Vars
        private readonly string[] HTML_SELFCLOSING_TAGS = ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr", "", ""];

        // Methods
        public string CreateRenderFunction(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, string js, string template) {
            //template = template.Replace("{{", "<x:text>").Replace("}}", "</x:text>").Trim();
            var document = new HtmlDocument();
            document.LoadHtml(template);
            var code = new StringBuilder();
            code.AppendLine("    render(state, renderUtils, renderCount) {");
            code.AppendLine("            var _ifs = {};");
            code.AppendLine("            return [");
            var index = 0;
            foreach (var node in document.DocumentNode.ChildNodes) {
                index += CreateRenderFunctionRecursive("", node, index, code, 0);
            }
            code.AppendLine("            ];");
            code.AppendLine("        }");
            return js.Insert(js.LastIndexOf("})"), code.ToString() + "    ");
        }
        public int CreateRenderFunctionRecursive(string name, HtmlNode node, int index, StringBuilder js, int level) {
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
                    js.AppendLine(indent + "{tag:\"#text\", attrs:{}, props:{}, events:{}, options:{index:" + (index + inc++) + "}, children:" + Utils.EscapeCsString(pre) + "},");
                    var k = text.IndexOf("}}", j);
                    if (k == -1) {
                        //error
                        throw new Exception($"Error compiling component {name}: invalid node {node.Name}: not implemented");
                        break;
                    }
                    //expression
                    var expression = text.Substring(j + 2, k - j - 2);
                    //write value
                    js.AppendLine(indent + "{tag:\"#text\", attrs:{}, props:{}, events:{}, options:{index:" + (index + inc++) + "}, children:" + expression + "},");
                    i = k + 2;
                    j = text.IndexOf("{{", i);
                }
                //post text
                var post = text.Substring(i);
                js.AppendLine(indent + "{tag:\"#text\", attrs:{}, props:{}, events:{}, options:{index:" + (index + inc++) + "}, children:" + Utils.EscapeCsString(post) + "},");
            } else if (node.NodeType == HtmlNodeType.Comment) {
                //#comment
                var textContent = node.InnerText;
                var jsLine = new StringBuilder();
                jsLine.Append(indent);
                jsLine.Append("{tag:\"#comment\", attrs:{}, props:{}, events:{}, options:{index:" + index + "}, children:" + Utils.EscapeCsString(textContent) + ")},");
                js.AppendLine(jsLine.ToString());
                inc++;
            } else if (node.Name == "x:text") {
                //x:text
                var jsLine = new StringBuilder();
                jsLine.Append(indent);
                jsLine.Append("{tag:\"#text\", attrs:{}, props:{}, events:{}, options:{index:" + index + "}, children: \"\" + " + node.InnerText + "},");
                js.AppendLine(jsLine.ToString());
                inc++;
            } else if (node.Name.StartsWith("x:")) {
                //error
                //logger.error("Error compiling component ${name}: invalid node ${node.localName}: not implemented");
                throw new Exception($"Error compiling component {name}: invalid node {node.Name}: not implemented");
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
                options.Add("index:" + index);
                foreach(var attr in node.GetAttributes()) {
                    if (attr.Name == "x-text") {
                        //...<span x-text="state.value"></span>...
                        //if (node.ChildNodes.length) logger.error("Error compiling component ${name}: invalid x-text node: non empty");
                        if (node.ChildNodes.Count > 0) throw new Exception($"Error compiling component {name}: invalid x-text node: non empty");
                        text = "\"\" + " + attr.Value;
                    } else if (attr.Name == "x-html") {
                        //...<span x-html="state.value"></span>...
                        //if (node.childNodes.length) logger.error("Error compiling component ${name}: invalid x-html node: non empty");
                        if (node.ChildNodes.Count > 0) throw new Exception($"Error compiling component {name}: invalid x-html node: non empty");
                        options.Append("format:\"html\"");
                        text = "\"\" + " + attr.Value;
                    } else if (attr.Name == "x-attr" || attr.Name == ":") {
                        //...<span x-attr="state.value"></span>...
                        //...<span :="state.value"></span>...
                        var attrValue = attr.Value;
                        attrs.Add("...renderUtils.toObject(" + attrValue + ")");
                    } else if (attr.Name.StartsWith("x-attr:") || attr.Name.StartsWith(":")) {
                        //...<span x-attr:title="state.value"></span>...
                        var attrName = (attr.Name.StartsWith(":") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1));
                        var attrValue = attr.Value;
                        if (attrName.StartsWith("[") && attrName.EndsWith("]")) {
                            attrs.Add("...renderUtils.toDynamicArgument(" + attrName.Substring(1, attrName.Length - 1) + ", " + attrValue + ")");
                        } else {
                            attrs.Add('"' + attrName + "\":" + attrValue);
                        }
                    } else if (attr.Name == "x-prop" || attr.Name == ".") {
                        //...<span x-prop="state.value"></span>...
                        //...<span .="state.value"></span>...
                        var propValue = attr.Value;
                        attrs.Add("..." + propValue + "");
                    } else if (attr.Name.StartsWith("x-prop:") || attr.Name.StartsWith(".")) {
                        //...<input x-prop:value="state.value"></input>...
                        //...<input .value="state.value"></input>...
                        var propName = Utils.KebabToCamalCase((attr.Name.StartsWith(".") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1)));
                        var propValue = attr.Value;
                        if (propName.StartsWith("[") && propName.EndsWith("]")) {
                            props.Add("...renderUtils.toDynamicProperty(" + propName.Substring(1, propName.Length - 1) + ", " + propValue + ")");
                        } else {
                            props.Add(propName + ":" + propValue);
                        }
                    } else if (attr.Name.StartsWith("x-on:") || attr.Name.StartsWith("@")) {
                        //...<button x-on:click="this.onIncrement(event)">+1</button>...
                        //...<button x-on:click="onIncrement">+1</button>...
                        //...<button @click="onIncrement">+1</button>...
                        var eventName = (attr.Name.StartsWith("@") ? attr.Name.Substring(1) : attr.Name.Substring(attr.Name.IndexOf(':') + 1));
                        var eventHandler = attr.Value;
                        if (eventHandler.IndexOf("(") == -1 && eventHandler.IndexOf(")") == -1 && eventHandler.IndexOf(".") == -1) eventHandler = "this." + eventHandler + "(event)";
                        events.Add("'" + eventName + "': (event) => " + eventHandler);
                    } else if (attr.Name == "x-if") {
                        //...<span x-if="state.value > 0">greather than 0</span>...
                        jsLine[0] = indent + "...((_ifs.c" + level + " = (" + attr.Value + ")) ? [";
                        jsPostLine.Add("] : []),");
                    } else if (attr.Name == "x-elseif") {
                        //...<span x-elseif="state.value < 0">less than 0</span>...
                        jsLine[0] = indent + "...(_ifs.c" + level + " ? [] : (_ifs.c" + level + " = (" + attr.Value + ")) ? [";
                        jsPostLine.Add("] : []),");
                    } else if (attr.Name == "x-else") {
                        //...<span x-else>is 0</span>...
                        jsLine[0] = indent + "...(!_ifs.c" + level + " ? [";
                        jsPostLine.Add("] : []),");
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
                        js.AppendLine(indent + "{tag:\"#comment\", attrs:{}, props:{}, events:{}, options:{index:" + index + ", forType:'" + forType + "'}, children:'x-for-start'},");
                        //add loop
                        var parts = attr.Value.Replace(" in ","|").Split('|');
                        //if (parts.Length != 2) logger.error(`Error compiling template: invalid x -for attribute detected: ${ attr.Value}`);
                        if (parts.Length != 2) throw new Exception($"Error compiling template: invalid x -for attribute detected: {attr.Value}");
                        var itemName = parts[0].Trim();
                        var indexName = "index";
                        if (itemName.StartsWith("(") && itemName.EndsWith(")")) {
                            var arr = itemName.Substring(1, itemName.Length - 1).Split(',');
                            itemName = arr[0].Trim();
                            indexName = arr[1].Trim();
                        }
                        var listName = parts[1].Trim();
                        jsLine[0] = indent + "...(renderUtils.toArray(" + listName + ").map((" + itemName + ", " + indexName + ") => (";
                        jsPostLine.Add(")),");
                        if (!string.IsNullOrEmpty(keyName)) options.Add("\"key\":" + itemName + "." + keyName);
                        //creates a comment end node that indicates the end of the for loop
                        jsPost.Add(indent + "{tag:\"#comment\", attrs:{}, props:{}, events:{}, options:{index:" + index + ", forType:'" + forType + "'}, children:'x-for-end'},\n");
                        wrapChildNodes = true;
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
                        jsLine[0] = indent + "...((renderCount==0) ? [";
                        jsPostLine.Add("] : [{tag:'" + node.Name.ToLower() + "', attrs:{}, props:{}, events: {}, options:{once:true}}]),");
                    } else if (attr.Name == "x-pre") {
                        //v-pre: skip childs
                        options.Add("format:\"html\"");
                        text = Utils.EscapeCsString(node.InnerHtml).Replace("<x:text>", "{{").Replace("</x:text>", "}}");
                    } else if (attr.Name.StartsWith("x-")) {
                        //error
                        //logger.error(`Error compiling template: invalid template attribute detected: ${ attr.Name}`);
                        throw new Exception($"Error compiling template: invalid template attribute detected: {attr.Name}");
                    } else {
                        attrs.Add("'" + attr.Name + "':" + Utils.EscapeCsString(attr.Value));
                    }
                }
                jsLine.Add(", attrs:" +'{' + String.Join(", ", attrs.ToArray()) + '}');
                jsLine.Add(", props:" + '{' + String.Join(", ", props.ToArray()) + '}' );
                jsLine.Add(", events:" + '{' + String.Join(", ", events.ToArray()) + '}');
                jsLine.Add(", options:" + '{' + String.Join(", ", options.ToArray()) + '}');
                if (!string.IsNullOrEmpty(text)) {
                    jsLine.Add(", children:" + text + "");
                    jsLine.Add("}," + (jsPostLine.Count > 0 ? String.Join("", jsPostLine) : ""));
                    js.Append(string.Join("", jsLine.ToArray()));
                } else if (node.ChildNodes.Count > 0) {
                    jsLine.Add(", children:[\n");
                    js.Append(string.Join("", jsLine.ToArray()));
                    var subIndex = 0;
                    node.ChildNodes.ToList().ForEach((subNode) => {
                        subIndex += CreateRenderFunctionRecursive(name, subNode, subIndex, js, level + 1);
                    });
                    
                    js.AppendLine(indent + "]}" + (wrapChildNodes ? ")" : "") + "," + (jsPostLine.Count > 0 ? String.Join("", jsPostLine) : ""));
                } else {
                    jsLine.Add("}," + (jsPostLine.Count > 0 ? String.Join("", jsPostLine) : "") + "\n");
                    js.Append(string.Join("", jsLine.ToArray()));
                }
                if (jsPost.Count > 0) js.Append(string.Join("", jsPost.ToArray()));
                inc++;
            }
            return inc;
        }

    } 


}







