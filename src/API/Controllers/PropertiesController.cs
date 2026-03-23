using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Properties;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Owner,Agent,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePropertyRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new CreatePropertyCommand(request.OwnerId, request.Title, request.Description, request.Type, request.BHK, request.Price, request.City, request.Latitude, request.Longitude, request.IsFeatured, request.Amenities, request.Images), cancellationToken));

    [Authorize(Roles = "Owner,Agent,Admin")]
    [HttpPatch("{propertyId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid propertyId, UpdatePropertyStatusRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdatePropertyStatusCommand(propertyId, request.Status), cancellationToken);
        return NoContent();
    }

    [HttpGet("{propertyId:guid}")]
    public async Task<IActionResult> GetById(Guid propertyId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetPropertyByIdQuery(propertyId), cancellationToken));

    [Authorize]
    [HttpGet("owner/{ownerId:guid}")]
    public async Task<IActionResult> GetOwnerProperties(Guid ownerId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetOwnerPropertiesQuery(ownerId), cancellationToken));

    public sealed record CreatePropertyRequest(Guid OwnerId, string Title, string Description, PropertyType Type, int BHK, decimal Price, string City, decimal Latitude, decimal Longitude, bool IsFeatured, IReadOnlyCollection<string> Amenities, IReadOnlyCollection<string> Images);
    public sealed record UpdatePropertyStatusRequest(PropertyStatus Status);
}
