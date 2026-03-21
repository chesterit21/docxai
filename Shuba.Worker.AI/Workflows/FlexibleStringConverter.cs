using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shuba.Worker.AI.Workflows;

/// <summary>
/// Handles deserialization of a JSON field that can be either:
/// - a plain string  → returned as-is
/// - an array of strings → joined with newline separator
/// Always serializes back as a plain string.
/// </summary>
public class FlexibleStringConverter : JsonConverter<string?>
{
    private readonly string _separator;

    public FlexibleStringConverter() : this("\n") { }
    public FlexibleStringConverter(string separator) => _separator = separator;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            JsonTokenType.StartArray => ReadArray(ref reader),
            _ => throw new JsonException($"Unexpected token type '{reader.TokenType}' for string field.")
        };
    }

    private string ReadArray(ref Utf8JsonReader reader)
    {
        var sb = new StringBuilder();
        bool first = true;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray) break;

            if (!first) sb.Append(_separator);

            sb.Append(reader.TokenType == JsonTokenType.String
                ? reader.GetString()
                : throw new JsonException($"Expected string inside array, got '{reader.TokenType}'."));

            first = false;
        }

        return sb.ToString();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}