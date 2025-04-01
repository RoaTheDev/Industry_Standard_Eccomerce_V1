using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Service;

public class ProductTagService : IProductTagService
{

    private readonly IGenericRepo<Product> _productRepo;
    private readonly IGenericRepo<Tag> _tagRepo;
    public ProductTagService(IGenericRepo<Product> productRepo, IGenericRepo<Tag> tagRepo)
    {
        _productRepo = productRepo;
        _tagRepo = tagRepo;
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> AddTagsToProduct(long productId,
        AddTagToProductRequest request)
    {
        if (!request.TagIds.Any())
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest,
                "No tag IDs provided");

        var product = await _productRepo.GetByConditionAsync(
            p => p.ProductId == productId && !p.IsDeleted,
            p => p.Include(pt => pt.Tags)
        );

        if (product is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                $"The product with ID {productId} does not exist");

        var tagsToAdd = await _tagRepo.GetAllByConditionAsync(
            t => request.TagIds.Contains(t.TagId) && !t.IsDeleted
        );

        if (tagsToAdd.Count != request.TagIds.Count)
        {
            var missingTagIds = request.TagIds.Except(tagsToAdd.Select(t => t.TagId)).ToList();
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                $"The following tag IDs were not found: {string.Join(", ", missingTagIds)}");
        }

        var existingTagIds = product.Tags.Select(t => t.TagId).ToList();
        var newTags = tagsToAdd.Where(t => !existingTagIds.Contains(t.TagId)).ToList();

        if (newTags.Count == 0)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK,
                new ConfirmationResponse
                {
                    Message = "All specified tags are already associated with the product"
                });
        }

        foreach (var tag in newTags)
        {
            product.Tags.Add(tag);
        }

        await _productRepo.UpdateAsync(product);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK,
            new ConfirmationResponse
            {
                Message =
                    $"Successfully added {newTags.Count} tag(s) to the product: {string.Join(", ", newTags.Select(t => t.TagName))}"
            });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(long id,
        ProductTagRemoveRequest request)
    {
        var product = await _productRepo.GetByConditionAsync(
            p => p.ProductId == id && !p.IsDeleted,
            p => p.Include(pt => pt.Tags)
        );

        if (product is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The product does not exist ");

        var tagsToRemove = product.Tags.Where(t => request.TagIds.Contains(t.TagId)).ToList();
        if (!tagsToRemove.Any())
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The tag does not exist");
        foreach (var tag in tagsToRemove)
        {
            product.Tags.Remove(tag);
        }

        await _productRepo.UpdateAsync(product);
        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "Product tag remove successfully"
        });
    }
}