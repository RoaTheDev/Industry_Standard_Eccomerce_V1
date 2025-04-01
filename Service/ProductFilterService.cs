using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq.Expressions;
using Ecommerce_site.Data;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class ProductFilterService : IProductFilterService
{
    private readonly ILogger _logger;
    private readonly IGenericRepo<Product> _productRepo;
    private readonly EcommerceSiteContext _dbContext;

    public ProductFilterService(ILogger logger, IGenericRepo<Product> productRepo, EcommerceSiteContext dbContext)
    {
        _logger = logger;
        _productRepo = productRepo;
        _dbContext = dbContext;
    }

    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetBestSellingProductsAsync(
        long cursorValue = 0,
        int pageSize = 10)
    {
        var filter = new ProductFilterRequest
        {
            SortBy = SortByEnum.BestSelling
        };

        return await GetFilteredProductsAsync(filter, cursorValue, pageSize, useFullTextSearch: true);
    }

    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetNewArrivalsAsync(
        long cursorValue = 0,
        int pageSize = 10)
    {
        var filter = new ProductFilterRequest
        {
            SortBy = SortByEnum.Latest
        };

        return await GetFilteredProductsAsync(filter, cursorValue, pageSize, useFullTextSearch: true);
    }


    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetFilteredProductsAsync(
        ProductFilterRequest filter,
        long cursorValue = 0,
        int pageSize = 10,
        bool useFullTextSearch = false)
    {
        try
        {
            pageSize = Math.Max(1, pageSize);

            if (filter is { MinPrice: not null, MaxPrice: not null } && filter.MinPrice > filter.MaxPrice)
            {
                return new ApiStandardResponse<PaginatedProductResponse>(
                    StatusCodes.Status400BadRequest,
                    "MinPrice cannot be greater than MaxPrice.");
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                filter.SearchQuery = filter.SearchQuery.Trim();
            }

            if (useFullTextSearch && !string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var fullTextResult = await PerformFullTextSearchAsync(filter, cursorValue, pageSize);
                if (fullTextResult.StatusCode == StatusCodes.Status200OK)
                {
                    return fullTextResult;
                }

                _logger.Warning("Full-text search failed, falling back to regular search");
            }

            if (filter.SortBy == SortByEnum.BestSelling)
            {
                return await GetBestSellingProductsAsync(filter, cursorValue, pageSize);
            }

            var predicate = BuildFilterPredicate(filter);
            var (sortSelector, ascending) = GetSortSelector(filter.SortBy ?? SortByEnum.None);
            var products =
                await GetPaginatedProductsAsync(filter, cursorValue, pageSize, predicate, sortSelector, ascending);
            var nextCursor = GetNextCursor(products, filter.IsLatest, pageSize);

            var response = new PaginatedProductResponse
            {
                Products = products.ToList(),
                NextCursor = nextCursor,
                PageSize = pageSize,
                AppliedFilters = new AppliedProductFilters
                {
                    CategoryId = filter.CategoryId,
                    TagIds = filter.TagIds,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice,
                    InStockOnly = filter.InStockOnly,
                    SortBy = filter.SortBy?.ToString(),
                    SortOrder = ascending ? SortOrder.Ascending.ToString() : SortOrder.Descending.ToString(),
                    SearchQuery = filter.SearchQuery
                }
            };

            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Error in GetFilteredProductsAsync");
            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving products");
        }
    }

    private async Task<ApiStandardResponse<PaginatedProductResponse>> GetBestSellingProductsAsync(
        ProductFilterRequest filter,
        long cursorValue = 0,
        int pageSize = 10)
    {
        try
        {
            var predicate = BuildFilterPredicate(filter);
            var filteredProducts = await _productRepo.GetSelectedColumnsListsByConditionAsync(
                predicate.And(p => p.OrderItems.Any()),
                p => new { p.ProductId, TotalOrdered = p.OrderItems.Sum(oi => oi.Quantity) },
                p => p.Include(pot => pot.OrderItems)
            );

            var orderedProducts = filteredProducts
                .OrderByDescending(p => p.TotalOrdered)
                .SkipWhile(p => cursorValue > 0 && p.ProductId <= cursorValue)
                .Take(pageSize)
                .Select(p => p.ProductId)
                .ToList();

            if (!orderedProducts.Any())
            {
                return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK,
                    new PaginatedProductResponse
                    {
                        Products = new List<PaginatedProduct>(),
                        NextCursor = null,
                        PageSize = pageSize,
                        AppliedFilters = new AppliedProductFilters
                        {
                            CategoryId = filter.CategoryId,
                            TagIds = filter.TagIds,
                            MinPrice = filter.MinPrice,
                            MaxPrice = filter.MaxPrice,
                            InStockOnly = filter.InStockOnly,
                            SortBy = nameof(SortByEnum.BestSelling)
                        }
                    });
            }

            var products = await _productRepo.GetSelectedColumnsListsByConditionAsync(
                p => orderedProducts.Contains(p.ProductId),
                p => new PaginatedProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Discount = p.DiscountPercentage,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Tags = p.Tags.Select(t => t.TagName).ToImmutableList(),
                    ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary).Select(pi => pi.ImageUrl).FirstOrDefault() ??
                                "",
                    CategoryName = p.Category.CategoryName,
                    CreateAt = p.CreatedAt
                },
                p => p.Include(pt => pt.Tags).Include(pc => pc.Category).Include(pi => pi.ProductImages)
            );

            var orderedResult = new List<PaginatedProduct>();
            foreach (var id in orderedProducts)
            {
                var product = products.FirstOrDefault(p => p.ProductId == id);
                if (product != null) orderedResult.Add(product);
            }

            long? nextCursor = orderedResult.Count == pageSize ? orderedResult.Last().ProductId : null;
            var response = new PaginatedProductResponse
            {
                Products = orderedResult,
                NextCursor = nextCursor,
                PageSize = pageSize,
                AppliedFilters = new AppliedProductFilters
                {
                    CategoryId = filter.CategoryId,
                    TagIds = filter.TagIds,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice,
                    InStockOnly = filter.InStockOnly,
                    SortBy = nameof(SortByEnum.BestSelling)
                }
            };

            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Error in GetBestSellingProductsAsync");
            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving best selling products");
        }
    }

    private async Task<ApiStandardResponse<PaginatedProductResponse>> PerformFullTextSearchAsync(
        ProductFilterRequest filter,
        long cursorValue = 0,
        int pageSize = 10)
    {
        try
        {
            var fullTextSupported = await CheckFullTextSearchSupportAsync();
            if (!fullTextSupported)
            {
                _logger.Warning("Full-text search is not supported by the database");
                return await GetRegularSearchProductsAsync(filter, cursorValue, pageSize);
            }

            var searchQuery = filter.SearchQuery ?? (object)DBNull.Value;
            var categoryId = filter.CategoryId ?? (object)DBNull.Value;
            var minPrice = filter.MinPrice ?? (object)DBNull.Value;
            var maxPrice = filter.MaxPrice ?? (object)DBNull.Value;
            var inStockOnly = filter.InStockOnly ?? (object)DBNull.Value;
            var tagIds = filter.TagIds != null ? string.Join(",", filter.TagIds) : (object)DBNull.Value;
            var sortBy = filter.SortBy.HasValue ? filter.SortBy.ToString()?.ToLower() : (object)DBNull.Value;

            var products = await _dbContext.Set<PaginatedProduct>()
                .FromSqlInterpolated(
                    $"EXEC inventory.SearchProducts @SearchQuery={searchQuery}, @CategoryId={categoryId}, @MinPrice={minPrice}, @MaxPrice={maxPrice}, @InStockOnly={inStockOnly}, @TagIds={tagIds}, @CursorValue={cursorValue}, @PageSize={pageSize}, @SortBy={sortBy}")
                .ToListAsync();

            long? nextCursor = products.Count == pageSize ? products.Last().ProductId : null;
            var response = new PaginatedProductResponse
            {
                Products = products,
                NextCursor = nextCursor,
                PageSize = pageSize,
                AppliedFilters = new AppliedProductFilters
                {
                    CategoryId = filter.CategoryId,
                    TagIds = filter.TagIds,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice,
                    InStockOnly = filter.InStockOnly,
                    SortBy = filter.SortBy?.ToString(),
                    SearchQuery = filter.SearchQuery
                }
            };

            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Error in PerformFullTextSearchAsync");
            return await GetRegularSearchProductsAsync(filter, cursorValue, pageSize);
        }
    }

    private async Task<bool> CheckFullTextSearchSupportAsync()
    {
        try
        {
            return await _dbContext.Database
                .SqlQueryRaw<bool>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'ProductCatalog') THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END")
                .SingleAsync();
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Error checking full-text search support");
            return false;
        }
    }

    private async Task<ApiStandardResponse<PaginatedProductResponse>> GetRegularSearchProductsAsync(
        ProductFilterRequest filter,
        long cursorValue,
        int pageSize)
    {
        try
        {
            var predicate = BuildFilterPredicate(filter);
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var searchTerm = filter.SearchQuery.ToLower();
                predicate = predicate.And(p =>
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm));
            }

            var (sortSelector, ascending) = GetSortSelector(filter.SortBy ?? SortByEnum.None);
            var products =
                await GetPaginatedProductsAsync(filter, cursorValue, pageSize, predicate, sortSelector, ascending);
            var nextCursor = GetNextCursor(products, filter.IsLatest, pageSize);

            var response = new PaginatedProductResponse
            {
                Products = products.ToList(),
                NextCursor = nextCursor,
                PageSize = pageSize,
                AppliedFilters = new AppliedProductFilters
                {
                    CategoryId = filter.CategoryId,
                    TagIds = filter.TagIds,
                    MinPrice = filter.MinPrice,
                    MaxPrice = filter.MaxPrice,
                    InStockOnly = filter.InStockOnly,
                    SortBy = filter.SortBy?.ToString(),
                    SortOrder = ascending ? SortOrder.Ascending.ToString() : SortOrder.Descending.ToString(),
                    SearchQuery = filter.SearchQuery
                }
            };

            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Error in GetRegularSearchProductsAsync");
            return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status500InternalServerError,
                "An error occurred while searching for products");
        }
    }

    private Expression<Func<Product, bool>> BuildFilterPredicate(ProductFilterRequest filter)
    {
        var predicate = PredicateBuilder.New<Product>(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var searchTerm = filter.SearchQuery.ToLower().Trim();
            predicate = predicate.And(p =>
                p.ProductName.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm));
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
            predicate = predicate.And(p => p.CategoryId == filter.CategoryId.Value);
        if (filter.MinPrice.HasValue && filter.MinPrice.Value >= 0)
            predicate = predicate.And(p => p.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue && filter.MaxPrice.Value >= 0)
            predicate = predicate.And(p => p.Price <= filter.MaxPrice.Value);
        if (filter.InStockOnly.HasValue)
            predicate = predicate.And(p => p.IsAvailable == filter.InStockOnly.Value);
        if (filter.TagIds?.Count > 0)
            predicate = predicate.And(p => p.Tags.Any(t => filter.TagIds.Contains(t.TagId)));

        return predicate;
    }

    private (Expression<Func<Product, object>> sortSelector, bool ascending) GetSortSelector(SortByEnum sortBy)
    {
        return sortBy switch
        {
            SortByEnum.MinPrice => (p => p.Price, true),
            SortByEnum.MaxPrice => (p => p.Price, false),
            SortByEnum.Name => (p => p.ProductName, true),
            SortByEnum.Date => (p => p.CreatedAt, false),
            SortByEnum.Latest => (p => p.CreatedAt, false),
            SortByEnum.BestSelling => (p => p.ProductId, true), // Handled separately in GetBestSellingProductsAsync
            _ => (p => p.ProductId, true)
        };
    }

    private async Task<IReadOnlyList<PaginatedProduct>> GetPaginatedProductsAsync(
        ProductFilterRequest filter,
        long cursorValue,
        int pageSize,
        Expression<Func<Product, bool>> predicate,
        Expression<Func<Product, object>> sortSelector,
        bool ascending)
    {
        if (filter.SortBy is SortByEnum.Latest or SortByEnum.Date)
        {
            return await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
                predicate,
                p => new PaginatedProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Discount = p.DiscountPercentage,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Tags = p.Tags.Select(t => t.TagName).ToImmutableList(),
                    ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary).Select(pi => pi.ImageUrl).FirstOrDefault() ??
                                "",
                    CategoryName = p.Category.CategoryName,
                    CreateAt = p.CreatedAt
                },
                p => p.CreatedAt,
                cursorValue > 0 ? new DateTime(cursorValue) : DateTime.MinValue,
                query => query.Include(p => p.Tags).Include(p => p.Category).Include(p => p.ProductImages),
                pageSize,
                false
            );
        }

        return await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
            predicate,
            p => new PaginatedProduct
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Discount = p.DiscountPercentage,
                Quantity = p.Quantity,
                Price = p.Price,
                Tags = p.Tags.Select(t => t.TagName).ToImmutableList(),
                ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary).Select(pi => pi.ImageUrl).FirstOrDefault() ?? "",
                CategoryName = p.Category.CategoryName,
                CreateAt = p.CreatedAt
            },
            sortSelector,
            cursorValue,
            query => query.Include(p => p.Tags).Include(p => p.Category).Include(p => p.ProductImages),
            pageSize,
            ascending
        );
    }

    private long? GetNextCursor(IReadOnlyList<PaginatedProduct> products, bool isLatest, int pageSize)
    {
        if (products.Count < pageSize) return null;
        return isLatest ? products.Last().CreateAt.Ticks : products.Last().ProductId;
    }
}