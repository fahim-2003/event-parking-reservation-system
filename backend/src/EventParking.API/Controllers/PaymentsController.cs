using EventParking.API.DTOs.Payments;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
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
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.CreateAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}
