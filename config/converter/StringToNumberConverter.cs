using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Exception;

namespace Ecommerce_site.config.converter
{
    public class StringToNumberConverter<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                // if (reader.TokenType != JsonTokenType.Number)
                // {
                //     throw new ApiValidationException("The value is either null or empty.");
                // }

                if (reader.TokenType == JsonTokenType.String)
                {
                    string stringValue = reader.GetString()!;
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        throw new ApiValidationException("The value is either null or empty.");
                    }

                    return ConvertStringToNumber(stringValue);
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return ConvertNumber(reader);
                }

                throw new ApiValidationException(
                    $"Unexpected JSON token: {reader.TokenType} for type {typeof(T).Name}.");
            }
            catch (System.Exception ex) when (ex is FormatException || ex is InvalidCastException ||
                                              ex is OverflowException)
            {
                throw new JsonException($"Invalid {typeof(T).Name} format.");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(Convert.ToDecimal(value));
        }

        private T ConvertStringToNumber(string stringValue)
        {
            Type targetType = typeof(T);

            if (targetType == typeof(int) && int.TryParse(stringValue, out int intValue))
                return (T)(object)intValue;
            if (targetType == typeof(long) && long.TryParse(stringValue, out long longValue))
                return (T)(object)longValue;
            if (targetType == typeof(float) && float.TryParse(stringValue, out float floatValue))
                return (T)(object)floatValue;
            if (targetType == typeof(double) && double.TryParse(stringValue, out double doubleValue))
                return (T)(object)doubleValue;
            if (targetType == typeof(decimal) && decimal.TryParse(stringValue, out decimal decimalValue))
                return (T)(object)decimalValue;

            throw new ApiValidationException($"The value '{stringValue}' is not a valid numeric value.");
        }

        private T ConvertNumber(Utf8JsonReader reader)
        {
            Type targetType = typeof(T);

            if (targetType == typeof(int))
                return (T)(object)reader.GetInt32();
            if (targetType == typeof(long))
                return (T)(object)reader.GetInt64();
            if (targetType == typeof(float))
                return (T)(object)reader.GetSingle();
            if (targetType == typeof(double))
                return (T)(object)reader.GetDouble();
            if (targetType == typeof(decimal))
                return (T)(object)reader.GetDecimal();

            throw new ApiValidationException($"Unsupported numeric type: {targetType.Name}");
        }
    }
}