using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Dto;

namespace Ecommerce_site.config.converter;

public class StrictJsonStringValidator : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(JsonSerializer.Serialize(new ApiStandardResponse<string?>(
                400, "Invalid string format"
            )));
        }

        return reader.GetString()!;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}