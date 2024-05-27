
using Microsoft.AspNetCore.Http.Features;

using Sample3.App;

using XComponents;

namespace Sample3 {

    public class Program {




        public static async Task Main(string[] args) {


            var values = new string[] { "hello", "world" };
            //new List(XComponents.Converter.ToEnumerable<int>(values)).ForEach((item) => {
            //    var aaa = item.Item1;
            //    var aaa2 = item.Item2;
            //    var aaa3 = item.Item3;

            //});
            //var if1 = false;
            //VNode[] auxxxxx = [
            //    new VNode("#text",0).Attribute("one","1").Property("asda", 123).Option("index",1).Text("hello world"),
            //    new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //    new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //    new VNode("#text",0).Attribute("one","1").Property("asda", 123).Children([
            //        new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //        new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //        new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //        new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //        new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text("hello world"),
            //    ]),
            //    .. ((if1 = true) == true ? new VNode[] { new VNode("#text", 0).Attribute("one","1").Property("asda", 123).Text("hello world") } : []),
            //    .. values.Select((item, index) => new VNode("#text",0).Attribute("one","1").Property("asda", 123).Text(item))  

            //];
            //var state = new {
            //    Items = new int[] { 1, 2, 3 }
            //};
            //VNode[] aaaaaaa = [
            //    new VNode("#text", 0).Text("\r\n"),
            //    new VNode("#text", 1).Text("\r\n"),
            //    new VNode("i", index:2).Children([
            //        new VNode("#text", 0).Text("hello"),
            //        new VNode("b", index:1),
            //        new VNode("#text", 2).Text("world"),
            //    ]),
            //    new VNode("#text", 3).Text("\r\n"),
            //    new VNode("#comment", 4).Option("forType", "position").Text("x-for-start"),
            //    .. XComponents.Converter.ToEnumerable(state.Items).Select((itemmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm, index) => new VNode("li", index:4).Children([
            //        new VNode("#text", 0).Text("\r\n"),
            //    ])),
            //    new VNode("#comment", 4).Option("forType", "position").Text("x-for-end"),
            //    new VNode("#text", 5).Text("\r\n\r\n"),
            //    new VNode("#comment", 6).Text(""),
            //    new VNode("#text", 7).Text("\r\n"),
            //];


            //var ffff = new VDOM("#text", new Dictionary<string, string>() {
            //    {"one", "1" }
            //});
            //var ffff2 = new VDOM("#text", new() {{"one", "1" }, {"two", "22" }});

            //VDOM[] jj = [
            //    new VDOM("#text", { ["one"] = "1"})
            //];

            //object aaa = [
            //    new { tag = "#text", attrs= new { }, props= new { }, events= new { }, options= new {  index= 0}, children= "\r\n\r\n\r\n"},
            //    new { tag = "#text", attrs= new { }, props= new { }, events= new { }, options= new {  index= 2}, children= "\r\nvalue= "},
            //    new { tag = "#text", attrs= new { }, props= new { }, events= new { }, options= new {  index= 3}, children= 123},
            //    new { tag = "#text", attrs= new { }, props= new { }, events= new { }, options= new {  index= 4}, children= "."},
            //    new { tag = "#text", attrs= new { }, props= new { }, events= new { }, options= new {  index= 5}, children= 124},
            //    new { tag = "#text", attrs= new { }, props= new { }, events= new { }, options= new {  index= 6}, children= "..\r\n"}
            //];

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


// XWebComponent -->  put before and after {{state.Value}}


// - improve diagnostics



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