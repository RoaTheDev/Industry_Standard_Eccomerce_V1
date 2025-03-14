using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Dto;

namespace Ecommerce_site.config.Rule;

public class StrictJsonBooleanValidator : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!(reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False))
        {
            throw new JsonException(JsonSerializer.Serialize(new ApiStandardResponse<string?>(
                400, "Invalid boolean format"
            )));
        }

        return reader.GetBoolean();
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}