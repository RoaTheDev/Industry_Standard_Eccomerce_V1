using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce_site.config.Rule;

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
                return new StrictJsonNumberValidator<int>();
            if (typeToConvert == typeof(long))
                return new StrictJsonNumberValidator<long>();
            if (typeToConvert == typeof(float))
                return new StrictJsonNumberValidator<float>();
            if (typeToConvert == typeof(double))
                return new StrictJsonNumberValidator<double>();
            if (typeToConvert == typeof(decimal))
                return new StrictJsonNumberValidator<decimal>();
            if (typeToConvert == typeof(bool))
                return new StrictJsonBooleanValidator();
            if (typeToConvert == typeof(DateTime))
                return new StringToDateTimeConverter();
            if (typeToConvert == typeof(string))
                return new StrictJsonStringValidator();

            throw new NotSupportedException($"Converter for {typeToConvert} is not supported.");
        }
    }
}