using AutoMapper;
using Ecommerce_site.Dto.response.CategoryResponse;
using Ecommerce_site.Model;

namespace Ecommerce_site.Mapper;

public class CategoryMapper : Profile
{
    public CategoryMapper()
    {
        CreateMap<Category, CategoryCreateResponse>();
        CreateMap<Category, CategoryResponse>();
    }
}