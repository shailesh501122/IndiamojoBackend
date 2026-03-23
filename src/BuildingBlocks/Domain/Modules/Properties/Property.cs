using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

public sealed class Property : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PropertyType Type { get; private set; }
    public int BHK { get; private set; }
    public decimal Price { get; private set; }
    public GeoLocation Location { get; private set; } = default!;
    public PropertyStatus Status { get; private set; } = PropertyStatus.Draft;
    public bool IsFeatured { get; private set; }
    public List<PropertyAmenity> Amenities { get; private set; } = [];
    public List<PropertyImage> Images { get; private set; } = [];

    private Property() { }

    public Property(Guid ownerId, string title, string description, PropertyType type, int bhk, decimal price, GeoLocation location, bool isFeatured)
    {
        OwnerId = ownerId;
        Title = title;
        Description = description;
        Type = type;
        BHK = bhk;
        Price = price;
        Location = location;
        IsFeatured = isFeatured;
    }

    public void SetStatus(PropertyStatus status) => Status = status;
    public void AddAmenity(string name) => Amenities.Add(new PropertyAmenity(Id, name));
    public void AddImage(string url) => Images.Add(new PropertyImage(Id, url));
}
