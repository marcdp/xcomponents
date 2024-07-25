using Microsoft.Extensions.DependencyInjection;

namespace XComponents {


    public class Router {

        // vars
        private IServiceProvider mServices;
        private Configuration mConfiguration;
        private Route[] mRoutes;


        // ctor
        public Router(IServiceProvider services, Configuration configuration) {
            mServices = services;
            mConfiguration = configuration;
            mRoutes = mConfiguration.Routes;
        }


        // methods
        public XPage? Resolve(XContext context) {
            var path = context.HttpContext.Request.Path;
            // exact match
            foreach (var route in mRoutes) {
                if (route.Pattern == null && route.Url.Equals(path, StringComparison.InvariantCultureIgnoreCase)) {
                    var page = mServices.GetRequiredKeyedService<XPage>(route.ComponentName);
                    page.OnInit(context, null);
                    return page;
                }
            }
            // match with parameters
            foreach (var route in mRoutes) {
                if (route.Pattern != null) {
                    var match = route.Pattern.Match(path);
                    if (match.Success) {
                        var page = mServices.GetRequiredKeyedService<XPage>(route.ComponentName);
                        var groupNames = route.Pattern.GetGroupNames();
                        for (var i = 1; i < match.Groups.Count; i++) {
                            var groupName = groupNames[i];
                            page.SetFromRoute(groupName, match.Groups[i].Value);
                        }
                        return page;
                    }
                }
            }
            // not found
            return null;
        }
    }

} 