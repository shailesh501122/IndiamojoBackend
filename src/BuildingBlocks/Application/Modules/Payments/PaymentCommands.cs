using MediatR;
using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Payments;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Payments;

public sealed record CreatePaymentCommand(Guid UserId, decimal Amount, PaymentGateway Gateway) : IRequest<Guid>;
public sealed record PaymentWebhookCommand(string Reference, bool IsSuccess) : IRequest;

public sealed class CreatePaymentHandler(IApplicationDbContext context, IPaymentGatewayService paymentGatewayService) : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var reference = await paymentGatewayService.CreatePaymentIntentAsync(request.Amount, request.Gateway, cancellationToken);
        var payment = new Payment(request.UserId, request.Amount, request.Gateway, reference);
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }
}

public sealed class PaymentWebhookHandler(IApplicationDbContext context) : IRequestHandler<PaymentWebhookCommand>
{
    public async Task Handle(PaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var payment = await context.Payments.FirstOrDefaultAsync(x => x.Reference == request.Reference, cancellationToken)
            ?? throw new KeyNotFoundException("Payment reference not found.");
        if (request.IsSuccess) payment.MarkPaid();
        await context.SaveChangesAsync(cancellationToken);
    }
}
