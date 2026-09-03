using EventParking.API.DTOs.Auth;
using EventParking.API.Enums;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace EventParking.API.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailSender = emailSender;
    }

    public async Task<AuthOperationResult<RegisterResponse>>
        RegisterCustomerAsync(RegisterRequest request)
    {
        if (!request.AcceptedTerms)
        {
            return AuthOperationResult<RegisterResponse>.Failure(
                "TERMS_NOT_ACCEPTED",
                "You must accept the terms and conditions.");
        }

        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();
        var phoneNumber = request.PhoneNumber.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return AuthOperationResult<RegisterResponse>.Failure(
                "EMAIL_ALREADY_REGISTERED",
                "An account with this email address already exists.");
        }

        var now = DateTime.UtcNow;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            AccountStatus = AccountStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            return AuthOperationResult<RegisterResponse>.Failure(
                "REGISTRATION_VALIDATION_FAILED",
                "Registration could not be completed.",
                BuildIdentityErrors(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            AppRoles.Customer);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return AuthOperationResult<RegisterResponse>.Failure(
                "ROLE_ASSIGNMENT_FAILED",
                "Registration could not be completed.");
        }

        var verificationToken =
            await _userManager.GenerateEmailConfirmationTokenAsync(user);

        await _emailSender.SendEmailVerificationAsync(
            user,
            verificationToken);

        return AuthOperationResult<RegisterResponse>.Success(
            new RegisterResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = AppRoles.Customer,
                EmailVerificationRequired = true
            });
    }

    public async Task<AuthOperationResult<AuthResponse>>
        LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return InvalidCredentials();
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordValid)
        {
            return InvalidCredentials();
        }

        // Do not expose account deactivation through login responses.
        if (user.AccountStatus != AccountStatus.Active)
        {
            return InvalidCredentials();
        }

        if (!user.EmailConfirmed)
        {
            return AuthOperationResult<AuthResponse>.Failure(
                "EMAIL_NOT_VERIFIED",
                "Verify your email address before signing in.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count != 1)
        {
            return AuthOperationResult<AuthResponse>.Failure(
                "ACCOUNT_ROLE_INVALID",
                "The account role configuration is invalid.");
        }

        var tokenResult =
            await _tokenService.CreateAccessTokenAsync(user);

        return AuthOperationResult<AuthResponse>.Success(
            new AuthResponse
            {
                AccessToken = tokenResult.Token,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles[0],
                AccountStatus = user.AccountStatus.ToString()
            });
    }

    public async Task<AuthOperationResult<MessageResponse>>
        VerifyEmailAsync(
            string userId,
            string token)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "INVALID_VERIFICATION_REQUEST",
                "The email verification link is invalid.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "INVALID_VERIFICATION_REQUEST",
                "The email verification link is invalid.");
        }

        if (user.EmailConfirmed)
        {
            return AuthOperationResult<MessageResponse>.Success(
                new MessageResponse
                {
                    Message = "Email address is already verified."
                });
        }

        user.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _userManager.ConfirmEmailAsync(
            user,
            token);

        if (!result.Succeeded)
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "INVALID_VERIFICATION_TOKEN",
                "The email verification link is invalid or expired.");
        }

        return AuthOperationResult<MessageResponse>.Success(
            new MessageResponse
            {
                Message = "Email address verified successfully."
            });
    }

    public async Task<AuthOperationResult<MessageResponse>>
        ResendVerificationAsync(
            ResendVerificationRequest request)
    {
        var genericResponse =
            AuthOperationResult<MessageResponse>.Success(
                new MessageResponse
                {
                    Message =
                        "If the account exists and requires verification, " +
                        "a verification link has been generated."
                });

        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null ||
            user.EmailConfirmed ||
            user.AccountStatus != AccountStatus.Active)
        {
            return genericResponse;
        }

        var token =
            await _userManager.GenerateEmailConfirmationTokenAsync(user);

        await _emailSender.SendEmailVerificationAsync(
            user,
            token);

        return genericResponse;
    }

    public async Task<AuthOperationResult<MessageResponse>>
        ForgotPasswordAsync(
            ForgotPasswordRequest request)
    {
        var genericResponse =
            AuthOperationResult<MessageResponse>.Success(
                new MessageResponse
                {
                    Message =
                        "If an eligible account exists, " +
                        "a password reset link has been generated."
                });

        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null ||
            !user.EmailConfirmed ||
            user.AccountStatus != AccountStatus.Active)
        {
            return genericResponse;
        }

        var token =
            await _userManager.GeneratePasswordResetTokenAsync(user);

        await _emailSender.SendPasswordResetAsync(
            user,
            token);

        return genericResponse;
    }

    public async Task<AuthOperationResult<MessageResponse>>
        ResetPasswordAsync(
            ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) ||
            string.IsNullOrWhiteSpace(request.Token))
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "INVALID_PASSWORD_RESET",
                "The password reset request is invalid or expired.");
        }

        var user =
            await _userManager.FindByIdAsync(request.UserId);

        if (user is null ||
            user.AccountStatus != AccountStatus.Active)
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "INVALID_PASSWORD_RESET",
                "The password reset request is invalid or expired.");
        }

        var resetResult =
            await _userManager.ResetPasswordAsync(
                user,
                request.Token,
                request.NewPassword);

        if (!resetResult.Succeeded)
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "PASSWORD_RESET_FAILED",
                "The password could not be reset.",
                BuildIdentityErrors(resetResult));
        }

        user.UpdatedAtUtc = DateTime.UtcNow;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return AuthOperationResult<MessageResponse>.Failure(
                "ACCOUNT_UPDATE_FAILED",
                "The password was reset but account metadata " +
                "could not be updated.");
        }

        return AuthOperationResult<MessageResponse>.Success(
            new MessageResponse
            {
                Message = "Password reset successfully."
            });
    }

    private static AuthOperationResult<AuthResponse>
        InvalidCredentials()
    {
        return AuthOperationResult<AuthResponse>.Failure(
            "INVALID_CREDENTIALS",
            "Invalid email or password.");
    }

    private static IReadOnlyDictionary<string, string[]>
        BuildIdentityErrors(IdentityResult result)
    {
        return result.Errors
            .GroupBy(GetErrorField)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.Description)
                    .ToArray());
    }

    private static string GetErrorField(
        IdentityError error)
    {
        if (error.Code.Contains(
                "Password",
                StringComparison.OrdinalIgnoreCase))
        {
            return "password";
        }

        if (error.Code.Contains(
                "Email",
                StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains(
                "UserName",
                StringComparison.OrdinalIgnoreCase))
        {
            return "email";
        }

        return "identity";
    }
}