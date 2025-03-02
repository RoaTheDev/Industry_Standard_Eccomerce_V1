using System.ComponentModel.DataAnnotations;
using Ecommerce_site.Model.Enum;

namespace Ecommerce_site.Dto.Request.OrderRequest;

public class OrderStatusChangeRequest
{
    public required long OrderId { get; set; }
    [EnumDataType(typeof(OrderStatusEnum),ErrorMessage = "Invalid Order Status")]
    public required OrderStatusEnum OrderStatus { get; set; }
}