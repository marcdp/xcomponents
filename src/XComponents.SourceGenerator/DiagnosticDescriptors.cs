using Microsoft.CodeAnalysis;

namespace XComponents.SourceGenerator {
    public class DiagnosticDescriptors {
        public static readonly DiagnosticDescriptor XC1000__FileNotFound = new DiagnosticDescriptor(
            id: "XC1000",
            title: "XComponent file not found",
            messageFormat: "XComponent file not found: {0}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        public static readonly DiagnosticDescriptor XC1001__HtmlParseError = new DiagnosticDescriptor(
            id: "XC1001",
            title: "HTML parser error",
            messageFormat: "HTML parser error: {0}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        public static readonly DiagnosticDescriptor XC1002__HtmlInvalidNode = new DiagnosticDescriptor(
            id: "XC1002",
            title: "HTML invalid node",
            messageFormat: "HTML invalid node: {0}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        public static readonly DiagnosticDescriptor XC1003__HtmlSyntaxError = new DiagnosticDescriptor(
            id: "XC1003",
            title: "HTML syntax error",
            messageFormat: "HTML syntax error: {0}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

    }


}
