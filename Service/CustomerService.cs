using System.Runtime.InteropServices;
using Ecommerce_site.Cache;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerDto;
using Ecommerce_site.Dto.response.CustomerDto;
using Ecommerce_site.Email;
using Ecommerce_site.Email.Msg;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class CustomerService : ICustomerService
{
    private readonly IGenericRepo<Customer> _customerRepo;
    private readonly IGenericRepo<User> _userRepo;
    private readonly IGenericRepo<Role> _roleRepo;
    private readonly OtpGenerator _otpGenerator;
    private readonly ILogger _logger;
    private readonly CustomPasswordHasher _passwordHasher;
    private readonly RedisCaching _cache;
    private readonly IFluentEmail _fluentEmail;
    private readonly RazorPageRenderer _razorPageRenderer;
    private readonly JwtGenerator _jwtGenerator;
    private const string RegisterAccountKey = "_customer.payload.";
    private const string RedisRefreshKey = "_customer.";
    private const string OtpVerificationKey = "_email.otp.";

    public CustomerService(IGenericRepo<Customer> customerRepo, ILogger logger, CustomPasswordHasher passwordHasher,
        RedisCaching cache, IGenericRepo<User> userRepo, IGenericRepo<Role> roleRepo, OtpGenerator otpGenerator,
        IFluentEmail fluentEmail, RazorPageRenderer razorPageRenderer, JwtGenerator jwtGenerator)
    {
        _customerRepo = customerRepo;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _cache = cache;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _otpGenerator = otpGenerator;
        _fluentEmail = fluentEmail;
        _razorPageRenderer = razorPageRenderer;
        _jwtGenerator = jwtGenerator;
    }

    private async Task SendEmailAsync(EmailMetadata emailMetadata, EmailVerificationMsg model)
    {
        var emailBody = await _razorPageRenderer.RenderTemplateAsync(emailMetadata.TemplatePath, model);
        var email = await _fluentEmail.To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .Body(emailBody, isHtml: true)
            .SendAsync();

        if (email.Successful)
        {
            _logger.Information("Email sent successfully!");
        }
        else
        {
            _logger.Error("Failed to send email.");
            foreach (var error in email.ErrorMessages)
            {
                _logger.Error(error);
                throw new ExternalException("External service not responding");
            }
        }
    }

    public async Task<ApiStandardResponse<CustomerGetByIdResponse?>> GetCustomerByIdAsync(long id)
    {
        try
        {
            var customer = await _userRepo.GetSelectedColumnsByConditionAsync(u => u.UserId == id,
                u => new
                {
                    u.Email,
                    u.UserId,
                    u.Customer!.PhoneNumber,
                    u.Customer.Dob,
                    u.FirstName,
                    u.LastName,
                    u.MiddleName,
                    u.Gender,
                }, include => include.Include(u => u.Customer)!
            );

            CustomerGetByIdResponse response = new CustomerGetByIdResponse
            {
                Dob = customer.Dob,
                Email = customer.Email,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                MiddleName = customer.MiddleName,
                Gender = customer.Gender,
                PhoneNumber = customer.PhoneNumber, CustomerId = customer.UserId
            };
            return new ApiStandardResponse<CustomerGetByIdResponse?>(StatusCodes.Status200OK, response);
        }
        catch (EntityNotFoundException e)
        {
            _logger.Error(e, $"The user with the id : {id} not found");
            return new ApiStandardResponse<CustomerGetByIdResponse?>(StatusCodes.Status404NotFound,
                "the registration session has ended", null);
        }
    }


    public async Task<ApiStandardResponse<CustomerRegisterResponse?>> RegisterCustomerAsync(
        CustomerRegisterRequestUap request)
    {
        try
        {
            if (await _userRepo.EntityExistByConditionAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                _logger.Warning("The user already exist.");
                return new ApiStandardResponse<CustomerRegisterResponse?>(StatusCodes.Status409Conflict,
                    "The user Already exist", null);
            }

            if (request.Password != request.ConfirmPassword)
            {
                _logger.Warning("The password does not match.");
                return new ApiStandardResponse<CustomerRegisterResponse?>(StatusCodes.Status400BadRequest,
                    "The  password does not match", null);
            }

            uint otp = _otpGenerator.GenerateSecureOtp();
            Guid session = Guid.NewGuid();

            await _cache.SetAsync($"{OtpVerificationKey}{session}", otp, TimeSpan.FromMinutes(15));
            await _cache.SetAsync($"{RegisterAccountKey}{session}", new UserCreationCache
            {
                Gender = request.Gender ?? GenderEnum.Male.ToString().ToLower(),
                Dob = request.Dob,
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName ?? "",
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            }, TimeSpan.FromMinutes(15));

            EmailMetadata emailMetadata = new EmailMetadata
            {
                Subject = "Email Verification",
                ToAddress = request.Email,
                TemplatePath = nameof(EmailVerification)
            };

            EmailVerificationMsg emailMsg = new EmailVerificationMsg
            {
                VerificationCode = otp,
                VerificationExpTime = 15
            };

            await SendEmailAsync(emailMetadata, emailMsg);
            return new ApiStandardResponse<CustomerRegisterResponse?>(StatusCodes.Status202Accepted,
                new CustomerRegisterResponse
                {
                    Session = session,
                    SignUpSessionExpAt = DateTime.UtcNow.AddMinutes(15).ToString("O")
                });
        }
        catch (ExternalException e)
        {
            _logger.Error(e, "The email service not responding");
            return new ApiStandardResponse<CustomerRegisterResponse?>(StatusCodes.Status503ServiceUnavailable,
                "External service not responding", null);
        }
    }

    public async Task<ApiStandardResponse<CustomerCreationResponse?>> EmailVerification(Guid session,
        EmailVerificationRequest request)
    {
        uint otp = await _cache.GetAsync<uint>($"{OtpVerificationKey}{session}");
        var customerRegisterObj = await _cache.GetAsync<UserCreationCache>($"{RegisterAccountKey}{session}");

        if (otp == 0)
        {
            _logger.Debug("The otp code has expired");
            return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status404NotFound,
                "The otp has expired", null);
        }

        if (otp != request.Otp)
        {
            _logger.Debug("The otp code does not match");
            return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status400BadRequest,
                "The code does not match", null);
        }

        if (customerRegisterObj == null)
        {
            return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status404NotFound,
                "the registration session has ended", null);
        }

        var role = await _roleRepo.GetSelectedColumnsByConditionAsync(
            r => EF.Functions.Like(r.RoleName, RoleEnums.Customer.ToString()),
            r => new { r.RoleName, r.RoleId });

        User createdUser = await _userRepo.AddAsync(
            new User
            {
                Gender = customerRegisterObj.Gender,
                FirstName = customerRegisterObj.FirstName,
                MiddleName = customerRegisterObj.MiddleName,
                LastName = customerRegisterObj.LastName,
                Email = customerRegisterObj.Email,
                DisplayName =
                    $"{customerRegisterObj.FirstName} {customerRegisterObj.MiddleName} {customerRegisterObj.LastName}",
                IsActive = true,
                PasswordHashed = _passwordHasher.HashPassword(customerRegisterObj.Password),
                RoleId = role.RoleId
            });

        await _customerRepo.AddAsync(new Customer
        {
            CustomerId = createdUser.UserId,
            PhoneNumber = customerRegisterObj.PhoneNumber,
            Dob = customerRegisterObj.Dob
        });

        string accessToken =
            _jwtGenerator.GenerateAccessToken(createdUser.UserId.ToString(), createdUser.Email, role.RoleName);
        string refreshToken = _jwtGenerator.GenerateRefreshToken();

        await _cache.RemoveAsync($"{OtpVerificationKey}{session}");
        await _cache.RemoveAsync($"{RegisterAccountKey}{session}");
        await _cache.SetAsync($"{RedisRefreshKey}{createdUser.UserId}", refreshToken);

        return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status201Created,
            new CustomerCreationResponse
            {
                UserId  = createdUser.UserId,
                Gender = createdUser.Gender,
                Dob = customerRegisterObj.Dob,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                MiddleName = createdUser.MiddleName,
                LastName = createdUser.LastName,
                PhoneNumber = customerRegisterObj.PhoneNumber,
                Token = new TokenResponse
                {
                    Token = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15).ToString("O")
                }
            });
    }

    public async Task<ApiStandardResponse<LoginResponse?>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepo.GetSelectedColumnsByConditionAsync(u => u.Email == request.Email,
                res => new { res.DisplayName, res.UserId, res.PasswordHashed, res.Role.RoleName },
                egu => egu.Include(u => u.Role));

            if (user.PasswordHashed == null)
            {
                return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status404NotFound,
                    "The account does not exist",
                    null);
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHashed))
            {
                return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status401Unauthorized,
                    "The user credential is not valid",
                    null);
            }

            string accessToken =
                _jwtGenerator.GenerateAccessToken(user.UserId.ToString(), request.Email, user.RoleName);
            string refreshToken = _jwtGenerator.GenerateRefreshToken();

            await _cache.SetAsync($"{RedisRefreshKey}{user.UserId}", refreshToken);

            return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status200OK, new LoginResponse
            {
                Token = new TokenResponse
                {
                    Token = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15).ToString("O")
                },
                Message = "Login success",
                DisplayName = user.DisplayName,
                CustomerId = user.UserId
            });
        }
        catch (EntityNotFoundException)
        {
            return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status404NotFound,
                "The account does not exist", null);
        }
    }
}