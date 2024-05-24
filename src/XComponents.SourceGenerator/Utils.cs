using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace XComponents.SourceGenerator {

    public static class Utils {


        // consts
        public const string NESTED_CLASS_DELIMITER = "+";
        public const string NAMESPACE_CLASS_DELIMITER = ".";


        // methods
        public static string GetClassDeclarationSyntaxFullName(ClassDeclarationSyntax source) {
            var items = new List<string>();
            var parent = source.Parent;
            while (parent.IsKind(SyntaxKind.ClassDeclaration)) {
                if (parent is ClassDeclarationSyntax parentClass) {
                    items.Add(parentClass.Identifier.Text);
                    parent = parent.Parent;
                }
            }
            var sb = new StringBuilder();
            if (parent is NamespaceDeclarationSyntax namespaceName) {
                sb.Append(namespaceName.Name).Append(NAMESPACE_CLASS_DELIMITER);
            }
            items.Reverse();
            items.ForEach(i => { sb.Append(i).Append(NESTED_CLASS_DELIMITER); });
            sb.Append(source.Identifier.Text);
            return sb.ToString();
        }


        // string methods
        public static string EscapeCsString(this string value) {
            return "\"" + value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").Replace("\"", "\\\"") + "\"";
        }
        public static string PascalToKebabCase(this string value) {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
        public static string KebabToPascalCase(this string value) {
            if (string.IsNullOrEmpty(value))
                return value;

            return value
                .Split('-')
                .Select(x => x.First().ToString().ToUpper() + x.Substring(1))
                .Aggregate((x, y) => x + y);
        }
        public static string KebabToCamalCase(this string value) {
           if (string.IsNullOrEmpty(value))
                return value;
            return value
                .Split('-')
                .Select(x => x.First().ToString().ToUpper() + x.Substring(1))
                .Aggregate((x, y) => x + y);
        }


        // 
        public static (int, int) GetTextPosition(string text, int line, int column, int index) {
            var counter = 0;
            foreach (var c in text) {
                if (c == '\n') {
                    line += 1;
                    column = 0;
                } else if (c == '\r') {
                } else {
                    column++;
                }
                if (counter == index) {
                    break;
                }
                counter++;
            }
            return (line, column);
        }
    }
}
