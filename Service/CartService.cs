using Ecommerce_site.Data;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CartRequest;
using Ecommerce_site.Dto.response.CartResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Service;

public class CartService : ICartService
{
    private readonly IGenericRepo<Cart> _cartRepo;
    private readonly IGenericRepo<CartItem> _cartItemsRepo;
    private readonly IGenericRepo<Model.Product> _productRepo;
    private readonly EcommerceSiteContext _dbContext;

    public CartService(IGenericRepo<Cart> cartRepo, IGenericRepo<CartItem> cartItemsRepo,
        IGenericRepo<Model.Product> productRepo, EcommerceSiteContext dbContext)
    {
        _cartRepo = cartRepo;
        _cartItemsRepo = cartItemsRepo;
        _productRepo = productRepo;
        _dbContext = dbContext;
    }

    public async Task<ApiStandardResponse<CartResponse?>> GetCartByCustomerIdAsync(long id)
    {
        CartResponse? cartResponse = await _cartRepo.GetSelectedColumnsByConditionAsync(
            c => c.CustomerId == id && !c.IsCheckout,
            c => new CartResponse
            {
                CartId = c.CartId,
                CustomerId = c.CustomerId,
                CreatedAt = c.CreatedAt,
                IsCheckedOut = c.IsCheckout,
                TotalBasePrice = c.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity),
                TotalDiscount = c.CartItems.Sum(ci => ci.Discount),
                TotalAmount = c.CartItems.Sum(ci => ci.TotalPrice),
                CartItems = c.CartItems.Select(ci => new CartItemResponse
                    {
                        CartItemsId = ci.CartItemId,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.ProductName,
                        Quantity = ci.Quantity,
                        Discount = ci.Discount,
                        TotalPrice = ci.TotalPrice,
                        UnitPrice = ci.UnitPrice
                    }
                ).ToList()
            }, cIn => cIn.Include(ct => ct.CartItems)
                .ThenInclude(ctp => ctp.Product));

        if (cartResponse is null)
            return new ApiStandardResponse<CartResponse?>(StatusCodes.Status404NotFound, "No active cart found.");

        if (cartResponse.CartItems.Count == 0)
            return new ApiStandardResponse<CartResponse?>(StatusCodes.Status204NoContent,
                "There are no items in the cart");

        return new ApiStandardResponse<CartResponse?>(StatusCodes.Status200OK, cartResponse);
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> AddToCartAsync(long customerId,
        AddToCartRequest request)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var product =
                    await _productRepo.GetSelectedColumnsByConditionAsync(p => p.ProductId == request.ProductId,
                        p => new { p.Price, p.DiscountPercentage, p.Quantity });

                if (product is null)
                    return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                        "Product not found.");

                if (product.Quantity <= 0)
                    return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status400BadRequest,
                        "Product is out of stock.");

                if (request.Quantity > product.Quantity)
                    return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status400BadRequest,
                        $"Only {product.Quantity} left for the product");


                var fk = await _cartRepo.GetSelectedColumnsByConditionAsync(
                    c => c.CustomerId == customerId && !c.IsCheckout,
                    c => new
                    {
                        c.CartId
                    });


                if (fk is null)
                {
                    Cart newCart = await _cartRepo.AddAsync(new Cart
                    {
                        CustomerId = customerId,
                        IsCheckout = false,
                    });
                    fk = new { newCart.CartId };
                }

                var existingCartItem = await _cartItemsRepo.GetByConditionAsync(ci =>
                        ci.CartId == fk.CartId && ci.ProductId == request.ProductId,
                    cIn => cIn.Include(ct => ct.Product));

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += request.Quantity;

                    if (existingCartItem.Quantity > product.Quantity)
                        return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status400BadRequest,
                            "The product stock is less that the amount you specify");

                    existingCartItem.Discount = (existingCartItem.UnitPrice * existingCartItem.Quantity)
                                                * (existingCartItem.Product.DiscountPercentage / 100m);
                    existingCartItem.TotalPrice = (existingCartItem.UnitPrice * existingCartItem.Quantity)
                                                  - existingCartItem.Discount;

                    await _cartItemsRepo.UpdateAsync(existingCartItem);
                }
                else
                {
                    decimal unitPrice = product.Price;
                    decimal discountAmount = (unitPrice * request.Quantity) * (product.DiscountPercentage / 100m);
                    decimal totalPrice = (unitPrice * request.Quantity) - discountAmount;

                    CartItem cartItem = new CartItem();
                    cartItem.CartId = fk.CartId;
                    cartItem.ProductId = request.ProductId;
                    cartItem.Quantity = request.Quantity;
                    cartItem.UnitPrice = unitPrice;
                    cartItem.Discount = discountAmount;
                    cartItem.TotalPrice = totalPrice;

                    await _cartItemsRepo.AddAsync(cartItem);
                }

                await transaction.CommitAsync();
                return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status201Created,
                    new ConfirmationResponse
                    {
                        Message = "Item added successfully"
                    });
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<ApiStandardResponse<UpdateCartItemResponse?>> UpdateCartItemAsync(long customerId,
        CartItemsUpdateRequest request)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var cartItem =
                    await _cartItemsRepo.GetByConditionAsync(ci =>
                            ci.CartItemId == request.CartItemsId && ci.Cart.CustomerId == customerId &&
                            !ci.Cart.IsCheckout,
                        cic => cic.Include(c => c.Cart)
                            .Include(c => c.Product));

                if (cartItem is null)
                    return new ApiStandardResponse<UpdateCartItemResponse?>(StatusCodes.Status404NotFound,
                        "Cart item not found.");
                if (request.Quantity > cartItem.Product.Quantity)
                    return new ApiStandardResponse<UpdateCartItemResponse?>(StatusCodes.Status400BadRequest,
                        "The amount cannot exceed the available product quantity");

                if (cartItem.Quantity != request.Quantity)
                {
                    cartItem.Quantity = request.Quantity;
                }

                cartItem.UnitPrice = cartItem.Product.Price;

                decimal discountAmount = (cartItem.UnitPrice * request.Quantity)
                                         * (cartItem.Product.DiscountPercentage / 100m);
                cartItem.Discount = discountAmount;
                cartItem.TotalPrice = (cartItem.UnitPrice * request.Quantity) - discountAmount;
                await _cartItemsRepo.UpdateAsync(cartItem);
                await transaction.CommitAsync();

                return new ApiStandardResponse<UpdateCartItemResponse?>(StatusCodes.Status200OK,
                    new UpdateCartItemResponse()
                    {
                        CartItemId = cartItem.CartItemId,
                        Quantity = cartItem.Quantity
                    });
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> RemoveCartItemAsync(long customerId, long cartItemId)
    {
        var cartItem = await _cartItemsRepo.GetByConditionAsync(ci =>
                ci.CartItemId == cartItemId && ci.Cart.CustomerId == customerId &&
                !ci.Cart.IsCheckout,
            cic => cic.Include(c => c.Cart));

        if (cartItem is null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                "There are no item in the cart");
        await _cartItemsRepo.DeleteAsync(cartItem);
        return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "The item has been remove"
        });
    }
}