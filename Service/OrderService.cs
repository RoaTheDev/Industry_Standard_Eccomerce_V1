using System.Collections.Concurrent;
using Ecommerce_site.Data;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.OrderRequest;
using Ecommerce_site.Dto.response.OrderResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Service;

public class OrderService : IOrderService
{
    private readonly IGenericRepo<Order> _orderRepo;
    private readonly IGenericRepo<OrderItem> _orderItemRepo;
    private readonly IGenericRepo<Customer> _customerRepo;
    private readonly IGenericRepo<Address> _addressRepo;
    private readonly IGenericRepo<Product> _productRepo;
    private readonly EcommerceSiteContext _dbContext;

    public OrderService(IGenericRepo<Order> orderRepo, IGenericRepo<OrderItem> orderItemRepo,
        IGenericRepo<Customer> customerRepo, IGenericRepo<Address> addressRepo, IGenericRepo<Product> productRepo,
        EcommerceSiteContext dbContext)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _productRepo = productRepo;
        _dbContext = dbContext;
    }

    private static readonly Dictionary<OrderStatusEnum, ConcurrentBag<OrderStatusEnum>>
        AllowedStatusTransitions = new()
        {
            {
                OrderStatusEnum.Pending,
                new ConcurrentBag<OrderStatusEnum> { OrderStatusEnum.Processing, OrderStatusEnum.Canceled }
            },
            {
                OrderStatusEnum.Processing,
                new ConcurrentBag<OrderStatusEnum> { OrderStatusEnum.Shipped, OrderStatusEnum.Canceled }
            },
            { OrderStatusEnum.Delivered, new ConcurrentBag<OrderStatusEnum>() },
            { OrderStatusEnum.Canceled, new ConcurrentBag<OrderStatusEnum>() }
        };

    public async Task<ApiStandardResponse<OrderResponse>> GetOrderByIdAsync(long orderId)
    {
        var order = await _orderRepo.GetSelectedColumnsByConditionAsync(o => o.OrderId == orderId,
            o => new OrderResponse
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                OrderNumber = o.OrderNumber,
                ShippingCost = o.ShippingCost,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                TotalBaseAmount = o.TotalBasedAmount,
                BillingAddressId = o.BillingAddressId,
                ShippingAddressId = o.ShippingAddressId,
                TotalDiscountAmount = o.OrderItems.Sum(oi => oi.Discount),
                OrderItem = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    ProductName = oi.Product.ProductName,
                    Quantity = oi.Quantity,
                    Discount = oi.Discount,
                    ProductId = oi.ProductId,
                    TotalPrice = oi.TotalPrice,
                    UnitPrice = oi.UnitPrice,
                    OrderItemId = oi.OrderItemId
                }).ToList()
            }, o => o.Include(oIn => oIn.OrderItems)
                .ThenInclude(otIn => otIn.Product));

        if (order is null)
            return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound, "The order does not exist");

        return new ApiStandardResponse<OrderResponse>(StatusCodes.Status200OK, order);
    }

    public async Task<ApiStandardResponse<OrderResponse>> OrderCreateAsync(OrderCreateRequest request)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                if (!await _customerRepo.EntityExistByConditionAsync(c =>
                        c.CustomerId == request.CustomerId && !c.IsDeleted))
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not exist");

                if (!await _addressRepo.EntityExistByConditionAsync(a =>
                        a.AddressId == request.BillingAddressId && a.CustomerId == request.CustomerId))
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not have this billing address");

                if (!await _addressRepo.EntityExistByConditionAsync(a =>
                        a.AddressId == request.ShippingAddressId && a.CustomerId == request.CustomerId))
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not have this shipping address");

                if (!request.OrderItems.Any())
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                        "Cannot create an order with no items");

                var groupedItems = request.OrderItems
                    .GroupBy(i => i.ProductId)
                    .Select(g =>
                        new { ProductId = g.Key, TotalQuantity = g.Sum(i => i.Quantity) })
                    .ToList();

                var productIds = groupedItems.Select(g => g.ProductId).ToList();

                var products = await _productRepo.GetAllByConditionAsync(p =>
                    productIds.Contains(p.ProductId) && !p.IsDeleted && p.IsAvailable);

                var productDict = products.ToDictionary(p => p.ProductId);

                string orderNumber =
                    string.Concat(DateTime.UtcNow.ToString("yy-MM-dd"), Guid.NewGuid().ToString("N"));

                foreach (var groupItem in groupedItems)
                {
                    if (!productDict.TryGetValue(groupItem.ProductId, out var product))
                        return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                            $"{product?.ProductName} does not exist");

                    if (groupItem.TotalQuantity > product.Quantity)
                        return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                            $"Your order for {product.ProductName} is over the available quantity");
                }


                decimal shippingCost = 3.5m;

                Order order = new Order
                {
                    CustomerId = request.CustomerId,
                    OrderNumber = orderNumber,
                    OrderStatus = OrderStatusEnum.Pending.ToString(),
                    BillingAddressId = request.BillingAddressId,
                    ShippingAddressId = request.ShippingAddressId,
                    TotalBasedAmount = 0,
                    TotalAmount = 0,
                    ShippingCost = shippingCost,
                };
                await _orderRepo.AddAsync(order);

                IList<OrderItem> orderItems = new List<OrderItem>();
                IList<Product> productsToUpdate = new List<Product>();

                decimal totalBaseAmount = 0;
                decimal totalDiscountAmount = 0;
                decimal totalAmount = 0;
                foreach (var item in request.OrderItems)
                {
                    var product = productDict[item.ProductId];
                    decimal unitPrice = product.Price;
                    decimal itemBaseAmount = unitPrice * item.Quantity;
                    decimal discountAmount = itemBaseAmount * (product.DiscountPercentage / 100m);
                    decimal itemTotalAmount = itemBaseAmount - discountAmount;

                    totalBaseAmount += itemBaseAmount;
                    totalDiscountAmount += discountAmount;
                    totalAmount += itemTotalAmount;

                    orderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        Discount = discountAmount,
                        TotalPrice = itemTotalAmount
                    });

                    product.Quantity -= Convert.ToInt32(item.Quantity);
                    productsToUpdate.Add(product);
                }

                await _orderItemRepo.AddBulkAsync(orderItems);
                await _productRepo.UpdateBulk(productsToUpdate);

                order.TotalBasedAmount = totalBaseAmount;
                order.TotalAmount = totalAmount + shippingCost;

                await _orderRepo.UpdateAsync(order);
                await transaction.CommitAsync();

                List<OrderItemResponse> orderItemResponses = orderItems.Select(oi => new OrderItemResponse
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = productDict[oi.ProductId].ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Discount = oi.Discount,
                    TotalPrice = oi.TotalPrice
                }).ToList();

                return new ApiStandardResponse<OrderResponse>(StatusCodes.Status201Created, new OrderResponse
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    BillingAddressId = order.BillingAddressId,
                    ShippingAddressId = order.ShippingAddressId,
                    TotalBaseAmount = totalBaseAmount,
                    TotalDiscountAmount = totalDiscountAmount,
                    ShippingCost = shippingCost,
                    TotalAmount = order.TotalAmount,
                    OrderStatus = order.OrderStatus,
                    OrderItem = orderItemResponses
                });
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public Task<ApiStandardResponse<List<OrderResponse>>> GetAllOrderByCustomerIdAsync(long customerId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ConfirmationResponse>> OrderStatusUpdateAsync(OrderStatusChangeRequest request)
    {
        throw new NotImplementedException();
    }
}