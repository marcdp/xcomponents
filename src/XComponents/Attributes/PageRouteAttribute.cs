
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class PageRouteAttribute(string pattern) : Attribute {
        public string Pattern { get; } = pattern;
    }

}
