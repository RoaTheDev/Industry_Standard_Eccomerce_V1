using System.Runtime.InteropServices;
using Ecommerce_site.Cache;
using Ecommerce_site.Data;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;
using Ecommerce_site.Email;
using Ecommerce_site.Email.Msg;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Ecommerce_site.Util.storage;
using FluentEmail.Core;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class CustomerService : ICustomerService
{
    private const string RegisterAccountKey = "_customer.payload.";
    private const string RedisRefreshKey = "_customer.";
    private const string OtpVerificationKey = "_email.otp.";
    private const string PasswordResetTokenKey = "_password.reset.";
    private readonly RedisCaching _cache;
    private readonly IGenericRepo<Customer> _customerRepo;
    private readonly IFluentEmail _fluentEmail;
    private readonly IConfiguration _config;
    private readonly JwtGenerator _jwtGenerator;
    private readonly ILogger _logger;
    private readonly OtpGenerator _otpGenerator;
    private readonly CustomPasswordHasher _passwordHasher;
    private readonly RazorPageRenderer _razorPageRenderer;
    private readonly IGenericRepo<Role> _roleRepo;
    private readonly IGenericRepo<User> _userRepo;
    private readonly IGenericRepo<AuthProvider> _provider;
    private readonly IStorageProvider _storageProvider;
    private readonly EcommerceSiteContext _dbContext;

    public CustomerService(IGenericRepo<Customer> customerRepo, ILogger logger, CustomPasswordHasher passwordHasher,
        RedisCaching cache, IGenericRepo<User> userRepo, IGenericRepo<Role> roleRepo, OtpGenerator otpGenerator,
        IFluentEmail fluentEmail, RazorPageRenderer razorPageRenderer, JwtGenerator jwtGenerator,
        EcommerceSiteContext dbContext, IConfiguration config, IGenericRepo<AuthProvider> provider,
        [FromKeyedServices("local")] IStorageProvider storageProvider)
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
        _dbContext = dbContext;
        _config = config;
        _provider = provider;
        _storageProvider = storageProvider;
    }

    public async Task<ApiStandardResponse<LoginResponse?>> LoginWithGoogle(GoogleLoginRequest request)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_config["CLIENT_ID"]]
                });
        }
        catch (InvalidJwtException)
        {
            return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status401Unauthorized,
                "Invalid Google token");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await _userRepo.GetByConditionAsync(u =>
                u.Email == payload.Email, uIn => uIn
                .Include(ur => ur.Role)
                .Include(ua => ua.AuthProviders));
            if (user is not null)
            {
                var userAuth = user.AuthProviders.FirstOrDefault(ua =>
                    ua.AuthProviderId == payload.Subject &&
                    ua.ProviderName.Contains(AuthProviderEnum.Google.ToString()));
                if (userAuth is null)
                {
                    await _provider.AddAsync(new AuthProvider
                    {
                        AuthProviderId = payload.Subject,
                        ProviderName = AuthProviderEnum.Google.ToString(),
                        IsDeleted = false,
                        UserId = user.UserId
                    });
                }
            }
            else
            {
                var role = await _roleRepo.GetSelectedColumnsByConditionAsync(
                    r => r.RoleName.Contains(RoleEnums.Customer.ToString()),
                    r => new { r.RoleId });

                user = new User
                {
                    RoleId = role!.RoleId,
                    DisplayName = payload.Name,
                    FirstName = payload.GivenName,
                    MiddleName = "",
                    LastName = payload.FamilyName,
                    Email = payload.Email,
                    IsDeleted = false,
                    Gender = GenderEnums.Male.ToString(),
                };

                user = await _userRepo.AddAsync(user);

                await _provider.AddAsync(new AuthProvider
                {
                    AuthProviderId = payload.Subject,
                    ProviderName = AuthProviderEnum.Google.ToString(),
                    IsDeleted = false,
                    UserId = user.UserId
                });

                await _customerRepo.AddAsync(new Customer
                {
                    Dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-19)),
                    IsDeleted = false,
                    CustomerId = user.UserId
                });
            }

            var accessToken = _jwtGenerator.GenerateAccessToken(
                user.UserId.ToString(),
                user.Email,
                RoleEnums.Customer.ToString()
            );
            var refreshToken = _jwtGenerator.GenerateRefreshToken();

            await _cache.SetAsync($"{RedisRefreshKey}{user.UserId}", refreshToken);
            await transaction.CommitAsync();
            return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status200OK, new LoginResponse
            {
                Token = new TokenResponse
                {
                    Token = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1).ToString("O")
                },
                CustomerId = user.UserId,
                DisplayName = user.DisplayName,
                Message = "Google login successful"
            });
        }
        catch (System.Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiStandardResponse<CustomerGetByIdResponse?>> GetCustomerByIdAsync(long id)
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
                u.Profile
            }, include => include.Include(u => u.Customer)!
        );

        if (customer is null)
            return new ApiStandardResponse<CustomerGetByIdResponse?>(StatusCodes.Status404NotFound,
                "The user does not exist");
        var response = new CustomerGetByIdResponse
        {
            Dob = customer.Dob!.Value,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            MiddleName = customer.MiddleName,
            Gender = customer.Gender,
            PhoneNumber = customer.PhoneNumber!, CustomerId = customer.UserId,
            Profile = customer.Profile
        };

        return new ApiStandardResponse<CustomerGetByIdResponse?>(StatusCodes.Status200OK, response);
    }


    public async Task<ApiStandardResponse<CustomerRegisterResponse?>> RegisterCustomerAsync(
        CustomerRegisterRequestUap request)
    {
        try
        {
            if (await _userRepo.EntityExistByConditionAsync(u => u.Email.ToLower() == request.Email!.ToLower()))
            {
                return new ApiStandardResponse<CustomerRegisterResponse?>(StatusCodes.Status409Conflict,
                    "The user Already exist");
            }

            if (request.Password != request.ConfirmPassword)
            {
                return new ApiStandardResponse<CustomerRegisterResponse?>(StatusCodes.Status400BadRequest,
                    "The password does not match");
            }

            var otp = _otpGenerator.GenerateSecureOtp();
            var session = Guid.NewGuid();

            await _cache.SetAsync($"{OtpVerificationKey}{session}", otp, TimeSpan.FromMinutes(15));
            await _cache.SetAsync($"{RegisterAccountKey}{session}", new UserCreationCache
            {
                Gender = request.Gender ?? GenderEnums.Male.ToString().ToLower(),
                Dob = request.Dob,
                Email = request.Email!,
                Password = request.Password!,
                FirstName = request.FirstName!,
                MiddleName = request.MiddleName ?? "",
                LastName = request.LastName!,
                PhoneNumber = request.PhoneNumber
            }, TimeSpan.FromMinutes(15));

            var emailMetadata = new EmailMetadata
            {
                Subject = "Email Verification",
                ToAddress = request.Email!,
                TemplatePath = nameof(EmailVerification)
            };

            var emailMsg = new EmailVerificationMsg
            {
                VerificationCode = otp,
                VerificationExpTime = 15
            };
            _ = SendEmailAsync(emailMetadata, emailMsg);

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
                "External service not responding");
        }
    }

    public async Task<ApiStandardResponse<CustomerCreationResponse?>> EmailVerificationAsync(Guid session,
        EmailVerificationRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var otp = await _cache.GetAsync<uint>($"{OtpVerificationKey}{session}");
            var customerRegisterObj = await _cache.GetAsync<UserCreationCache>($"{RegisterAccountKey}{session}");

            if (otp == 0)
            {
                return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status404NotFound,
                    "The otp has expired");
            }

            if (otp != request.Otp)
            {
                return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status400BadRequest,
                    "The code does not match");
            }

            if (customerRegisterObj == null)
                return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status404NotFound,
                    "the registration session has ended");

            var role = await _roleRepo.GetSelectedColumnsByConditionAsync(
                r => r.RoleName.Contains(RoleEnums.Customer.ToString()),
                r => new { r.RoleName, r.RoleId });

            if (role is null)
                return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status404NotFound,
                    "The customer role does not exist yet");

            var createdUser = await _userRepo.AddAsync(
                new User
                {
                    Gender = customerRegisterObj.Gender,
                    FirstName = customerRegisterObj.FirstName,
                    MiddleName = customerRegisterObj.MiddleName,
                    LastName = customerRegisterObj.LastName,
                    Email = customerRegisterObj.Email,
                    DisplayName =
                        $"{customerRegisterObj.FirstName} {customerRegisterObj.MiddleName} {customerRegisterObj.LastName}",
                    PasswordHashed = _passwordHasher.HashPassword(customerRegisterObj.Password),
                    RoleId = role.RoleId
                });

            await _customerRepo.AddAsync(new Customer
            {
                CustomerId = createdUser.UserId,
                PhoneNumber = customerRegisterObj.PhoneNumber,
                Dob = customerRegisterObj.Dob
            });

            await transaction.CommitAsync();

            var accessToken =
                _jwtGenerator.GenerateAccessToken(createdUser.UserId.ToString(), createdUser.Email, role.RoleName);
            var refreshToken = _jwtGenerator.GenerateRefreshToken();

            await _cache.RemoveAsync($"{OtpVerificationKey}{session}");
            await _cache.RemoveAsync($"{RegisterAccountKey}{session}");
            await _cache.SetAsync($"{RedisRefreshKey}{createdUser.UserId}", refreshToken);

            return new ApiStandardResponse<CustomerCreationResponse?>(StatusCodes.Status201Created,
                new CustomerCreationResponse
                {
                    UserId = createdUser.UserId,
                    Email = createdUser.Email,
                    DisplayName = createdUser.DisplayName,
                    Token = new TokenResponse
                    {
                        Token = accessToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(15).ToString("O")
                    }
                });
        }
        catch (System.Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiStandardResponse<LoginResponse?>> LoginAsync(LoginRequestUap requestUap)
    {
        var user = await _userRepo.GetSelectedColumnsByConditionAsync(u => u.Email == requestUap.Email,
            res => new { res.DisplayName, res.UserId, res.PasswordHashed, res.Role.RoleName },
            egu => egu.Include(u => u.Role));


        if (user is null || user.PasswordHashed == null)
            return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status404NotFound,
                "The account does not exist");

        if (!_passwordHasher.VerifyPassword(requestUap.Password, user.PasswordHashed))
            return new ApiStandardResponse<LoginResponse?>(StatusCodes.Status401Unauthorized,
                "The user credential is not valid");

        var accessToken =
            _jwtGenerator.GenerateAccessToken(user.UserId.ToString(), requestUap.Email, user.RoleName);
        var refreshToken = _jwtGenerator.GenerateRefreshToken();

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

    public async Task<ApiStandardResponse<CustomerUpdateResponse?>> UpdateCustomerInfoAsync(long id,
        CustomerUpdateRequest request)
    {
        var user =
            await _userRepo.GetByConditionAsync(u => u.Customer!.CustomerId == id,
                egl => egl.Include(u => u.Customer)!,
                false);

        if (user is null)
            return new ApiStandardResponse<CustomerUpdateResponse?>(StatusCodes.Status404NotFound,
                "The account does not exist");

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            if (user.Email != request.Email &&
                await _userRepo.EntityExistByConditionAsync(u => u.Email == request.Email))
            {
                return new ApiStandardResponse<CustomerUpdateResponse?>(StatusCodes.Status400BadRequest,
                    "The email already exist");
            }

            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != user.FirstName)
            user.FirstName = request.FirstName;
        if (request.MiddleName != null && request.MiddleName != user.MiddleName)
            user.MiddleName = request.MiddleName;
        if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName != user.LastName)
            user.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.Gender) && request.Gender != user.Gender)
            user.Gender = request.Gender;
        if (request.Dob != null)
        {
            user.Customer!.Dob = request.Dob.Value;
        }

        await _userRepo.UpdateAsync(user);

        return new ApiStandardResponse<CustomerUpdateResponse?>(StatusCodes.Status200OK,
            new CustomerUpdateResponse
            {
                Dob = user.Customer!.Dob.ToString()!,
                Email = user.Email,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                PhoneNUmber = user.Customer.PhoneNumber!,
                Gender = user.Gender
            });
    }


    public async Task<ApiStandardResponse<ForgotPasswordResponse?>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userRepo.GetByConditionAsync(u => u.Email.ToLower() == request.Email!.ToLower());

            if (user is null)
                return new ApiStandardResponse<ForgotPasswordResponse?>(StatusCodes.Status404NotFound,
                    "No account with this email exists");

            var resetSession = Guid.NewGuid().ToString();
            var tokenExpiry = DateTime.UtcNow.AddMinutes(15);

            await _cache.SetAsync($"{PasswordResetTokenKey}{resetSession}", user.UserId, TimeSpan.FromMinutes(15));

            var emailMetadata = new EmailMetadata
            {
                Subject = "Password Reset Request",
                ToAddress = user.Email,
                TemplatePath = nameof(PasswordReset)
            };

            var resetUrl = $"{_config["CLIENT_URL"]}/auth/reset-password?token={resetSession}";

            var emailMsg = new PasswordResetMsg
            {
                ResetLink = resetUrl,
                ExpirationMinutes = 15
            };
            await Task.Run(() => SendEmailAsync(emailMetadata, emailMsg).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.Error(t.Exception, "Failed to send email in background task.");
                }
            }));
            return new ApiStandardResponse<ForgotPasswordResponse?>(StatusCodes.Status200OK,
                new ForgotPasswordResponse
                {
                    Message = "Password reset instructions have been sent to your email",
                    ExpiresAt = tokenExpiry.ToString("O")
                });
        }
        catch (ExternalException e)
        {
            _logger.Error(e, "The email service not responding");
            return new ApiStandardResponse<ForgotPasswordResponse?>(StatusCodes.Status503ServiceUnavailable,
                "External service not responding");
        }
    }

    public async Task<ApiStandardResponse<ResetPasswordResponse?>> ResetPasswordAsync(ResetPasswordRequest request,
        string session)
    {
        var userId = await _cache.GetAsync<long>($"{PasswordResetTokenKey}{session}");
        if (userId == 0)
            return new ApiStandardResponse<ResetPasswordResponse?>(StatusCodes.Status400BadRequest,
                "Invalid or expired reset token");

        var user = await _userRepo.GetByConditionAsync(u => u.UserId == userId);
        if (user is null)
            return new ApiStandardResponse<ResetPasswordResponse?>(StatusCodes.Status404NotFound,
                "User not found");

        user.PasswordHashed = _passwordHasher.HashPassword(request.Password!);
        await _userRepo.UpdateAsync(user);

        await _cache.RemoveAsync($"{PasswordResetTokenKey}{session}");

        return new ApiStandardResponse<ResetPasswordResponse?>(StatusCodes.Status200OK,
            new ResetPasswordResponse { Message = "Password has been reset successfully" });
    }

    public async Task<ApiStandardResponse<LogoutResponse?>> LogoutAsync(long userId)
    {
        await _cache.RemoveAsync($"{RedisRefreshKey}{userId}");

        return new ApiStandardResponse<LogoutResponse?>(StatusCodes.Status200OK,
            new LogoutResponse { Message = "Logged out successfully" });
    }


    public async Task<ApiStandardResponse<ConfirmationResponse?>> PasswordChangeAsync(long id,
        PasswordChangeRequest request)
    {
        var user = await _userRepo.GetByConditionAsync(u => u.UserId == id);

        if (user is null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                "The account does not exist");
        if (user.IsDeleted)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status400BadRequest,
                "The account is locked");
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword!, user.PasswordHashed!))
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status400BadRequest,
                "The current password does not match");
        user.PasswordHashed = _passwordHasher.HashPassword(request.NewPassword!);
        await _userRepo.UpdateAsync(user);
        return new ApiStandardResponse<ConfirmationResponse?>(
            StatusCodes.Status200OK,
            new ConfirmationResponse { Message = "password changed successfully" });
    }


    public async Task<ApiStandardResponse<ConfirmationResponse>> ChangeProfileImage(long id, IFormFile file)
    {
        var user = await _userRepo.GetByIdAsync(id);

        if (user is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The user does not exist");

        if (!string.IsNullOrEmpty(user.Profile))
            await _storageProvider.DeleteFileAsync(user.Profile);

        string newProfile = await _storageProvider.UploadFileAsync<User>(Guid.NewGuid(), file);
        user.Profile = newProfile;
        await _userRepo.UpdateAsync(user);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK,
            new ConfirmationResponse()
            {
                Message = "The image has been changed successfully"
            });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> LinkGoogleAccount(long userId, string idToken)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_config["CLIENT_ID"]]
                });
        }
        catch (InvalidJwtException)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status401Unauthorized,
                "Invalid Google token");
        }

        var existingAuth = await _provider.GetByConditionAsync(ap =>
            ap.AuthProviderId == payload.Subject &&
            ap.ProviderName == AuthProviderEnum.Google.ToString());

        if (existingAuth is { IsDeleted: false } && existingAuth.UserId != userId)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status409Conflict,
                "This Google account is already linked to another user");
        }

        var user = await _userRepo.GetByConditionAsync(u => u.UserId == userId,
            include => include.Include(u => u.AuthProviders));

        if (user == null)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status404NotFound,
                "User not found");
        }

        var existingUserAuth = user.AuthProviders.FirstOrDefault(a =>
            a.AuthProviderId == payload.Subject &&
            a.ProviderName == AuthProviderEnum.Google.ToString());
        if (existingUserAuth is { IsDeleted: false })
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status409Conflict,
                "This Google account is already linked to your account");
        }

        if (existingUserAuth is { IsDeleted: true })
        {
            existingUserAuth.IsDeleted = false;
            existingUserAuth.UpdatedAt = DateTime.UtcNow;
            await _provider.UpdateAsync(existingUserAuth);
        }
        else if (existingAuth is { IsDeleted: true })
        {
            // Case where this Google account was previously linked to someone else but deleted
            // We'll create a new record rather than reviving the old one
            await _provider.AddAsync(new AuthProvider
            {
                UserId = userId,
                ProviderName = AuthProviderEnum.Google.ToString(),
                AuthProviderId = payload.Subject,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }
        else
        {
            await _provider.AddAsync(new AuthProvider
            {
                UserId = userId,
                ProviderName = AuthProviderEnum.Google.ToString(),
                AuthProviderId = payload.Subject,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        return new ApiStandardResponse<ConfirmationResponse?>(
            StatusCodes.Status200OK,
            new ConfirmationResponse { Message = "Google account successfully linked" });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> UnlinkProvider(long userId, string providerId,
        string providerName)
    {
        var authProviders = await _provider.GetAllByConditionAsync(ap => ap.UserId == userId && !ap.IsDeleted);

        if (authProviders.Count < 1)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status404NotFound,
                "No authentication providers found for this user");
        }

        var authProvider = authProviders.FirstOrDefault(ap =>
            ap.AuthProviderId == providerId &&
            ap.ProviderName == providerName);

        if (authProvider == null)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status404NotFound,
                "Authentication provider not found");
        }

        var user = await _userRepo.GetByConditionAsync(u => u.UserId == userId);
        bool hasPassword = user != null && !string.IsNullOrEmpty(user.PasswordHashed);

        if (authProviders.Count <= 1 && !hasPassword)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status400BadRequest,
                "Cannot remove the only authentication method. Add another method first.");
        }

        authProvider.IsDeleted = true;
        authProvider.UpdatedAt = DateTime.UtcNow;
        await _provider.UpdateAsync(authProvider);

        return new ApiStandardResponse<ConfirmationResponse?>(
            StatusCodes.Status200OK,
            new ConfirmationResponse { Message = $"{providerName} account successfully unlinked" });
    }

    public async Task<ApiStandardResponse<List<AuthProviderResponse>?>> GetLinkedProvidersAsync(long userId)
    {
        var authProviders = await _dbContext.AuthProviders
            .Where(ap => ap.UserId == userId && !ap.IsDeleted)
            .Select(ap => new AuthProviderResponse
            {
                ProviderId = ap.AuthProviderId,
                ProviderName = ap.ProviderName,
                LinkedAt = ap.CreatedAt
            }).AsNoTracking()
            .ToListAsync();

        var user = await _userRepo.GetByConditionAsync(u => u.UserId == userId);
        if (user == null)
        {
            return new ApiStandardResponse<List<AuthProviderResponse>?>(
                StatusCodes.Status404NotFound,
                "User not found");
        }

        if (!string.IsNullOrEmpty(user.PasswordHashed))
        {
            authProviders.Add(new AuthProviderResponse
            {
                ProviderId = "password",
                ProviderName = "Password",
                LinkedAt = user.CreatedAt
            });
        }

        return new ApiStandardResponse<List<AuthProviderResponse>?>(
            StatusCodes.Status200OK,
            authProviders);
    }

    private async Task SendEmailAsync(EmailMetadata emailMetadata, PasswordResetMsg model)
    {
        var emailBody = await _razorPageRenderer.RenderTemplateAsync(emailMetadata.TemplatePath, model);
        var email = await _fluentEmail.To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .Body(emailBody, true)
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
            }

            throw new ExternalException("External service not responding");
        }
    }

    private async Task SendEmailAsync(EmailMetadata emailMetadata, EmailVerificationMsg model)
    {
        var emailBody = await _razorPageRenderer.RenderTemplateAsync(emailMetadata.TemplatePath, model);
        var email = await _fluentEmail.To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .Body(emailBody, true)
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
            }

            throw new ExternalException("External service not responding");
        }
    }
}