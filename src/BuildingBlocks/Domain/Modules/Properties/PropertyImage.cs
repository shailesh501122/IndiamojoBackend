using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

public sealed class PropertyImage : BaseEntity
{
    public Guid PropertyId { get; private set; }
    public string Url { get; private set; } = string.Empty;

    private PropertyImage() { }

    public PropertyImage(Guid propertyId, string url)
    {
        PropertyId = propertyId;
        Url = url;
    }
}
