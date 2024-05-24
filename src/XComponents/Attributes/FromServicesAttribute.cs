
namespace XComponents.Attributes {

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FromServicesAttribute(string? key = null) : Attribute {
        public string? Key { get; } = key;
    }

}
