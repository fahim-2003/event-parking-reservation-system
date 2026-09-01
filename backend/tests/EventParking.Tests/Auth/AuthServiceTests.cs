using EventParking.API.DTOs.Auth;
using EventParking.API.Enums;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using EventParking.API.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace EventParking.Tests.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_Succeeds_ForVerifiedActiveCustomer()
    {
        var user = CreateUser(
            emailConfirmed: true,
            accountStatus: AccountStatus.Active);

        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var emailSender = new Mock<IEmailSender>();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.CheckPasswordAsync(
                    user,
                    "Customer9!"))
            .ReturnsAsync(true);

        userManager
            .Setup(manager =>
                manager.GetRolesAsync(user))
            .ReturnsAsync(
                new List<string>
                {
                    AppRoles.Customer
                });

        var expiresAtUtc =
            new DateTime(
                2026,
                9,
                1,
                20,
                0,
                0,
                DateTimeKind.Utc);

        tokenService
            .Setup(service =>
                service.CreateAccessTokenAsync(user))
            .ReturnsAsync(
                new AccessTokenResult
                {
                    Token = "test-access-token",
                    ExpiresAtUtc = expiresAtUtc
                });

        var service = new AuthService(
            userManager.Object,
            tokenService.Object,
            emailSender.Object);

        var result = await service.LoginAsync(
            new LoginRequest
            {
                Email = user.Email!,
                Password = "Customer9!"
            });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "test-access-token",
            result.Value.AccessToken);

        Assert.Equal(
            expiresAtUtc,
            result.Value.ExpiresAtUtc);

        Assert.Equal(
            user.Id,
            result.Value.UserId);

        Assert.Equal(
            AppRoles.Customer,
            result.Value.Role);

        Assert.Equal(
            AccountStatus.Active.ToString(),
            result.Value.AccountStatus);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_ForWrongPassword()
    {
        var user = CreateUser(
            emailConfirmed: true,
            accountStatus: AccountStatus.Active);

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.CheckPasswordAsync(
                    user,
                    "WrongPassword!"))
            .ReturnsAsync(false);

        var service = CreateService(
            userManager);

        var result = await service.LoginAsync(
            new LoginRequest
            {
                Email = user.Email!,
                Password = "WrongPassword!"
            });

        Assert.False(result.Succeeded);
        Assert.Equal(
            "INVALID_CREDENTIALS",
            result.ErrorCode);

        Assert.Equal(
            "Invalid email or password.",
            result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_ReturnsGenericInvalidCredentials_ForDeactivatedAccount()
    {
        var user = CreateUser(
            emailConfirmed: true,
            accountStatus: AccountStatus.Deactivated);

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.CheckPasswordAsync(
                    user,
                    "Customer9!"))
            .ReturnsAsync(true);

        var service = CreateService(
            userManager);

        var result = await service.LoginAsync(
            new LoginRequest
            {
                Email = user.Email!,
                Password = "Customer9!"
            });

        Assert.False(result.Succeeded);

        Assert.Equal(
            "INVALID_CREDENTIALS",
            result.ErrorCode);

        Assert.Equal(
            "Invalid email or password.",
            result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_ReturnsEmailNotVerified_ForUnverifiedAccount()
    {
        var user = CreateUser(
            emailConfirmed: false,
            accountStatus: AccountStatus.Active);

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.CheckPasswordAsync(
                    user,
                    "Customer9!"))
            .ReturnsAsync(true);

        var service = CreateService(
            userManager);

        var result = await service.LoginAsync(
            new LoginRequest
            {
                Email = user.Email!,
                Password = "Customer9!"
            });

        Assert.False(result.Succeeded);

        Assert.Equal(
            "EMAIL_NOT_VERIFIED",
            result.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAccountRoleInvalid_WhenUserHasMultipleRoles()
    {
        var user = CreateUser(
            emailConfirmed: true,
            accountStatus: AccountStatus.Active);

        var userManager = CreateUserManagerMock();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.CheckPasswordAsync(
                    user,
                    "Customer9!"))
            .ReturnsAsync(true);

        userManager
            .Setup(manager =>
                manager.GetRolesAsync(user))
            .ReturnsAsync(
                new List<string>
                {
                    AppRoles.Customer,
                    AppRoles.Administrator
                });

        var service = CreateService(
            userManager);

        var result = await service.LoginAsync(
            new LoginRequest
            {
                Email = user.Email!,
                Password = "Customer9!"
            });

        Assert.False(result.Succeeded);

        Assert.Equal(
            "ACCOUNT_ROLE_INVALID",
            result.ErrorCode);
    }

    [Fact]
    public async Task ForgotPasswordAsync_DoesNotRevealUnknownAccount()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var emailSender = new Mock<IEmailSender>();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(
                    "missing@eventparking.local"))
            .ReturnsAsync(
                (ApplicationUser?)null);

        var service = new AuthService(
            userManager.Object,
            tokenService.Object,
            emailSender.Object);

        var result =
            await service.ForgotPasswordAsync(
                new ForgotPasswordRequest
                {
                    Email =
                        "missing@eventparking.local"
                });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "If an eligible account exists, " +
            "a password reset link has been generated.",
            result.Value.Message);

        emailSender.Verify(
            sender =>
                sender.SendPasswordResetAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_SendsReset_ForEligibleAccount()
    {
        var user = CreateUser(
            emailConfirmed: true,
            accountStatus: AccountStatus.Active);

        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var emailSender = new Mock<IEmailSender>();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.GeneratePasswordResetTokenAsync(
                    user))
            .ReturnsAsync("reset-token");

        emailSender
            .Setup(sender =>
                sender.SendPasswordResetAsync(
                    user,
                    "reset-token"))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            userManager.Object,
            tokenService.Object,
            emailSender.Object);

        var result =
            await service.ForgotPasswordAsync(
                new ForgotPasswordRequest
                {
                    Email = user.Email!
                });

        Assert.True(result.Succeeded);

        emailSender.Verify(
            sender =>
                sender.SendPasswordResetAsync(
                    user,
                    "reset-token"),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_DoesNotRevealUnknownAccount()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var emailSender = new Mock<IEmailSender>();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(
                    "missing@eventparking.local"))
            .ReturnsAsync(
                (ApplicationUser?)null);

        var service = new AuthService(
            userManager.Object,
            tokenService.Object,
            emailSender.Object);

        var result =
            await service.ResendVerificationAsync(
                new ResendVerificationRequest
                {
                    Email =
                        "missing@eventparking.local"
                });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "If the account exists and requires verification, " +
            "a verification link has been generated.",
            result.Value.Message);

        emailSender.Verify(
            sender =>
                sender.SendEmailVerificationAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerificationAsync_SendsVerification_ForEligibleAccount()
    {
        var user = CreateUser(
            emailConfirmed: false,
            accountStatus: AccountStatus.Active);

        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var emailSender = new Mock<IEmailSender>();

        userManager
            .Setup(manager =>
                manager.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(manager =>
                manager.GenerateEmailConfirmationTokenAsync(
                    user))
            .ReturnsAsync("verification-token");

        emailSender
            .Setup(sender =>
                sender.SendEmailVerificationAsync(
                    user,
                    "verification-token"))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            userManager.Object,
            tokenService.Object,
            emailSender.Object);

        var result =
            await service.ResendVerificationAsync(
                new ResendVerificationRequest
                {
                    Email = user.Email!
                });

        Assert.True(result.Succeeded);

        emailSender.Verify(
            sender =>
                sender.SendEmailVerificationAsync(
                    user,
                    "verification-token"),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsInvalidRequest_WhenRequiredValuesMissing()
    {
        var service = CreateService(
            CreateUserManagerMock());

        var result =
            await service.ResetPasswordAsync(
                new ResetPasswordRequest
                {
                    UserId = "",
                    Token = "",
                    NewPassword = "NewPassword9!"
                });

        Assert.False(result.Succeeded);

        Assert.Equal(
            "INVALID_PASSWORD_RESET",
            result.ErrorCode);
    }

    [Fact]
    public async Task VerifyEmailAsync_ReturnsInvalidRequest_WhenParametersMissing()
    {
        var service = CreateService(
            CreateUserManagerMock());

        var result =
            await service.VerifyEmailAsync(
                "",
                "");

        Assert.False(result.Succeeded);

        Assert.Equal(
            "INVALID_VERIFICATION_REQUEST",
            result.ErrorCode);
    }

    private static ApplicationUser CreateUser(
        bool emailConfirmed,
        AccountStatus accountStatus)
    {
        return new ApplicationUser
        {
            Id = "customer-1",
            UserName =
                "customer1@eventparking.local",
            Email =
                "customer1@eventparking.local",
            FullName = "Test Customer",
            PhoneNumber = "0771234567",
            EmailConfirmed = emailConfirmed,
            AccountStatus = accountStatus,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static AuthService CreateService(
        Mock<UserManager<ApplicationUser>> userManager)
    {
        return new AuthService(
            userManager.Object,
            new Mock<ITokenService>().Object,
            new Mock<IEmailSender>().Object);
    }

    private static Mock<UserManager<ApplicationUser>>
        CreateUserManagerMock()
    {
        var store =
            new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            null!,
            new IdentityErrorDescriber(),
            null!,
            null!);
    }
}
