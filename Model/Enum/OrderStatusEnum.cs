using System.Reflection;
using System.Text.Json.Serialization;

namespace Ecommerce_site.Model.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatusEnum
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Canceled
}