using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

public sealed class PropertyAmenity : BaseEntity
{
    public Guid PropertyId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private PropertyAmenity() { }

    public PropertyAmenity(Guid propertyId, string name)
    {
        PropertyId = propertyId;
        Name = name;
    }
}
