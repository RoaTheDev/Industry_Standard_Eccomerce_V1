using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;
using Ecommerce_site.Service.IService;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IValidator<CustomerRegisterRequestUap> _registerValidator;
    private readonly IValidator<LoginRequestUap> _loginValidator;

    public AuthController(ICustomerService customerService,
        IValidator<CustomerRegisterRequestUap> registerValidator,
        IValidator<LoginRequestUap> loginValidator)
    {
        _customerService = customerService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiStandardResponse<CustomerGetByIdResponse>>> GetCustomerById([FromRoute] long id)
    {
        var response = await _customerService.GetCustomerByIdAsync(id);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }


    [HttpPost("register/")]
    public async Task<ActionResult<ApiStandardResponse<CustomerRegisterRequestUap>>> Register(
        CustomerRegisterRequestUap request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<CustomerRegisterRequestUap?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _customerService.RegisterCustomerAsync(request);
        if (response.StatusCode != StatusCodes.Status202Accepted) return StatusCode(response.StatusCode, response);

        return Accepted(response);
    }

    [HttpPost("email-verification/")]
    public async Task<ActionResult<ApiStandardResponse<CustomerCreationResponse>>> EmailVerification(
        [FromHeader(Name = "Auth-Session-Token")]
        Guid session, [FromBody] EmailVerificationRequest request)
    {
        var response = await _customerService.EmailVerification(session, request);
        if (response.StatusCode != StatusCodes.Status201Created) return StatusCode(response.StatusCode, response);

        HttpContext.Response.Cookies.Append("AuthToken", response.Data!.Token.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });
        return CreatedAtAction(nameof(GetCustomerById), new { Id = response.Data.UserId }, response);
    }

    [HttpPost("login/")]
    public async Task<ActionResult<ApiStandardResponse<LoginResponse>>> Login(LoginRequestUap request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<LoginResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _customerService.LoginAsync(request);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        HttpContext.Response.Cookies.Append("AuthToken", response.Data!.Token.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        });
        return Ok(response);
    }
}