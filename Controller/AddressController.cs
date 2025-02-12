using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.response.AddressResponse;
using Ecommerce_site.Service.IService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("{id:long}/")]
    public async Task<ActionResult<ApiStandardResponse<AddressResponse>>> GetAddressById(long id)
    {
        var response = await _addressService.GetAddressByAddressIdAsync(id);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    public async Task<ActionResult<ApiStandardResponse<AddressResponse>>> CreateAddress(AddressCreationRequest request)
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

        var response = await _addressService.CreateAddressAsync(request);
        if (response.StatusCode != StatusCodes.Status201Created) return StatusCode(response.StatusCode, response);

        return CreatedAtAction(nameof(GetAddressById), new { Id = response.Data!.AddressId }, response);
    }
    
    
}