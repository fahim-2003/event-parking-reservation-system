using EventParking.API.DTOs.Auth;
using EventParking.API.Interfaces.Services;
using EventParking.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var result =
            await _authService.RegisterCustomerAsync(request);

        if (result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                result.Value);
        }

        if (result.ErrorCode == "EMAIL_ALREADY_REGISTERED")
        {
            return CreateProblem(
                StatusCodes.Status409Conflict,
                result.ErrorCode,
                result.ErrorMessage ?? "The email address is already registered.");
        }

        if (result.ValidationErrors is not null)
        {
            return CreateValidationProblem(
                result.ErrorCode ?? "REGISTRATION_VALIDATION_FAILED",
                result.ErrorMessage ?? "Registration validation failed.",
                result.ValidationErrors);
        }

        if (result.ErrorCode == "TERMS_NOT_ACCEPTED")
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                result.ErrorCode,
                result.ErrorMessage ?? "Terms and conditions must be accepted.");
        }

        return CreateProblem(
            StatusCodes.Status500InternalServerError,
            result.ErrorCode ?? "REGISTRATION_FAILED",
            result.ErrorMessage ?? "Registration could not be completed.");
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var result =
            await _authService.LoginAsync(request);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        if (result.ErrorCode == "INVALID_CREDENTIALS")
        {
            return CreateProblem(
                StatusCodes.Status401Unauthorized,
                result.ErrorCode,
                result.ErrorMessage ?? "Invalid email or password.");
        }

        if (result.ErrorCode == "EMAIL_NOT_VERIFIED")
        {
            return CreateProblem(
                StatusCodes.Status403Forbidden,
                result.ErrorCode,
                result.ErrorMessage ?? "Email verification is required.");
        }

        if (result.ErrorCode == "ACCOUNT_ROLE_INVALID")
        {
            return CreateProblem(
                StatusCodes.Status403Forbidden,
                result.ErrorCode,
                "The account cannot sign in.");
        }

        return CreateProblem(
            StatusCodes.Status401Unauthorized,
            "INVALID_CREDENTIALS",
            "Invalid email or password.");
    }

    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string userId,
        [FromQuery] string token)
    {
        var result =
            await _authService.VerifyEmailAsync(
                userId,
                token);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return CreateProblem(
            StatusCodes.Status400BadRequest,
            result.ErrorCode ?? "EMAIL_VERIFICATION_FAILED",
            result.ErrorMessage ?? "Email verification failed.");
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request)
    {
        var result =
            await _authService.ResendVerificationAsync(request);

        return Ok(result.Value);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        var result =
            await _authService.ForgotPasswordAsync(request);

        return Ok(result.Value);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        var result =
            await _authService.ResetPasswordAsync(request);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        if (result.ValidationErrors is not null)
        {
            return CreateValidationProblem(
                result.ErrorCode ?? "PASSWORD_RESET_FAILED",
                result.ErrorMessage ?? "Password reset failed.",
                result.ValidationErrors);
        }

        return CreateProblem(
            StatusCodes.Status400BadRequest,
            result.ErrorCode ?? "PASSWORD_RESET_FAILED",
            result.ErrorMessage ?? "Password reset failed.");
    }

    private ObjectResult CreateProblem(
        int statusCode,
        string errorCode,
        string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetProblemTitle(statusCode),
            Detail = detail,
            Instance = HttpContext.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return StatusCode(statusCode, problem);
    }

    private BadRequestObjectResult CreateValidationProblem(
        string errorCode,
        string detail,
        IReadOnlyDictionary<string, string[]> errors)
    {
        var validationProblem =
            new ValidationProblemDetails(
                errors.ToDictionary(
                    item => item.Key,
                    item => item.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed.",
                Detail = detail,
                Instance = HttpContext.Request.Path
            };

        validationProblem.Extensions["errorCode"] = errorCode;
        validationProblem.Extensions["traceId"] =
            HttpContext.TraceIdentifier;

        return BadRequest(validationProblem);
    }

    private static string GetProblemTitle(
        int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest =>
                "Bad request.",

            StatusCodes.Status401Unauthorized =>
                "Authentication failed.",

            StatusCodes.Status403Forbidden =>
                "Access denied.",

            StatusCodes.Status409Conflict =>
                "Conflict.",

            _ =>
                "An error occurred."
        };
    }
}