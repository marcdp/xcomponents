
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class PageLayoutAttribute(string name) : Attribute {
        public string Name { get; } = name;
    }

}
