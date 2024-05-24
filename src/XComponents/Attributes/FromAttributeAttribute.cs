
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FromAttributeAttribute(string? name = null) : Attribute {
        public string? Name { get; } = name;
    }

}
