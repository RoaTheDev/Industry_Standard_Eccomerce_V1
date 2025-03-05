using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Dto;
using Ecommerce_site.Exception;

namespace Ecommerce_site.config.Rule;

public class StrictJsonStringValidator : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new ApiValidationException("Invalid numeric format.");

        return reader.GetString()!;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}