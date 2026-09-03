using System.Reflection;
using EventParking.API.Controllers;
using EventParking.API.DTOs.Auth;
using EventParking.API.Interfaces.Services;
using EventParking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventParking.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsCreated_WhenRegistrationSucceeds()
    {
        var authService = new Mock<IAuthService>();

        var request = new RegisterRequest
        {
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            PhoneNumber = "0771234567",
            Password = "Customer9!",
            ConfirmPassword = "Customer9!",
            AcceptedTerms = true
        };

        var response = new RegisterResponse
        {
            UserId = "customer-1",
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            Role = "Customer",
            EmailVerificationRequired = true
        };

        authService
            .Setup(service =>
                service.RegisterCustomerAsync(request))
            .ReturnsAsync(
                AuthOperationResult<RegisterResponse>
                    .Success(response));

        var controller = CreateController(
            authService);

        var result = await controller.Register(request);

        var objectResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status201Created,
            objectResult.StatusCode);

        Assert.Same(
            response,
            objectResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsConflict_ForDuplicateEmail()
    {
        var authService = new Mock<IAuthService>();

        var request = new RegisterRequest
        {
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            PhoneNumber = "0771234567",
            Password = "Customer9!",
            ConfirmPassword = "Customer9!",
            AcceptedTerms = true
        };

        authService
            .Setup(service =>
                service.RegisterCustomerAsync(request))
            .ReturnsAsync(
                AuthOperationResult<RegisterResponse>
                    .Failure(
                        "EMAIL_ALREADY_REGISTERED",
                        "An account with this email address already exists."));

        var controller = CreateController(
            authService);

        var result = await controller.Register(request);

        var objectResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            objectResult.StatusCode);

        var problem =
            Assert.IsType<ProblemDetails>(
                objectResult.Value);

        Assert.Equal(
            "EMAIL_ALREADY_REGISTERED",
            problem.Extensions["errorCode"]);
    }

    [Fact]
    public async Task Register_ReturnsValidationProblem_WhenValidationErrorsExist()
    {
        var authService = new Mock<IAuthService>();

        var request = new RegisterRequest
        {
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            PhoneNumber = "0771234567",
            Password = "weak",
            ConfirmPassword = "weak",
            AcceptedTerms = true
        };

        var errors =
            new Dictionary<string, string[]>
            {
                ["password"] =
                    new[]
                    {
                        "Password does not meet requirements."
                    }
            };

        authService
            .Setup(service =>
                service.RegisterCustomerAsync(request))
            .ReturnsAsync(
                AuthOperationResult<RegisterResponse>
                    .Failure(
                        "REGISTRATION_VALIDATION_FAILED",
                        "Registration validation failed.",
                        errors));

        var controller = CreateController(
            authService);

        var result = await controller.Register(request);

        var badRequest =
            Assert.IsType<BadRequestObjectResult>(
                result);

        var problem =
            Assert.IsType<ValidationProblemDetails>(
                badRequest.Value);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            problem.Status);

        Assert.Equal(
            "REGISTRATION_VALIDATION_FAILED",
            problem.Extensions["errorCode"]);

        Assert.True(
            problem.Errors.ContainsKey("password"));
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenAuthenticationSucceeds()
    {
        var authService = new Mock<IAuthService>();

        var request = new LoginRequest
        {
            Email = "customer1@eventparking.local",
            Password = "Customer9!"
        };

        var response = new AuthResponse
        {
            AccessToken = "test-token",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            UserId = "customer-1",
            FullName = "Test Customer",
            Email = "customer1@eventparking.local",
            Role = "Customer",
            AccountStatus = "Active"
        };

        authService
            .Setup(service =>
                service.LoginAsync(request))
            .ReturnsAsync(
                AuthOperationResult<AuthResponse>
                    .Success(response));

        var controller = CreateController(
            authService);

        var result = await controller.Login(request);

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        Assert.Same(
            response,
            okResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_ForInvalidCredentials()
    {
        var authService = new Mock<IAuthService>();

        var request = new LoginRequest
        {
            Email = "customer1@eventparking.local",
            Password = "WrongPassword!"
        };

        authService
            .Setup(service =>
                service.LoginAsync(request))
            .ReturnsAsync(
                AuthOperationResult<AuthResponse>
                    .Failure(
                        "INVALID_CREDENTIALS",
                        "Invalid email or password."));

        var controller = CreateController(
            authService);

        var result = await controller.Login(request);

        var objectResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            objectResult.StatusCode);

        var problem =
            Assert.IsType<ProblemDetails>(
                objectResult.Value);

        Assert.Equal(
            "INVALID_CREDENTIALS",
            problem.Extensions["errorCode"]);
    }

    [Fact]
    public async Task Login_ReturnsForbidden_WhenEmailNotVerified()
    {
        var authService = new Mock<IAuthService>();

        var request = new LoginRequest
        {
            Email = "customer1@eventparking.local",
            Password = "Customer9!"
        };

        authService
            .Setup(service =>
                service.LoginAsync(request))
            .ReturnsAsync(
                AuthOperationResult<AuthResponse>
                    .Failure(
                        "EMAIL_NOT_VERIFIED",
                        "Verify your email address before signing in."));

        var controller = CreateController(
            authService);

        var result = await controller.Login(request);

        var objectResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status403Forbidden,
            objectResult.StatusCode);

        var problem =
            Assert.IsType<ProblemDetails>(
                objectResult.Value);

        Assert.Equal(
            "EMAIL_NOT_VERIFIED",
            problem.Extensions["errorCode"]);
    }

    [Fact]
    public async Task VerifyEmail_ReturnsBadRequest_WhenVerificationFails()
    {
        var authService = new Mock<IAuthService>();

        authService
            .Setup(service =>
                service.VerifyEmailAsync(
                    "customer-1",
                    "bad-token"))
            .ReturnsAsync(
                AuthOperationResult<MessageResponse>
                    .Failure(
                        "INVALID_VERIFICATION_TOKEN",
                        "The email verification link is invalid or expired."));

        var controller = CreateController(
            authService);

        var result =
            await controller.VerifyEmail(
                "customer-1",
                "bad-token");

        var objectResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            objectResult.StatusCode);

        var problem =
            Assert.IsType<ProblemDetails>(
                objectResult.Value);

        Assert.Equal(
            "INVALID_VERIFICATION_TOKEN",
            problem.Extensions["errorCode"]);
    }

    [Fact]
    public async Task ResendVerification_ReturnsGenericOkResponse()
    {
        var authService = new Mock<IAuthService>();

        var request =
            new ResendVerificationRequest
            {
                Email = "missing@eventparking.local"
            };

        var message =
            new MessageResponse
            {
                Message =
                    "If the account exists and requires verification, " +
                    "a verification link has been generated."
            };

        authService
            .Setup(service =>
                service.ResendVerificationAsync(request))
            .ReturnsAsync(
                AuthOperationResult<MessageResponse>
                    .Success(message));

        var controller = CreateController(
            authService);

        var result =
            await controller.ResendVerification(request);

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        Assert.Same(
            message,
            okResult.Value);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsGenericOkResponse()
    {
        var authService = new Mock<IAuthService>();

        var request =
            new ForgotPasswordRequest
            {
                Email = "missing@eventparking.local"
            };

        var message =
            new MessageResponse
            {
                Message =
                    "If an eligible account exists, " +
                    "a password reset link has been generated."
            };

        authService
            .Setup(service =>
                service.ForgotPasswordAsync(request))
            .ReturnsAsync(
                AuthOperationResult<MessageResponse>
                    .Success(message));

        var controller = CreateController(
            authService);

        var result =
            await controller.ForgotPassword(request);

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        Assert.Same(
            message,
            okResult.Value);
    }

    [Fact]
    public async Task ResetPassword_ReturnsValidationProblem_WhenIdentityValidationFails()
    {
        var authService = new Mock<IAuthService>();

        var request =
            new ResetPasswordRequest
            {
                UserId = "customer-1",
                Token = "reset-token",
                NewPassword = "weakpass"
            };

        var errors =
            new Dictionary<string, string[]>
            {
                ["password"] =
                    new[]
                    {
                        "Password must contain an uppercase letter."
                    }
            };

        authService
            .Setup(service =>
                service.ResetPasswordAsync(request))
            .ReturnsAsync(
                AuthOperationResult<MessageResponse>
                    .Failure(
                        "PASSWORD_RESET_FAILED",
                        "The password could not be reset.",
                        errors));

        var controller = CreateController(
            authService);

        var result =
            await controller.ResetPassword(request);

        var badRequest =
            Assert.IsType<BadRequestObjectResult>(
                result);

        var problem =
            Assert.IsType<ValidationProblemDetails>(
                badRequest.Value);

        Assert.Equal(
            "PASSWORD_RESET_FAILED",
            problem.Extensions["errorCode"]);

        Assert.True(
            problem.Errors.ContainsKey("password"));
    }

    [Fact]
    public void AuthController_IsAllowAnonymous()
    {
        var attribute =
            typeof(AuthController)
                .GetCustomAttribute<
                    AllowAnonymousAttribute>();

        Assert.NotNull(attribute);
    }

    private static AuthController CreateController(
        Mock<IAuthService> authService)
    {
        var httpContext =
            new DefaultHttpContext();

        httpContext.Request.Path =
            "/api/auth/test";

        httpContext.TraceIdentifier =
            "test-trace-id";

        return new AuthController(
            authService.Object)
        {
            ControllerContext =
                new ControllerContext
                {
                    HttpContext = httpContext
                }
        };
    }
}
