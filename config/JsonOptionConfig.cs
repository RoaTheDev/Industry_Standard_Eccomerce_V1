using Ecommerce_site.config.converter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ecommerce_site.config
{
    // Targeting MVC's JsonOptions (used in Web API controllers)
    public class JsonOptionConfig : IConfigureOptions<JsonOptions>
    {
        private readonly CustomJsonConverterFactory _converterFactory;

        public JsonOptionConfig(CustomJsonConverterFactory converterFactory)
        {
            _converterFactory = converterFactory;
        }

        public void Configure(JsonOptions options)
        {
            options.AllowInputFormatterExceptionMessages = false;
            options.JsonSerializerOptions.Converters.Add(_converterFactory);
        }
    }
}