using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.config.converter;

namespace Ecommerce_site.config
{
    public class CustomJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(int)
                   || typeToConvert == typeof(long)
                   || typeToConvert == typeof(float)
                   || typeToConvert == typeof(double)
                   || typeToConvert == typeof(decimal)
                   || typeToConvert == typeof(bool)
                   || typeToConvert == typeof(string)
                   || typeToConvert == typeof(DateTime);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(int))
                return new StringToNumberConverter<int>();
            if (typeToConvert == typeof(long))
                return new StringToNumberConverter<long>();
            if (typeToConvert == typeof(float))
                return new StringToNumberConverter<float>();
            if (typeToConvert == typeof(double))
                return new StringToNumberConverter<double>();
            if (typeToConvert == typeof(decimal))
                return new StringToNumberConverter<decimal>();
            if (typeToConvert == typeof(bool))
                return new StringToBoolConverter();
            if (typeToConvert == typeof(DateTime))
                return new StringToDateTimeConverter();
            if (typeToConvert == typeof(string))
                return new IntToStringConverter();

            throw new NotSupportedException($"Converter for {typeToConvert} is not supported.");
        }
    }
}