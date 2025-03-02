using AutoMapper;
using Ecommerce_site.Dto.response.AddressResponse;
using Ecommerce_site.Model;

namespace Ecommerce_site.Mapper;

public class AddressMapper : Profile
{
    public AddressMapper()
    {
        CreateMap<Address, AddressResponse>();
    }
}