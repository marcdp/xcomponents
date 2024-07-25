using System.IO.Pipelines;
using System.Text.Json; 

namespace XComponents {


    public static class Converter {

        // general
        public static T? To<T>(object? value) {
            if (value == null) {
                return default;
            } else if (value.GetType() == typeof(T)) {
                return (T)value;
            } else if (value is JsonElement jsonElement) {
                if (jsonElement.ValueKind == JsonValueKind.String) {
                    return jsonElement.Deserialize<T>();
                } else if (jsonElement.ValueKind == JsonValueKind.Number) {
                    return jsonElement.Deserialize<T>();
                } else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False) {
                    return jsonElement.Deserialize<T>();
                } else if (jsonElement.ValueKind == JsonValueKind.Null) {
                    return default;
                } else {
                    throw new NotImplementedException();
                }
            }
            return (T) System.Convert.ChangeType(value, typeof(T));
        }

        // enumerable
        //public static (T, int, T)[] ToEnumerable<T>(IEnumerable<T> value) {
        //    var result = new List<(T, int, T)>();
        //    foreach (var item in value) {
        //        result.Add((item, result.Count, item));
        //    }
        //    return result.ToArray();
        //}
        //public static (int, int, int)[] ToEnumerable(int value) {
        //    var result = new (int, int, int)[value];
        //    for (int i = 1; i <= value; i++) {
        //        result[i - 1] = (i, i - 1, i);
        //    }
        //    return result;
        //}
        //public static (string, int, object?)[] ToEnumerable(IDictionary<string, object> aObject) {
        //    var result = new List<(string, int, object?)>();
        //    foreach (var key in aObject.Keys) {
        //        result.Add((key.ToString()!, result.Count, aObject[key]));
        //    }
        //    return result.ToArray();
        //}
        //public static (string, int, object?)[] ToEnumerable(object aObject) {
        //    var result = new List<(string, int, object?)>();
        //    var type = aObject.GetType();
        //    foreach (var propertyInfo in type.GetProperties()) {
        //        var propertyValue = (propertyInfo.CanRead ? propertyInfo.GetValue(aObject) : null);
        //        result.Add((propertyInfo.Name, result.Count, propertyValue));
        //    }
        //    return result.ToArray();
        //}


        public static IEnumerable<VNode> Enumerate<T>(IEnumerable<T> enumeration, Func<T, int, object?, VNode> handler) {
            var index = 0;
            var result = new List<VNode>();
            foreach (var item in enumeration) {
                result.Add(handler(item, index++, item));
            }
            return result;
        }
        public static IEnumerable<VNode> Enumerate(int value, Func<int, int, object?, VNode> handler) {
            var index = 0;
            var result = new List<VNode>();
            for (int i = 1; i <= value; i++) {
                result.Add(handler(i, index++, i));
            }
            return result;            
        }
        public static IEnumerable<VNode> Enumerate(string value, Func<string, int, object?, VNode> handler) {
            var index = 0;
            var result = new List<VNode>();
            foreach (var c in value) {
                result.Add(handler(c.ToString(), index++, c));
            }
            return result;
        }
        public static IEnumerable<VNode> Enumerate(object enumeration, Func<string, int, object?, VNode> handler) {
            var index = 0;
            var result = new List<VNode>();
            foreach(var propertyInfo in enumeration.GetType().GetProperties()) {
                var value = propertyInfo.GetValue(enumeration);
                result.Add(handler(propertyInfo.Name, index++, value));
            }
            return result;
        }

        // boolean
        public static bool ToBoolean(bool value) {
            return value;
        }
        public static bool ToBoolean(short value) {
            return value != 0;
        }
        public static bool ToBoolean(int value) {
            return value != 0;
        }
        public static bool ToBoolean(long value) {
            return value != 0;
        }
        public static bool ToBoolean(double value) {
            return value != 0;
        }
        public static bool ToBoolean(float value) {
            return value != 0;
        }
        public static bool ToBoolean(string? value) {
            return value != null && value.Length > 0;
        }
        public static bool ToBoolean(object? value) {
            return value != null && ToBoolean(value.ToString());
        }
    }

} 