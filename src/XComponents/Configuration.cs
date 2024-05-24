
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;


namespace XComponents {


    public class Configuration {

        //// inner class
        //public class SettingsClass {
        //    public bool Development { get; set; } = false;
        //    public string LayoutDefault { get; set; } = "layout-default";
        //    public bool RenderStreaming { get; set; } = true;
        //}

        // Vars
        private List<Module> mModules { get; } = new();
        private List<Route> mRoutes { get; } = new();

        // Ctor
        public Configuration(IServiceCollection services) {
            Services = services;
        }

        // Props
        public IServiceCollection Services { get; }
        public bool Development { get; set; } = false;
        public string LayoutDefault { get; set; } = "layout-default";
        public bool RenderStreaming { get; set; } = true;
        public string Version { get; set; } = "";
        //public SettingsClass Settings { get;set; } = new();

        public Module[] Modules => mModules.ToArray();
        public Route[] Routes => mRoutes.ToArray();

        // Methods
        public void AddModule<T>() where T : Module, new() {
            mModules.Add(new T());
        }
        public void AddModule(Module module) {
            mModules.Add(module);
        }
        public void AddRoute(string componentName, string url, Regex? regex) {
            mRoutes.Add(new Route(componentName, url, regex));
        }

    } 

}
