
using System.Reflection.Metadata.Ecma335;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using XShell.Components;

namespace XShell {


    public static class Extensions {


        //methods
        public static void AddXShell(this IServiceCollection services, Action<Configuration> action) {
            // configuration
            var configuration = new Configuration(services);
            services.AddSingleton(configuration);
            action.Invoke(configuration);
            // add antiforgery
            services.AddAntiforgery();
            // add razor components
            services.AddRazorComponents();
            // add controllers
            services.AddControllers(); 
        }
        public static void UseXShell(this WebApplication app) {
            // configuration
            var configuration = app.Services.GetRequiredService<Configuration>();
            // use status code pages with reexecute
            app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
            // use antiforgery
            app.UseAntiforgery();
            // map controllers
            app.MapControllers();
            // map razor components
            app.MapRazorComponents<App>()
                .AddAdditionalAssemblies(configuration.GetAssemblies());


        }

    }
}
