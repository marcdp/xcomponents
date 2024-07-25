
using Microsoft.AspNetCore.Http.Features;

using Sample3.App;

using XComponents;

namespace Sample3 {

    public class Program {

       
        public static async Task Main(string[] args) {

            // Create the builder
            var builder = WebApplication.CreateBuilder(args);
              
            // Add services to the container.
            builder.Services.AddXComponents(options => {
                // development  
                options.Development = builder.Environment.IsDevelopment();
                options.Version = "1.0";
                // modules 
                options.AddModule<Sample3.App.Module>();
            });

            // Build the app.
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment()) {    
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            } 
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseXComponents();              
              
            // Run
            await app.RunAsync();
        }
    }
}

//x - default layout 
//x - XPage -> specify layout
//x - custom component name
//x - render auto closed html elements
//x - errors invalid layout
//x - errors invalid XComponents
//x - add static js files to XComponents and serve them
//x - async components
//x - server render streaming 
//x - buffer response
//x - converters
//x - rpc
//x - WebComponents --> render on server
//x - rpc in XComponents, XLayouts, XWebComponents like Astro Island
//x - Configuration.Version --> to add at the end of all urls!!
//x - Permetre especificar el Template engine a utilitzar ??? (x, raw, etc)
//x - client XComponents.rpc
//x - XShell
//x - improve diagnostics
//x - XComponents -> normal script


//x SSR slot inheritance
// HotReload --> reload page
// Streaming










// - partial/includes that not includes wrapper
// - partial/includes that includes wrapper
// - webcompoennt way

// XWebComponent -->  put before and after {{state.Value}}

// - define settings at directory level? (authentication required), layout, acl, ....

// - inherits slots (ex: Layout that inherits another Layout)

// - XWebComponent -> avoid render SLOT on server
// - XWebComponent --> generate render function
// - WebComponents --> hydrate on client -> implement

// - use Settings.cs by directory/namespace
// - client XComponents.log errors

// - authenticate attributes at page level?

// - XPage.redirect

// - error page
// - error page 404 not found

// - markdown component / file extension?