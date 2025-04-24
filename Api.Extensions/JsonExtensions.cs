using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Api.Extensions
{
    public static class JsonExtensions
    {
        public static JsonElement GetElement(this JsonElement jsonElement, string path)
        {
            if (jsonElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return default;

            string[] segments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                if (int.TryParse(segment, out var index) && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    jsonElement = jsonElement.EnumerateArray().ElementAtOrDefault(index);
                    if (jsonElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                        return default;

                    continue;
                }

                jsonElement = jsonElement.TryGetProperty(segment, out var value) ? value : default;

                if (jsonElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    return default;
            }

            return jsonElement;
        }

        public static JsonNode GetNode(this JsonNode root, string path)
        {
            var temp = root;

            if (root is not JsonNode && root is not JsonObject)
                temp = root.AsObject();

            string pattern = @"\[(\d+)\]";
            string replacement = ".$1";

            string result = Regex.Replace(path, pattern, replacement);
            string[] paths = result.Split('.', StringSplitOptions.RemoveEmptyEntries);

            foreach (var prop in paths)
            {
                if (int.TryParse(prop, out int index))
                    temp = temp[index];
                else //if (item is string propertyName)
                    temp = temp[prop];
            }

            return temp;
        }

        public static JsonNode GetNode(this JsonObject root, string path) => GetNode(root as JsonNode, path);

        public static JsonNode UpdateNode(this JsonNode root, string path, object value)
        {
            var temp = root;

            if (root is not JsonNode && root is not JsonObject)
                temp = root.AsObject();

            string pattern = @"\[(\d+)\]";
            string replacement = ".$1";

            string result = Regex.Replace(path, pattern, replacement);
            string[] paths = result.Split('.', StringSplitOptions.RemoveEmptyEntries);

            foreach (var prop in paths)
            {
                if (int.TryParse(prop, out int index))
                    temp = temp[index];
                else //if (item is string propertyName)
                    temp = temp[prop];
            }

            temp.ReplaceWith(value);

            return root;
        }

        public static JsonNode UpdateNode(this JsonNode node, string propertyName, object propertyValue, bool descendNested = true)
        {
            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    UpdateNode(item, propertyName, propertyValue, descendNested);
                }
            }
            else if (node is JsonObject obj)
            {
                List<(string Key, object Value)> replacements = default;
                foreach (var property in obj)
                {
                    if (property.Value is JsonValue value)
                    {
                        if (property.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            (replacements ??= []).Add((property.Key, propertyValue));
                        }
                    }
                    else if (property.Value != null && descendNested)
                    {
                        UpdateNode(property.Value, propertyName, propertyValue, descendNested);
                    }
                }

                if (replacements != null)
                    foreach (var item in replacements)
                        obj[item.Key] = JsonValue.Create(item.Value);

            }
            return node;
        }

        public static JsonNode UpdateNode(this JsonObject root, string path, object value) => UpdateNode(root as JsonNode, path, value);
    }
}
