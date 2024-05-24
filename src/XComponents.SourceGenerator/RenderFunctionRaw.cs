using System;
using System.Linq;
using System.Text;

using HtmlAgilityPack;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace XComponents.SourceGenerator {

    public class RenderFunctionRaw {


        // Methods
        public void CreateRenderFunction(SourceGenerator.Definition definition, SourceGenerator.Diagnostics diagnostics, StringBuilder code, HtmlNodeCollection nodes, int level, ref int sequence) {
            var baseLevel = 4;
            var indent = new String(' ', (level + baseLevel) * 4);
            var html = new StringBuilder();
            foreach(var node in nodes) {
                html.AppendLine(node.OuterHtml);
            }
            code.AppendLine($"            __context.WriteHtml(\"{html.Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n")}\");");
        } 


    } 


}







