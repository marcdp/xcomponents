
using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.Extensions.DependencyInjection;


namespace XShell {

    public class Configuration {

        //inner class
        public class MenuItem {
            public string Name { get; set; } = "";
            public string Icon { get; set; } = "";
            public string Label { get; set; } = "";
            public string Route { get; set; } = "";
            public string ClassName { get; set; } = "";
            public List<MenuItem> Childs { get; set; } = [];
            public MenuItem Clone() {
                var clone = new MenuItem();
                clone.Name = this.Name;
                clone.Icon = this.Icon;
                clone.Label = this.Label;
                clone.Route = this.Route;
                clone.ClassName = this.ClassName;
                clone.Childs.AddRange(this.Childs.Select(c => c.Clone()));
                return clone;
            }
        };
        public class Module(Assembly assembly) {
            public Assembly Assembly { get; set; } = assembly;
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public List<MenuItem> Menu { get; set; } = new();
        }
        public class WebManifestIcon {
            public string Src { get; set; } = "";
            public string Sizes { get; set; } = "";
            public string Type { get; set; } = "";
        }
        public class WebManifestShortcut {
            public string Name { get; set; } = "";
            public string ShortName { get; set; } = "";
            public string Description { get; set; } = "";
            public string Url { get; set; } = "";
            public WebManifestIcon[] Icons { get; set; } = [];
        }
        public class WebManifest {
            public string Lang { get; set; } = "";
            public string Name { get; set; } = "";
            public string ShortName { get; set; } = "";
            public string Description { get; set; } = "";
            public string Version { get; set; } = "";
            public string Build { get; set; } = "";
            public string Copyright { get; set; } = "";
            public string StartUrl { get; set; } = "/";
            public string Scope { get; set; } = "/";
            public WebManifestShortcut[] Shortcuts { get; } = [];
            public WebManifestIcon[] Icons { get; } = [];
            public WebManifestIcon[] Screenshots { get; } = [];
            public string ThemeColor { get; set; } = "";
            public string BackgroundColor { get; set; } = "";
            public string Display { get; set; } = "";
            public string Orientation { get; set; } = "";
        }
        public class UIIconLibrary {
            public string Prefix { get; set; } = "";
            public string Url { get; set; } = "";
        }
        public class UIWebComponentLibrary {
            public string Prefix { get; set; } = "";
            public string Url { get; set; } = "";
        }
        public class UIConfiguration {
            public bool SmartBrowsing { get; set; } = true;
            public List<UIIconLibrary> IconLibraries { get; } = new();
            public List<UIWebComponentLibrary> WebComponentLibraries { get; } = new();
        }


        //ctor
        public Configuration(IServiceCollection services) {
            Services = services;
            UI.IconLibraries.Add(new UIIconLibrary() {
                Prefix = "x",
                Url = "/_content/XShell/icons/{name}.svg"
            });
            UI.WebComponentLibraries.Add(new UIWebComponentLibrary() {
                Prefix = "x",
                Url = "/_content/XShell/js/x/{name}.js"
            });
        }


        //props
        public IServiceCollection Services { get; }
        public WebManifest Manifest { get; set; } = new();
        public List<Module> Modules { get; } = new();
        public UIConfiguration UI { get; } = new();


        //methods
        public Assembly[] GetAssemblies() {
            return Modules.Select(x => x.Assembly).ToArray();
        }
        
    } 

}
