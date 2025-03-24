using System.Security.Claims;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;
using Ecommerce_site.Model.Enum;
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
        var response = await _customerService.EmailVerificationAsync(session, request);
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

    [HttpPost("signin-google")]
    public async Task<ActionResult<ApiStandardResponse<LoginResponse>>> GoogleLogin(
        [FromBody] GoogleLoginRequest request)
    {
        var response = await _customerService.LoginWithGoogle(request);
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

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest request)
    {
        var response = await _customerService.ForgotPasswordAsync(request);
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = GetStatusTitle.GetTitleForStatus(StatusCodes.Status400BadRequest),
                Detail = "Validation errors",
                Extensions = { ["errors"] = errors }
            });
        }

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

    [HttpPost("reset-password/{session}")]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromRoute] string session,
        [FromBody] ResetPasswordRequest request)
    {
        var response = await _customerService.ResetPasswordAsync(request, session);
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = GetStatusTitle.GetTitleForStatus(StatusCodes.Status400BadRequest),
                Detail = "Validation errors",
                Extensions = { ["errors"] = errors }
            });
        }

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

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "Invalid user ID"
            });
        }

        var response = await _customerService.LogoutAsync(userId);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        HttpContext.Response.Cookies.Delete("AuthToken");

        return Ok(response.Data);
    }

    [Authorize(Policy = "Any")]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var response = await _customerService.GetCustomerByIdAsync(userId);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        var currentUser = new CurrentUserResponse
        {
            CustomerId = response.Data!.CustomerId,
            DisplayName = $"{response.Data.FirstName} {response.Data.MiddleName} {response.Data.LastName}".Trim(),
            Email = response.Data.Email
        };
        return Ok(currentUser);
    }
}