using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.response.AddressResponse;
using Ecommerce_site.Service.IService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/customer/{customerId:long}/addresses")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly IValidator<AddressCreationRequest> _addressCreateValidator;
    private readonly IValidator<AddressUpdateRequest> _addressUpdateValidator;

    public AddressController(IAddressService addressService, IValidator<AddressCreationRequest> addressCreateValidator,
        IValidator<AddressUpdateRequest> addressUpdateValidator)
    {
        _addressService = addressService;
        _addressCreateValidator = addressCreateValidator;
        _addressUpdateValidator = addressUpdateValidator;
    }

    [HttpGet("{addressId:long}/")]
    public async Task<ActionResult<ApiStandardResponse<AddressResponse>>> GetAddressById([FromRoute] long customerId,
        [FromRoute] long addressId)
    {
        var response = await _addressService.GetAddressByAddressIdAsync(customerId, addressId);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiStandardResponse<AddressResponse>>> CreateAddress([FromRoute] long customerId,
        [FromBody] AddressCreationRequest request)
    {
        var validationResult = await _addressCreateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<AddressResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _addressService.CreateAddressAsync(customerId, request);
        if (response.StatusCode != StatusCodes.Status201Created) return StatusCode(response.StatusCode, response);

        return CreatedAtAction(nameof(GetAddressById),
            new { customerId, addressId = response.Data!.AddressId }, response);
    }

    [HttpPatch]
    public async Task<ActionResult<ApiStandardResponse<AddressResponse>>> UpdateAddress([FromRoute] long customerId,
        [FromBody] AddressUpdateRequest request)
    {
        var validationResult = await _addressUpdateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<AddressResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _addressService.UpdateAddressAsync(customerId, request);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiStandardResponse<IEnumerable<AddressResponse>>>> GetAllAddressFromCustomerId(
        [FromRoute(Name = "customerId")] long id)
    {
        var response = await _addressService.GetAddressListByCustomerIdAsync(id);
        
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpDelete("{addressId:long}/")]
    public async Task<ActionResult<ConfirmationResponse>> DeleteAddressById([FromRoute] long customerId,
        [FromRoute] long addressId)
    {
        var response = await _addressService.DeleteAddressAsync(customerId, addressId);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);
        return Ok(response);
    }
}