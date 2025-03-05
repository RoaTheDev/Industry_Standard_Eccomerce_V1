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
    private readonly IGenericRepo<Cart> _cartRepo;
    private readonly IGenericRepo<User> _userRepo;
    private readonly EcommerceSiteContext _dbContext;

    public OrderService(IGenericRepo<Order> orderRepo, IGenericRepo<OrderItem> orderItemRepo,
        IGenericRepo<Customer> customerRepo, IGenericRepo<Address> addressRepo, IGenericRepo<Product> productRepo,
        EcommerceSiteContext dbContext, IGenericRepo<Cart> cartRepo, IGenericRepo<User> userRepo)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _productRepo = productRepo;
        _dbContext = dbContext;
        _cartRepo = cartRepo;
        _userRepo = userRepo;
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
            {
                OrderStatusEnum.Shipped,
                new ConcurrentBag<OrderStatusEnum> { OrderStatusEnum.Delivered }
            },
            { OrderStatusEnum.Delivered, new ConcurrentBag<OrderStatusEnum>() },
            { OrderStatusEnum.Canceled, new ConcurrentBag<OrderStatusEnum>() }
        };

    public async Task<ApiStandardResponse<OrderResponse>> GetOrderByIdAsync(long customerId, long orderId)
    {
        var order = await _orderRepo.GetSelectedColumnsByConditionAsync(
            o => o.OrderId == orderId && o.CustomerId == customerId,
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


    public async Task<ApiStandardResponse<OrderResponse>> OrderCreateFromCartAsync(long customerId,
        OrderCreateRequest request)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                if (!await _customerRepo.EntityExistByConditionAsync(c =>
                        c.CustomerId == customerId && !c.IsDeleted))
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not exist");

                bool addressesValid = await ValidateCustomerAddresses(customerId, request.BillingAddressId,
                    request.ShippingAddressId);

                if (!addressesValid)
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not have the specified billing or shipping address");

                if (!request.OrderItems.Any())
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                        "Cannot create an order with no items");

                var distinctProductIds = request.OrderItems.Select(i => i.ProductId).ToHashSet();

                if (distinctProductIds.Count != request.OrderItems.Count)
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                        "Duplicate products in order request");

                var cart = await _cartRepo.GetByConditionAsync(
                    c => c.CustomerId == customerId && !c.IsCheckout,
                    cInc => cInc.Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                );

                if (cart == null)
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status404NotFound,
                        "No active cart found for this customer");

                if (!cart.CartItems.Any())
                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                        "Cannot create an order with no items");

                var cartItemDict = cart.CartItems.ToDictionary(ci => ci.ProductId);

                foreach (var orderItem in request.OrderItems)
                {
                    if (!cartItemDict.TryGetValue(orderItem.ProductId, out var cartItem))
                        return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                            $"Product ID {orderItem.ProductId} is not in your cart");

                    if (orderItem.Quantity != cartItem.Quantity)
                        return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                            $"Quantity mismatch for product ID {orderItem.ProductId}");
                }

                var availableProducts = cart.CartItems
                    .Where(ci => !ci.Product.IsDeleted && ci.Product.IsAvailable)
                    .Select(ci => ci.Product)
                    .ToList();

                if (availableProducts.Count != cart.CartItems.Count)
                {
                    var unavailableProductNames = cart.CartItems
                        .Where(ci => ci.Product.IsDeleted || !ci.Product.IsAvailable)
                        .Select(ci => ci.Product.ProductName)
                        .ToList();

                    return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                        $"The following products are no longer available: {string.Join(", ", unavailableProductNames)}");
                }

                foreach (var cartItem in cart.CartItems)
                {
                    var product = cartItem.Product;
                    if (cartItem.Quantity > product.Quantity)
                        return new ApiStandardResponse<OrderResponse>(StatusCodes.Status400BadRequest,
                            $"Your order for {product.ProductName} exceeds available quantity. Only {product.Quantity} available.");
                }

                string orderNumber =
                    string.Concat(DateTime.UtcNow.ToString("yy-MM-dd"), Guid.NewGuid().ToString("N"));
                decimal shippingCost = 3.5m;

                Order order = new Order
                {
                    CustomerId = customerId,
                    OrderNumber = orderNumber,
                    OrderStatus = OrderStatusEnum.Pending.ToString(),
                    BillingAddressId = request.BillingAddressId,
                    ShippingAddressId = request.ShippingAddressId,
                    ShippingCost = shippingCost,
                };

                IList<OrderItem> orderItems = new List<OrderItem>();
                IList<Product> productsToUpdate = new List<Product>();

                decimal totalBaseAmount = 0;
                decimal totalDiscountAmount = 0;
                decimal totalAmount = 0;

                foreach (var item in cart.CartItems)
                {
                    var product = item.Product;
                    decimal unitPrice = product.Price;
                    decimal itemBaseAmount = unitPrice * item.Quantity;
                    decimal discountAmount = itemBaseAmount * (product.DiscountPercentage / 100m);
                    decimal itemTotalAmount = itemBaseAmount - discountAmount;

                    totalBaseAmount += itemBaseAmount;
                    totalDiscountAmount += discountAmount;
                    totalAmount += itemTotalAmount;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        Discount = discountAmount,
                        TotalPrice = itemTotalAmount
                    });

                    product.Quantity -= Convert.ToInt32(item.Quantity);
                    productsToUpdate.Add(product);
                }

                order.TotalBasedAmount = Math.Round(totalBaseAmount, 2);
                order.TotalAmount = Math.Round(totalAmount + shippingCost, 2);

                await _orderRepo.AddAsync(order);

                foreach (var item in orderItems)
                {
                    item.OrderId = order.OrderId;
                }

                await _orderItemRepo.AddBulkAsync(orderItems);
                await _productRepo.UpdateBulk(productsToUpdate);

                cart.IsCheckout = true;
                await _cartRepo.UpdateAsync(cart);

                await transaction.CommitAsync();

                List<OrderItemResponse> orderItemResponses = orderItems.Select(oi => new OrderItemResponse
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = cartItemDict[oi.ProductId].Product.ProductName,
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

    public async Task<ApiStandardResponse<DirectOrderResponse>> DirectOrderCreateAsync(long customerId,
        DirectOrderRequest request)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                if (!await _customerRepo.EntityExistByConditionAsync(c =>
                        c.CustomerId == customerId && !c.IsDeleted))
                    return new ApiStandardResponse<DirectOrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not exist");

                bool addressesValid = await ValidateCustomerAddresses(customerId, request.BillingAddressId,
                    request.ShippingAddressId);

                if (!addressesValid)
                    return new ApiStandardResponse<DirectOrderResponse>(StatusCodes.Status404NotFound,
                        "The customer does not have the specified billing or shipping address");

                var product = await _productRepo.GetByConditionAsync(p =>
                    p.ProductId == request.OrderItem.ProductId && p.IsAvailable && !p.IsDeleted);

                if (product is null)
                    return new ApiStandardResponse<DirectOrderResponse>(StatusCodes.Status404NotFound,
                        "The product you're selecting is not available");

                if (request.OrderItem.Quantity > product.Quantity)
                    return new ApiStandardResponse<DirectOrderResponse>(StatusCodes.Status404NotFound,
                        $"{product.ProductName} has only {product.Quantity} remaining");

                string orderNumber =
                    string.Concat(DateTime.UtcNow.ToString("yy-MM-dd"), Guid.NewGuid().ToString("N"));

                decimal shippingCost = 3.50m;
                decimal unitPrice = product.Price;
                decimal itemBaseAmount = unitPrice * request.OrderItem.Quantity;
                decimal discountAmount = itemBaseAmount * (product.DiscountPercentage / 100m);
                decimal itemTotalAmount = itemBaseAmount - discountAmount;

                var order = await _orderRepo.AddAsync(new Order
                {
                    CustomerId = customerId,
                    BillingAddressId = request.BillingAddressId,
                    ShippingAddressId = request.ShippingAddressId,
                    OrderNumber = orderNumber,
                    ShippingCost = shippingCost,
                    OrderStatus = OrderStatusEnum.Pending.ToString(),
                    TotalAmount = itemTotalAmount,
                    TotalBasedAmount = itemBaseAmount,
                });

                await _orderRepo.AddAsync(order);

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = request.OrderItem.Quantity,
                    UnitPrice = unitPrice,
                    Discount = discountAmount,
                    TotalPrice = itemTotalAmount
                };

                await _orderItemRepo.AddAsync(orderItem);
                product.Quantity -= Convert.ToInt32(request.OrderItem.Quantity);
                await _productRepo.UpdateAsync(product);
                await transaction.CommitAsync();

                return new ApiStandardResponse<DirectOrderResponse>(StatusCodes.Status201Created,
                    new DirectOrderResponse
                    {
                        CustomerId = customerId,
                        OrderDate = order.OrderDate,
                        OrderId = order.OrderId,
                        OrderNumber = order.OrderNumber,
                        OrderStatus = order.OrderStatus,
                        ShippingCost = order.ShippingCost,
                        TotalAmount = order.TotalAmount,
                        TotalBaseAmount = order.TotalBasedAmount,
                        BillingAddressId = request.BillingAddressId,
                        ShippingAddressId = request.ShippingAddressId,
                        TotalDiscountAmount = discountAmount,
                        OrderItem = new OrderItemResponse
                        {
                            Discount = discountAmount,
                            Quantity = request.OrderItem.Quantity,
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            TotalPrice = itemTotalAmount,
                            UnitPrice = product.Price,
                            OrderItemId = orderItem.OrderItemId
                        }
                    });
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<ApiStandardResponse<List<OrderResponse>>> GetAllOrderByCustomerIdAsync(long customerId)
    {
        var order = await _orderRepo.GetSelectedColumnsListsByConditionAsync(o => o.CustomerId == customerId,
            o => new OrderResponse
            {
                CustomerId = customerId,
                OrderDate = o.OrderDate,
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderStatus = o.OrderStatus,
                ShippingCost = o.ShippingCost,
                TotalAmount = o.TotalAmount,
                BillingAddressId = o.BillingAddressId,
                ShippingAddressId = o.ShippingAddressId,
                TotalBaseAmount = o.TotalBasedAmount,
                TotalDiscountAmount = o.OrderItems.Sum(oi => oi.Discount),
                OrderItem = o.OrderItems.Select(oi => new OrderItemResponse
                {
                    Discount = oi.Discount,
                    Quantity = oi.Quantity,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    TotalPrice = oi.TotalPrice,
                    UnitPrice = oi.UnitPrice,
                    OrderItemId = oi.OrderItemId
                }).ToList()
            });

        return order.Count != 0
            ? new ApiStandardResponse<List<OrderResponse>>(StatusCodes.Status200OK, order)
            : new ApiStandardResponse<List<OrderResponse>>(StatusCodes.Status404NotFound,
                "The order does not exist");
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> OrderStatusUpdateAsync(
        OrderStatusChangeRequest request)
    {
        var isAdmin = await _userRepo.EntityExistByConditionAsync(u =>
                u.UserId == request.AdminId && !u.IsDeleted &&
                EF.Functions.Like(u.Role.RoleName, RoleEnums.Admin.ToString()),
            uIn => uIn.Include(u => u.Role));
        
        if (!isAdmin)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "Only admin can make changes");

        var order = await _orderRepo.GetByIdAsync(request.OrderId);
        if (order is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The order does not exist");

        if (!Enum.TryParse<OrderStatusEnum>(order.OrderStatus, true, out var currentStatus))
        {
            return new ApiStandardResponse<ConfirmationResponse>(
                StatusCodes.Status400BadRequest,
                "Invalid order status value."
            );
        }

        if (request.OrderStatus == currentStatus)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK,
                new ConfirmationResponse { Message = "Order status is already up to date." });
        }

        if (AllowedStatusTransitions.TryGetValue(currentStatus, out var allowedTransitions))
        {
            if (!allowedTransitions.Contains(request.OrderStatus))
            {
                var allowedStatuses = string.Join(", ", allowedTransitions);
                return new ApiStandardResponse<ConfirmationResponse>(
                    StatusCodes.Status400BadRequest,
                    $"Invalid status transition from {currentStatus} to {request.OrderStatus}. Allowed transitions: {allowedStatuses}.");
            }
        }
        else
        {
            return new ApiStandardResponse<ConfirmationResponse>(
                StatusCodes.Status400BadRequest,
                "Current order status cannot be updated.");
        }

        order.OrderStatus = request.OrderStatus.ToString();
        await _orderRepo.UpdateAsync(order);

        return new ApiStandardResponse<ConfirmationResponse>(
            StatusCodes.Status200OK,
            new ConfirmationResponse { Message = $"Order status updated to {request.OrderStatus}." });
    }

    private async Task<bool> ValidateCustomerAddresses(long customerId, long billingAddressId, long shippingAddressId)
    {
        var addresses = await _addressRepo.GetAllByConditionAsync(a =>
            a.CustomerId == customerId &&
            (a.AddressId == billingAddressId || a.AddressId == shippingAddressId));

        bool hasBillingAddress = addresses.Any(a => a.AddressId == billingAddressId);
        bool hasShippingAddress = addresses.Any(a => a.AddressId == shippingAddressId);

        return hasBillingAddress && hasShippingAddress;
    }
}