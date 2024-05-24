
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FromHeaderAttribute(string name) : Attribute {
        public string Name { get; } = name;
    }

}
