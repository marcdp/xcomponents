
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Class,AllowMultiple = false)]
    public class ModuleAttribute(string name, string title) : Attribute {
        public string Name { get; set; } = name;
        public string Title { get; set; } = title;
    }

}
