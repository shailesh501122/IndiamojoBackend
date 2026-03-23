using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Reviews;

public sealed class Review : BaseEntity
{
    public Guid PropertyId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;

    private Review() { }

    public Review(Guid propertyId, Guid userId, int rating, string comment)
    {
        PropertyId = propertyId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
    }
}
