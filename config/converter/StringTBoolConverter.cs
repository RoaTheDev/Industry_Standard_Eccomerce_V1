using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Dto;

namespace Ecommerce_site.config.converter;

public class StringToBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var strValue = reader.GetString()?.ToLower();
                return strValue == "true" || strValue == "1";
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32() == 1;
            }
            else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                return reader.GetBoolean();
            }
        }
        catch (System.Exception)
        {
            throw new JsonException(JsonSerializer.Serialize(new ApiStandardResponse<string?>(
                400, "Invalid boolean format", null
            )));
        }

        throw new JsonException(JsonSerializer.Serialize(new ApiStandardResponse<string?>(
            400, "Invalid boolean format", null
        )));
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}