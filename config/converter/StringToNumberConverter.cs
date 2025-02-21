using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.Exception;

namespace Ecommerce_site.config.converter
{
    public class StringToNumberConverter<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        throw new ApiValidationException("The value is either null or empty.");
                    }

                    return ConvertStringToNumber(stringValue);

                case JsonTokenType.Number:
                    return ConvertNumber(reader);

                default:
                    throw new ApiValidationException(
                        $"Unexpected JSON token: {reader.TokenType} for type {typeof(T).Name}.");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(Convert.ToDecimal(value));
        }

        private T ConvertStringToNumber(string stringValue)
        {
            Type targetType = typeof(T);

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Int32:
                    return (T)(object)int.Parse(stringValue);
                case TypeCode.Int64:
                    return (T)(object)long.Parse(stringValue);
                case TypeCode.Single:
                    return (T)(object)float.Parse(stringValue);
                case TypeCode.Double:
                    return (T)(object)double.Parse(stringValue);
                case TypeCode.Decimal:
                    return (T)(object)decimal.Parse(stringValue);
                default:
                    throw new ApiValidationException($"Unsupported numeric type: {targetType.Name}");
            }
        }

        private T ConvertNumber(Utf8JsonReader reader)
        {
            Type targetType = typeof(T);

            try
            {
                switch (Type.GetTypeCode(targetType))
                {
                    case TypeCode.Int32:
                        return (T)(object)reader.GetInt32();
                    case TypeCode.Int64:
                        return (T)(object)reader.GetInt64();
                    case TypeCode.Single:
                        return (T)(object)reader.GetSingle();
                    case TypeCode.Double:
                        return (T)(object)reader.GetDouble();
                    case TypeCode.Decimal:
                        return (T)(object)reader.GetDecimal();
                    default:
                        throw new ApiValidationException($"Unsupported numeric type: {targetType.Name}");
                }
            }
            catch (InvalidOperationException)
            {
                throw new ApiValidationException($"The JSON token value is not compatible with {targetType.Name}.");
            }
            catch (OverflowException)
            {
                throw new ApiValidationException($"The JSON number is too large or too small for {targetType.Name}.");
            }
        }
    }
}