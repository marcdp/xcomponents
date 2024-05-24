
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FromQueryAttribute(string? name = null) : Attribute {
        public string? Name { get; } = name;
    }

}
