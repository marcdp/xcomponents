using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.AspNetCore.Mvc;

namespace XShell.Controllers {

    [Route("/manifest.webmanifest")]
    public class Manifest(Configuration configuration) : Controller {

        [HttpGet]
        public IActionResult Index() {
            var json = JsonSerializer.Serialize(configuration.Manifest, new JsonSerializerOptions() {                 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver {
                    Modifiers = { 
                        ExcludeEmptyStrings 
                    }
                }
            });
            return new ContentResult() {
                Content = json,
                ContentType = "application/manifest+json"
            };
        }

        //private methods
        void ExcludeEmptyStrings(JsonTypeInfo jsonTypeInfo) {
            if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            foreach (JsonPropertyInfo jsonPropertyInfo in jsonTypeInfo.Properties) {
                if (jsonPropertyInfo.PropertyType == typeof(string)) {
                    jsonPropertyInfo.ShouldSerialize = static (obj, value) =>
                        !string.IsNullOrEmpty((string)value);
                }
            }
        }
    }

}
