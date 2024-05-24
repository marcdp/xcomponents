
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using XComponents.Attributes;


namespace XComponents {
    

    public class Module {

        //props
        public string Name { get; }
        public string Description { get; }
        public Assembly Assembly { get; }
        public string Namespace { get; }

        //ctor
        public Module() {
            Name = this.GetType().GetCustomAttribute<Attributes.ModuleAttribute>()?.Name ?? this.GetType().Name;
            Description = this.GetType().GetCustomAttribute<Attributes.ModuleAttribute>()?.Title ?? "";
            Assembly = this.GetType().Assembly;
            Namespace = this.GetType().Namespace!;
        }

        //methods
        public virtual void ConfigureServices(IServiceCollection services, Configuration configuration) {
            // add XComponents
            Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(XComponent)) && !type.IsAbstract).ToList().ForEach(type => {
                var componentName = XComponents.SourceGenerator.Utils.PascalToKebabCase(type.Name);
                if (componentName.IndexOf("-") == -1) throw new Exception($"Unable to register XComponent {type.FullName}: invalid name: {componentName}: must have a dash in its component name");
                services.AddKeyedTransient(typeof(XComponent), componentName, type);
            });
            // add XLayouts
            Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(XLayout)) && !type.IsAbstract).ToList().ForEach(type => {
                var componentName = XComponents.SourceGenerator.Utils.PascalToKebabCase(type.Name);
                if (componentName.IndexOf("-") == -1) throw new Exception($"Unable to register XLayout {type.FullName}: invalid name: {componentName}: must have a dash in its component name");
                services.AddKeyedTransient(typeof(XLayout), componentName, type);
            });
            // add XPages (routes)
            Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(XPage)) && !type.IsAbstract).ToList().ForEach(type => {
                var pattern = type.GetCustomAttribute<PageRouteAttribute>()?.Pattern;
                var className = type.FullName!.Replace(Namespace + ".", "").Replace(".", "");
                var componentName = XComponents.SourceGenerator.Utils.PascalToKebabCase(className);
                services.AddKeyedTransient(typeof(XPage), componentName, type);
                // url
                var url = type.FullName!.Replace(Namespace + ".", "").Replace(".", "/");
                if (!url.StartsWith("/")) url = "/" + url;
                if (url.StartsWith("/Pages/")) url = url.Substring(url.IndexOf("/", 1));
                if (url.EndsWith("/Default")) url = url.Substring(0, url.LastIndexOf("/"));
                if (url.StartsWith("/")) url = url.Substring(1);
                // route
                var route = new StringBuilder();
                foreach(var part in url.Split("/")) {
                    route.Append("/" + XComponents.SourceGenerator.Utils.PascalToKebabCase(part));
                }
                // regex route
                StringBuilder? regexString = null;
                if (pattern != null && pattern.IndexOf("{") != -1) {
                    if (pattern.StartsWith("/")) pattern = pattern.Substring(1);
                    regexString = new StringBuilder();
                    foreach (var patternPart in pattern.Split("/")) {
                        if (patternPart.StartsWith("{") && patternPart.EndsWith("}")) {
                            var expressionPart = patternPart.Substring(1, patternPart.Length - 2) + ":string";
                            var expressionParts = expressionPart.Split(":");
                            var expressionPartName = expressionParts[0];
                            var expressionPartType = expressionParts[1];
                            if (expressionPartName.StartsWith("*")) {
                                regexString.Append("/(?<" + expressionPartName.Substring(1) + ">[a-zA-Z0-9-/]*)");
                            } else if (expressionPartType.Equals("int")) {
                                regexString.Append("/(?<" + expressionPartName + ">-\\d+$|^0$|\\d+)"); // negative, zero or positive
                            } else if (expressionPartType.Equals("bool")) {
                                regexString.Append("/(?<" + expressionPartName + ">true|false|0|1)");
                            } else if (expressionPartType.Equals("date")) {
                                regexString.Append("/(?<" + expressionPartName + ">\\d{4}-\\d{2}-\\d{2})");
                            } else if (expressionPartType.Equals("datetime")) {
                                regexString.Append("/(?<" + expressionPartName + ">\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2})");
                            } else {
                                regexString.Append("/(?<" + expressionPartName + ">[a-zA-Z0-9-]+)");
                            }
                        } else {
                            regexString.Append("/" + XComponents.SourceGenerator.Utils.PascalToKebabCase(patternPart));
                        }
                    }
                }
                // add route
                configuration.AddRoute(componentName, route.ToString(), (regexString == null ? null : new Regex(regexString.ToString(), RegexOptions.IgnoreCase)));
            });
        }
    }

}
