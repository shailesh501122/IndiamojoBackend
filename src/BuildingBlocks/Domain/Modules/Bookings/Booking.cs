using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Bookings;

public sealed class Booking : BaseEntity
{
    public Guid PropertyId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime VisitDateUtc { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;

    private Booking() { }

    public Booking(Guid propertyId, Guid tenantId, DateTime visitDateUtc)
    {
        PropertyId = propertyId;
        TenantId = tenantId;
        VisitDateUtc = visitDateUtc;
    }

    public void Review(BookingStatus status) => Status = status;
}
