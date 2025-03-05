using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.response.AddressResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/customer/{customerId:long}/addresses")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet("{addressId:long}/")]
    public async Task<ActionResult<AddressResponse>> GetAddressById([FromRoute] long customerId,
        [FromRoute] long addressId)
    {
        var response = await _addressService.GetAddressByAddressIdAsync(customerId, addressId);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return Ok(response.Data);
    }

    [HttpPost]
    public async Task<ActionResult<AddressResponse>> CreateAddress([FromRoute] long customerId,
        [FromBody] AddressCreationRequest request)
    {
        var response = await _addressService.CreateAddressAsync(customerId, request);
        if (!response.Success)
        {
            var problemDetails = new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            };
            return StatusCode(response.StatusCode, problemDetails);
        }

        return CreatedAtAction(nameof(GetAddressById),
            new { customerId, addressId = response.Data!.AddressId }, response.Data);
    }

    [HttpPatch]
    public async Task<ActionResult<AddressResponse>> UpdateAddress([FromRoute] long customerId,
        [FromBody] AddressUpdateRequest request)
    {
        var response = await _addressService.UpdateAddressAsync(customerId, request);
        if (!response.Success)
        {
            var problemDetails = new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.FirstOrDefault()!.ToString()
            };
            return StatusCode(response.StatusCode, problemDetails);
        }

        return Ok(response.Data);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressResponse>>> GetAllAddressFromCustomerId(
        [FromRoute(Name = "customerId")] long id)
    {
        var response = await _addressService.GetAddressListByCustomerIdAsync(id);

        if (!response.Success)
        {
            var problemDetails = new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.FirstOrDefault()!.ToString()
            };
            return StatusCode(response.StatusCode, problemDetails);
        }

        return Ok(response.Data);
    }

    [HttpDelete("{addressId:long}/")]
    public async Task<ActionResult<ConfirmationResponse>> DeleteAddressById([FromRoute] long customerId,
        [FromRoute] long addressId)
    {
        var response = await _addressService.DeleteAddressAsync(customerId, addressId);
        if (!response.Success)
        {
            var problemDetails = new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.FirstOrDefault()!.ToString()
            };
            return StatusCode(response.StatusCode, problemDetails);
        }

        return Ok(response.Data);
    }
}