using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Dto;

namespace Ecommerce_site.config.converter;

public class StringToDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType == JsonTokenType.String && DateTime.TryParse(reader.GetString(), out DateTime result))
            {
                return result;
            }
        }
        catch (System.Exception)
        {
            throw new JsonException(JsonSerializer.Serialize(new ApiStandardResponse<string?>(
                400, "Invalid DateTime format", null
            )));
        }

        throw new JsonException(JsonSerializer.Serialize(new ApiStandardResponse<string?>(
            400, "Invalid DateTime format", null
        )));
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}