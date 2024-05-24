
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using XComponents.Middlewares;


namespace XComponents {


    public static class Extensions {


        //methods
        public static void AddXComponents(this IServiceCollection services, Action<Configuration> action) {
            // configuration
            var configuration = new Configuration(services);
            services.AddSingleton(configuration);
            action.Invoke(configuration);
            // custom
            typeof(Extensions).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(XComponent)) && !type.IsAbstract).ToList().ForEach(type => {
                var componentName = XComponents.SourceGenerator.Utils.PascalToKebabCase(type.Name);
                if (componentName.IndexOf("-") == -1) throw new Exception($"Unable to register XComponent {type.FullName}: invalid name: {componentName}: must have a dash in its component name");
                services.AddKeyedTransient(typeof(XComponent), componentName, type);
            });
            // configure modules
            foreach (var module in configuration.Modules) {
                module.ConfigureServices(services, configuration);
            }
            // add router  
            services.AddSingleton<Router>();
        }
        public static void UseXComponents(this IApplicationBuilder app) {
            // use status code pages with re execute
            app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
            // use XMiddleware
            app.UseMiddleware<XJsonRpcMiddleware>();
            app.UseMiddleware<XWebComponentsMiddleware>();
            app.UseMiddleware<XRenderMiddleware>();

        }
    } 

}
