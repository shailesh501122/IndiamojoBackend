using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Payments;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Payments;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken cancellationToken)
        => Ok(new { paymentId = await sender.Send(new CreatePaymentCommand(request.UserId, request.Amount, request.Gateway), cancellationToken) });

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new PaymentWebhookCommand(request.Reference, request.IsSuccess), cancellationToken);
        return Accepted();
    }

    public sealed record CreatePaymentRequest(Guid UserId, decimal Amount, PaymentGateway Gateway);
    public sealed record PaymentWebhookRequest(string Reference, bool IsSuccess);
}
