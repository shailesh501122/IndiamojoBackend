using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Payments;

public sealed class Payment : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentGateway Gateway { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string Reference { get; private set; } = string.Empty;

    private Payment() { }

    public Payment(Guid userId, decimal amount, PaymentGateway gateway, string reference)
    {
        UserId = userId;
        Amount = amount;
        Gateway = gateway;
        Reference = reference;
    }

    public void MarkPaid() => Status = PaymentStatus.Paid;
}
