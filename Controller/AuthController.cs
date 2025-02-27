using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public AuthController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<CustomerGetByIdResponse>> GetCustomerById([FromRoute] long id)
    {
        var response = await _customerService.GetCustomerByIdAsync(id);
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


    [HttpPost("register/")]
    public async Task<ActionResult<CustomerRegisterRequestUap>> Register(
        CustomerRegisterRequestUap request)
    {
        var response = await _customerService.RegisterCustomerAsync(request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return Accepted(response.Data);
    }

    [HttpPost("email-verification/")]
    public async Task<ActionResult<CustomerCreationResponse>> EmailVerification(
        [FromHeader(Name = "Auth-Session-Token")]
        Guid session, [FromBody] EmailVerificationRequest request)
    {
        var response = await _customerService.EmailVerification(session, request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(Response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        HttpContext.Response.Cookies.Append("AuthToken", response.Data!.Token.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });
        return CreatedAtAction(nameof(GetCustomerById), new { Id = response.Data.UserId }, response.Data);
    }

    [HttpPost("login/")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequestUap request)
    {
        var response = await _customerService.LoginAsync(request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        HttpContext.Response.Cookies.Append("AuthToken", response.Data!.Token.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });
        return Ok(response.Data);
    }
}