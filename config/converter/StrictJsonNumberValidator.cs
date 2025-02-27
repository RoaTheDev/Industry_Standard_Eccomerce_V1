using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Exception;

namespace Ecommerce_site.config.converter
{
    public class StrictJsonNumberValidator<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType != JsonTokenType.Number)
                    throw new ApiValidationException("The value is either null or empty.");

                return ConvertNumber(reader);
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