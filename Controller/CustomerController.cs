using System.Security.Claims;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPatch("{id:long}/")]
    public async Task<ActionResult<CustomerUpdateResponse>> UpdateInfo([FromRoute] long id,
        [FromBody] CustomerUpdateRequest request)
    {
        var response = await _customerService.UpdateCustomerInfoAsync(id, request);
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

    [HttpPatch("{id:long}/password-change/")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> PasswordChange([FromRoute] long id,
        [FromBody] PasswordChangeRequest request)
    {
        var response = await _customerService.PasswordChangeAsync(id, request);
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

    [HttpPatch("{id:long}/profile/")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> ProfileChange([FromRoute] long id,
        [FromForm] FormFile file)
    {
        var response = await _customerService.ChangeProfileImage(id, file);
        if (!response.Success)
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        return Ok(response.Data);
    }

    [HttpPost("link/google")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse?>>> LinkGoogleAccount(
        [FromBody] LinkGoogleRequest request)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _customerService.LinkGoogleAccount(userId, request.IdToken);
        return !response.Success
            ? StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            })
            : Ok(response.Data);
    }

    [HttpDelete("unlink/{providerName:alpha}/{providerId:alpha}")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse?>>> UnlinkProvider(
        [FromRoute] string providerName, [FromRoute] string providerId)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _customerService.UnlinkProvider(userId, providerId, providerName);
        return !response.Success
            ? StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            })
            : Ok(response.Data);
    }

    [HttpGet("providers")]
    public async Task<ActionResult<ApiStandardResponse<List<AuthProviderResponse>?>>> GetLinkedProviders()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var response = await _customerService.GetLinkedProvidersAsync(userId);
        return !response.Success
            ? StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            })
            : Ok(response.Data);
    }
}