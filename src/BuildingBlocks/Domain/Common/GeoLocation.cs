namespace IndiamojoBackend.BuildingBlocks.Domain.Common;

public sealed class GeoLocation
{
    public string City { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    private GeoLocation() { }

    public GeoLocation(string city, decimal latitude, decimal longitude)
    {
        City = city;
        Latitude = latitude;
        Longitude = longitude;
    }
}
