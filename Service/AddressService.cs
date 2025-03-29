using AutoMapper;
using Ecommerce_site.Data;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.response.AddressResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;

namespace Ecommerce_site.Service;

public class AddressService : IAddressService
{
    private readonly IGenericRepo<Address> _addressRepo;
    private readonly IGenericRepo<Customer> _customerRepo;
    private readonly EcommerceSiteContext _dbContext;
    private readonly IMapper _mapper;

    public AddressService(IGenericRepo<Address> addressRepo, IGenericRepo<Customer> customerRepo, IMapper mapper,
        EcommerceSiteContext dbContext)
    {
        _addressRepo = addressRepo;
        _customerRepo = customerRepo;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<ApiStandardResponse<AddressResponse?>> GetAddressByAddressIdAsync(long customerId, long addressId)
    {
        var address = await _addressRepo.GetSelectedColumnsByConditionAsync(
            addr => addr.AddressId == addressId && addr.CustomerId == customerId && !addr.IsDeleted,
            addr => new AddressResponse
            {
                AddressId = addr.AddressId,
                CustomerId = addr.CustomerId,
                Country = addr.Country,
                City = addr.City,
                State = addr.State,
                PostalCode = addr.PostalCode,
                FirstAddressLine = addr.FirstAddressLine,
                SecondAddressLine = addr.SecondAddressLine,
            });
        if (address is null)
            return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status404NotFound,
                "the address does not exist");
        return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status200OK, address);
    }

    public async Task<ApiStandardResponse<IEnumerable<AddressResponse>?>> GetAddressListByCustomerIdAsync(
        long customerId)
    {
        var addressList = await _addressRepo.GetSelectedColumnsListsByConditionAsync(
            addr => addr.CustomerId == customerId,
            addr => new AddressResponse
            {
                AddressId = addr.AddressId,
                CustomerId = addr.CustomerId,
                Country = addr.Country,
                City = addr.City,
                State = addr.State,
                PostalCode = addr.PostalCode,
                FirstAddressLine = addr.FirstAddressLine,
                SecondAddressLine = addr.SecondAddressLine,
            });

        if (!addressList.Any())
            return new ApiStandardResponse<IEnumerable<AddressResponse>?>(StatusCodes.Status404NotFound,
                "the user does not have any address");

        return new ApiStandardResponse<IEnumerable<AddressResponse>?>(StatusCodes.Status200OK, addressList);
    }

    public async Task<ApiStandardResponse<AddressResponse?>> CreateAddressAsync(long customerId,
        AddressCreationRequest request)
    {
        if (!await _customerRepo.EntityExistByConditionAsync(c => c.CustomerId == customerId))
            return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status404NotFound,
                "the user does not exist");

        bool hasDefaultAddress =
            await _addressRepo.EntityExistByConditionAsync(addr =>
                addr.CustomerId == customerId && addr.IsDefault == true);

        Address address = await _addressRepo.AddAsync(new Address
        {
            CustomerId = customerId,
            Country = request.Country,
            State = request.State,
            City = request.City,
            IsDeleted = false,
            PostalCode = request.PostalCode,
            SecondAddressLine = request.SecondAddressLine,
            FirstAddressLine = request.FirstAddressLine,
            IsDefault = !hasDefaultAddress
        });
        AddressResponse addressResponse = _mapper.Map<AddressResponse>(address);
        return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status201Created, addressResponse);
    }

    public async Task<ApiStandardResponse<AddressResponse?>> UpdateAddressAsync(long customerId, long addressId,
        AddressUpdateRequest request)
    {
        Address? address = await _addressRepo.GetByConditionAsync(
            addr => addr.AddressId == addressId && addr.CustomerId == customerId, false);

        if (address is null)
            return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status404NotFound,
                "the address does not exist");

        if (!string.IsNullOrWhiteSpace(request.FirstAddressLine) &&
            request.FirstAddressLine != address.FirstAddressLine)
            address.FirstAddressLine = request.FirstAddressLine;
        if (request.SecondAddressLine != address.SecondAddressLine)
            address.SecondAddressLine = request.SecondAddressLine;
        if (!string.IsNullOrWhiteSpace(request.PostalCode) && request.PostalCode != address.PostalCode)
            address.PostalCode = request.PostalCode;
        if (string.IsNullOrWhiteSpace(request.City) && request.City != address.City)
            address.City = request.City;
        if (string.IsNullOrWhiteSpace(request.State) && request.State != address.State)
            address.State = request.State;
        if (string.IsNullOrWhiteSpace(request.Country) && request.Country != address.Country)
            address.Country = request.Country;

        await _addressRepo.UpdateAsync(address);
        AddressResponse addressResponse = _mapper.Map<AddressResponse>(address);
        return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status200OK, addressResponse);
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> ChangeDefaultAddress(long customerId, long addressId)
    {
        var currentDefaultAddr =
            await _addressRepo.GetByConditionAsync(addr => addr.CustomerId == customerId && addr.IsDefault);
        if (currentDefaultAddr is null)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "There is no default address");
        }

        var newDefaultAddr = await _addressRepo.GetByIdAsync(addressId);

        if (newDefaultAddr is null)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The address you selected does not exist");
        }

        if (currentDefaultAddr.AddressId == addressId)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest,
                "This address is already set as the default.");
        }

        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                currentDefaultAddr.IsDefault = false;
                newDefaultAddr.IsDefault = true;
                await _addressRepo.UpdateBulk(new List<Address> { currentDefaultAddr, newDefaultAddr });
                await transaction.CommitAsync();
                return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
                {
                    Message = "The default address has been changed"
                });
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> DeleteAddressAsync(long customerId, long addressId)
    {
        var address =
            await _addressRepo.GetByConditionAsync(
                addr => addr.AddressId == addressId && addr.CustomerId == customerId,
                false);

        if (address is null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                "the address does not exist");

        address.IsDeleted = true;

        await _addressRepo.UpdateAsync(address);

        return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK,
            new ConfirmationResponse { Message = "The address is deleted" });
    }
}