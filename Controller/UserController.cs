using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;
using Ecommerce_site.Service.IService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IValidator<CustomerUpdateRequest> _updateValidator;
    private readonly IValidator<PasswordChangeRequest> _passwordChangeValidator;

    public UserController(IValidator<CustomerUpdateRequest> updateValidator, ICustomerService customerService,
        IValidator<PasswordChangeRequest> passwordChangeValidator)
    {
        _updateValidator = updateValidator;
        _customerService = customerService;
        _passwordChangeValidator = passwordChangeValidator;
    }

    [HttpPatch("{id:long}/")]
    public async Task<ActionResult<ApiStandardResponse<CustomerUpdateResponse>>> UpdateInfo([FromRoute] long id,
        [FromBody] CustomerUpdateRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<CustomerUpdateResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _customerService.UpdateCustomerInfoAsync(id, request);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpPatch("{id:long}/password-change")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> PasswordChange([FromRoute] long id,
        [FromBody] PasswordChangeRequest request)
    {
        var validationResult = await _passwordChangeValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _customerService.PasswordChangeAsync(id, request);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);
        return Ok(response);
    }
}