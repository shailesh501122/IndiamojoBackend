using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Bookings;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Bookings;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Tenant,Admin")]
    [HttpPost]
    public async Task<IActionResult> Schedule(ScheduleVisitRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new ScheduleVisitCommand(request.PropertyId, request.TenantId, request.VisitDateUtc), cancellationToken));

    [Authorize(Roles = "Owner,Agent,Admin")]
    [HttpPatch("{bookingId:guid}")]
    public async Task<IActionResult> Review(Guid bookingId, ReviewBookingRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ReviewBookingCommand(bookingId, request.Status), cancellationToken);
        return NoContent();
    }

    public sealed record ScheduleVisitRequest(Guid PropertyId, Guid TenantId, DateTime VisitDateUtc);
    public sealed record ReviewBookingRequest(BookingStatus Status);
}
