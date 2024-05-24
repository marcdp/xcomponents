
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class TemplateEngineAttribute(string name) : Attribute {
        public string Name { get; set; } = name;
    }

}
