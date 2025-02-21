using System.Text.Json.Serialization;
using Ecommerce_site.config.converter;

namespace Ecommerce_site.Dto.Request.CategoryRequest;

public class CategoryCreateRequest
{
    public required string CategoryName { get; set; }

    public required string Description { get; set; }

    // [MustBeAValidInteger(ErrorMessage = "The value {0} is not a valid quantity")]
    [JsonConverter(typeof(StringToNumberConverter<long>))]
    public required long CreateBy { get; set; }
}