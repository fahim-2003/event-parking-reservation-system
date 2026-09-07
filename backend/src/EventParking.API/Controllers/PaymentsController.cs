using System.Security.Claims;
using EventParking.API.DTOs.Payments;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Customer")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var customerId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _paymentService.ProcessAsync(
                request,
                customerId,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid payment.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Payment conflict.",
                detail: exception.Message);
        }
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> Mine(
        CancellationToken cancellationToken)
    {
        var customerId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        var payments =
            await _paymentService.GetForCustomerAsync(
                customerId,
                cancellationToken);

        return Ok(payments);
    }
}
