using System.Globalization;
using System.Text.Json;

namespace GPA.Utils.Permissions
{
    public interface IPermissionComparer
    {
       bool PermissionMatchesPathStep(string profile, PermissionPathWithValue permissionPathWithValue);
    }

    public class PermissionComparer : IPermissionComparer
    {
        public bool PermissionMatchesPathStep(string profile, PermissionPathWithValue permissionPathWithValue)
        {
            return PermissionMatchesPathStep(
                profile,
                permissionPathWithValue.ValueToCompare,
                permissionPathWithValue);
        }

        public bool PermissionMatchesPathStep(string profile, object valueToCompare, PermissionPaths pathToCompare)
        {
            // Start with the dynamic permission object
            using var doc = JsonDocument.Parse(profile);
            JsonElement currentObject = doc.RootElement
                .GetProperty("Documents")
                .EnumerateArray().FirstOrDefault();

            // Traverse the path
            foreach (var step in pathToCompare.PermissionPath)
            {
                if (currentObject.ValueKind == JsonValueKind.Object)
                {
                    if (currentObject.TryGetProperty(step.PropertyName, out var nextObject))
                    {
                        currentObject = nextObject;
                    }
                    else
                    {
                        // If the path doesn't exist, return false
                        return false;
                    }
                }
                else if (currentObject.ValueKind == JsonValueKind.Array)
                {
                    ProcessIfSimpleArray(step, currentObject, valueToCompare, onMatch: (item) =>
                    {
                        currentObject = item;
                    });

                    ProcessIfComplexArray(step, currentObject, onMatch: (item) =>
                    {
                        currentObject = item;
                    });
                }
                else
                {
                    // If the path doesn't exist, return false
                    return false;
                }
            }

            // At this point, currentObject is the value from the path in the dynamic permission object
            // Compare it to the value to compare
            return HasSameValue(ReadValueAsString(currentObject), valueToCompare);
        }

        public void ProcessIfSimpleArray(PathStep step, JsonElement currentObject, object valueToCompare, Action<JsonElement> onMatch)
        {
            if (step.IsSimpleArray)
            {
                foreach (var item in currentObject.EnumerateArray())
                {
                    var value = ReadValueAsString(item);
                    if (value is not null && HasSameValue(value, valueToCompare))
                    {
                        onMatch(item);
                        break;
                    }
                }
            }
        }

        public void ProcessIfComplexArray(PathStep step, JsonElement currentObject, Action<JsonElement> onMatch)
        {
            if (!step.IsSimpleArray)
            {
                foreach (var item in currentObject.EnumerateArray())
                {
                    if (
                        item.ValueKind == JsonValueKind.Object &&
                        item.TryGetProperty(step.PropertyName, out var value) &&
                        HasSameValue(ReadValueAsString(value), step.ArrayPropertyValue)
                        )
                    {
                        onMatch(item);
                        break;
                    }
                }
            }
        }

        public string ReadValueAsString(JsonElement jsonElement)
        {
#pragma warning disable CS8603 // Utf8JsonReader is not nullable
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.GetDecimal().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => null
            };
#pragma warning disable CS8603 // Utf8JsonReader is not nullable
        }

        private bool HasSameValue(object obj1, object obj2)
        {
            return obj1?.ToString()?.Equals(obj2?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}
