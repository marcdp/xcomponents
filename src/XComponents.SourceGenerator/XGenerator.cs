using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XComponents.SourceGenerator {

    [Generator]
    public class XGenerator : ISourceGenerator {


        // constants
        public const string TEMPLATE_EXTENSION = ".html";
        public const string CS_EXTENSION = ".cs";
        public const string XBASE_CLASS_NAME = "XBase";
        public const string XBASE_CLASS_NAME_FULL = "XComponents.XBase";
        public const string XPAGE_CLASS_NAME_FULL = "XComponents.XPage";
        public const string XWEBCOMPONENT_CLASS_NAME_FULL = "XComponents.XWebComponent";

        // methods
        public void Initialize(GeneratorInitializationContext context) {
            // No initialization required for this one
        }
        public void Execute(GeneratorExecutionContext context) {
            var classNames = new List<string>();
            var sourceGenerator = new SourceGenerator();
            var diagnostics = new SourceGenerator.Diagnostics(context);
            // Find all classes that inherit from XComponent
            var xTypes = context.Compilation
                    .SyntaxTrees  
                    .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
                    .Where(x => x is ClassDeclarationSyntax)
                    .Cast<ClassDeclarationSyntax>()
                    .Where((classDeclarationSyntax) => {
                        var classSymbol = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(classDeclarationSyntax);
                        var targetClassSymbol = classSymbol;
                        while (targetClassSymbol != null) {
                            if (targetClassSymbol.ToString().Equals(XBASE_CLASS_NAME_FULL)) {
                                return true;
                            }
                            targetClassSymbol = targetClassSymbol.BaseType;
                        }
                        return false;
                    })
                    .ToList();

            // Generate partial code for each class
            foreach (var classDeclarationSyntax in xTypes) {
                // Prepare code generation
                var classSymbol = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(classDeclarationSyntax);
                if (classSymbol == null) continue;
                var fullName = Utils.GetClassDeclarationSyntaxFullName(classDeclarationSyntax);
                var namespaceName = fullName.Substring(0, fullName.Length - classDeclarationSyntax.Identifier.Text.Length - 1);
                var className = fullName.Split('.').Last();
                var componentName = Utils.PascalToKebabCase(className);
                var csPath = classDeclarationSyntax.SyntaxTree.FilePath;
                // Definition
                var definition = new SourceGenerator.Definition() {
                    Name = className,
                    Namespace = namespaceName,
                    ComponentName = Utils.PascalToKebabCase(className),
                    TypePath = csPath,
                    Extends = (classSymbol.BaseType != null ? ": " + classSymbol.BaseType.ToString() : ": " + XBASE_CLASS_NAME + " "),
                    IsPage = classSymbol.BaseType!.ToString().Equals(XPAGE_CLASS_NAME_FULL),
                    IsWebComponent = classSymbol.BaseType!.ToString().Equals(XWEBCOMPONENT_CLASS_NAME_FULL),
                };
                classSymbol.GetAttributes().ToList().ForEach(attr => {
                    if (attr.AttributeClass?.Name == "LayoutAttribute") {
                        attr.ConstructorArguments.ToList().ForEach(arg => {
                            if (arg.Value != null) definition.LayoutName = arg.Value.ToString();
                        });
                    } else if(attr.AttributeClass?.Name == "TemplateEngineAttribute") {
                        attr.ConstructorArguments.ToList().ForEach(arg => {
                            if (arg.Value != null) definition.TemplateEngine = arg.Value.ToString();
                        });
                    }
                });
                // Properties
                var properties = new List<SourceGenerator.StateProperty>();
                classSymbol.GetMembers().OfType<IPropertySymbol>().ToList().ForEach(property => {
                    var propertyType = property.Type.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
                    var stateProperty = new SourceGenerator.StateProperty() {
                        Name = property.Name,
                        Type = propertyType.ToString(),
                        Readonly = property.IsReadOnly,
                    };
                    property.GetAttributes().ToList().ForEach(attr => {
                        if (attr.AttributeClass?.Name == "StateAttribute") stateProperty.StateAttribute = true;
                        if (attr.AttributeClass?.Name == "FromAttributeAttribute") {
                            stateProperty.FromAttribute = Utils.PascalToKebabCase(property.Name);
                            attr.ConstructorArguments.ToList().ForEach(arg => {
                                if (arg.Value != null) stateProperty.FromAttribute = arg.Value.ToString();
                            });
                        }
                        if (attr.AttributeClass?.Name == "FromQueryAttribute") {
                            stateProperty.FromQuery = Utils.PascalToKebabCase(property.Name);
                            attr.ConstructorArguments.ToList().ForEach(arg => {
                                if (arg.Value != null) stateProperty.FromQuery = arg.Value.ToString();
                            });
                        }
                        if (attr.AttributeClass?.Name == "FromRouteAttribute") {
                            stateProperty.FromRoute = property.Name;
                            attr.ConstructorArguments.ToList().ForEach(arg => {
                                if (arg.Value != null) stateProperty.FromRoute = arg.Value.ToString();
                            });
                        }
                        if (attr.AttributeClass?.Name == "FromHeaderAttribute") {
                            attr.ConstructorArguments.ToList().ForEach(arg => {
                                if (arg.Value != null) stateProperty.FromHeader = arg.Value.ToString();
                            });
                        }
                        if (attr.AttributeClass?.Name == "FromServicesAttribute") {
                            stateProperty.FromServices = "";
                            attr.ConstructorArguments.ToList().ForEach(arg => {
                                if (arg.Value != null) stateProperty.FromServices = arg.Value.ToString();
                            });
                        }
                    });
                    properties.Add(stateProperty);
                });
                definition.Properties = properties.ToArray();
                // Methods
                var rpcMethods = new List<SourceGenerator.RpcMethod>();
                classSymbol.GetMembers().OfType<IMethodSymbol>().ToList().ForEach(method => {
                    method.GetAttributes().ToList().ForEach(attr => {
                        if (attr.AttributeClass?.Name == "RpcAttribute") {
                            //IsAsync
                            var rpcMethod = new SourceGenerator.RpcMethod();
                            rpcMethod.Name = method.Name;
                            rpcMethod.Type = method.ReturnType.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
                            var rpcMethodParams = new List<SourceGenerator.RpcMethodParam>();
                            method.Parameters.ToList().ForEach(parameter => {
                                var parameterType = parameter.Type.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
                                var rpcParameter = new SourceGenerator.RpcMethodParam() {
                                    Name = parameter.Name,
                                    Type = parameterType.ToString(),
                                };
                                rpcMethodParams.Add(rpcParameter);
                            });
                            rpcMethod.Params = rpcMethodParams.ToArray();
                            rpcMethods.Add(rpcMethod);
                        }
                    });
                });
                definition.RpcMethods = rpcMethods.ToArray();
                // Template
                definition.TemplatePath = csPath.Substring(0, csPath.Length - CS_EXTENSION.Length);
                var templateAdditionalFile = context.AdditionalFiles.Where(predicate => predicate.Path.Equals(definition.TemplatePath)).FirstOrDefault();
                if (templateAdditionalFile == null) {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.XC1000__FileNotFound, classDeclarationSyntax.GetLocation(), definition.TemplatePath));
                    continue;
                }
                var template = templateAdditionalFile.GetText();
                if (template == null) {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.XC1000__FileNotFound, classDeclarationSyntax.GetLocation(), definition.TemplatePath));
                    continue;
                }
                definition.Template = template.ToString();
                
                // Generate
                var source = sourceGenerator.Generate(definition, diagnostics);
                // Add the source code to the compilation
                context.AddSource($"{namespaceName + "." + className}.g.cs", source);
                // Add the class name to the list
                classNames.Add(className);
            }

            // Find all x files without class 
            foreach (var file in context.AdditionalFiles) {
                if (file.Path.EndsWith(TEMPLATE_EXTENSION)) {
                    var className = System.IO.Path.GetFileNameWithoutExtension(file.Path);
                    if (!classNames.Contains(className)) {
                        var componentName = Utils.PascalToKebabCase(className);
                        var namespaceName = "XComponents.Autogenerated";
                        var templatePath = file.Path;
                        var template = file.GetText();
                        if (template == null) {
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.XC1000__FileNotFound, null, templatePath));
                        } else {
                            // Generate partial class code
                            var definition = new SourceGenerator.Definition() {
                                Name = className,
                                Namespace = namespaceName,
                                ComponentName = componentName,
                                TypePath = "",
                                Extends = ": XComponents.XComponent",
                                Template = template.ToString(),
                                TemplatePath = templatePath
                            };
                            // Generate
                            var source = sourceGenerator.Generate(definition, diagnostics);
                            // Add the source code to the compilation
                            context.AddSource($"{namespaceName + "." + className}.g.cs", source);
                            // Add the class name to the list
                            classNames.Add(className);
                        }
                    }
                }
            }
        }
    } 
}







