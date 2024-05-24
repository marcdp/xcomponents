using System.Text.RegularExpressions;

namespace XComponents {

    public class Route(string componentName, string url, Regex? pattern) {

        //props
        public string ComponentName { get; set; } = componentName;
        public string Url { get; set; } = url;
        public Regex? Pattern { get; set; } = pattern;


    }

}
