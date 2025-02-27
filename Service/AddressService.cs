﻿using Ecommerce_site.Dto;
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

    public AddressService(IGenericRepo<Address> addressRepo, IGenericRepo<Customer> customerRepo)
    {
        _addressRepo = addressRepo;
        _customerRepo = customerRepo;
    }

    public async Task<ApiStandardResponse<AddressResponse?>> GetAddressByAddressIdAsync(long customerId, long addressId)
    {
        var address = await _addressRepo.GetSelectedColumnsByConditionAsync(
            addr => addr.AddressId == addressId && addr.CustomerId == customerId,
            addr => new
            {
                addr.AddressId,
                addr.CustomerId,
                addr.Country,
                addr.City,
                addr.State,
                addr.PostalCode,
                addr.FirstAddressLine,
                addr.SecondAddressLine,
                addr.IsDeleted
            });
        if (address is null)
            return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status404NotFound,
                "the address does not exist");
        if (address.IsDeleted)
            return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status404NotFound,
                "The address does not exist");
        return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status200OK, new AddressResponse
        {
            AddressId = address.AddressId,
            CustomerId = address.CustomerId,
            Country = address.Country,
            State = address.State,
            City = address.City,
            PostalCode = address.PostalCode,
            FirstAddressLine = address.FirstAddressLine,
            SecondAddressLine = address.SecondAddressLine
        });
    }

    public async Task<ApiStandardResponse<IEnumerable<AddressResponse>?>> GetAddressListByCustomerIdAsync(
        long customerId)
    {
        var addressList = await _addressRepo.GetSelectedColumnsListsByConditionAsync(
            addr => addr.CustomerId == customerId,
            addr => new
            {
                addr.AddressId,
                addr.CustomerId,
                addr.Country,
                addr.City,
                addr.State,
                addr.PostalCode,
                addr.FirstAddressLine,
                addr.SecondAddressLine,
            });


        if (!addressList.Any())
            return new ApiStandardResponse<IEnumerable<AddressResponse>?>(StatusCodes.Status404NotFound,
                "the user does not have any address");

        List<AddressResponse> response = new List<AddressResponse>();

        foreach (var address in addressList)
        {
            response.Add(new AddressResponse
            {
                AddressId = address.AddressId,
                CustomerId = address.CustomerId,
                Country = address.Country,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                FirstAddressLine = address.FirstAddressLine,
                SecondAddressLine = address.SecondAddressLine
            });
        }

        return new ApiStandardResponse<IEnumerable<AddressResponse>?>(StatusCodes.Status200OK, response);
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

        return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status201Created, new AddressResponse
        {
            AddressId = address.AddressId,
            CustomerId = address.CustomerId,
            Country = address.Country,
            State = address.State,
            City = address.City,
            PostalCode = address.PostalCode,
            FirstAddressLine = address.FirstAddressLine,
            SecondAddressLine = address.SecondAddressLine
        });
    }

    public async Task<ApiStandardResponse<AddressResponse?>> UpdateAddressAsync(long customerId,
        AddressUpdateRequest request)
    {
        var address =
            await _addressRepo.GetByConditionAsync(
                addr => addr.AddressId == request.AddressId && addr.CustomerId == customerId, false);

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

        return new ApiStandardResponse<AddressResponse?>(StatusCodes.Status200OK, new AddressResponse
        {
            AddressId = address.AddressId,
            CustomerId = address.CustomerId,
            Country = address.Country,
            State = address.State,
            City = address.City,
            PostalCode = address.PostalCode,
            FirstAddressLine = address.FirstAddressLine,
            SecondAddressLine = address.SecondAddressLine
        });
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