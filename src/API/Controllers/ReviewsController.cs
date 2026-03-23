using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Reviews;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(AddReviewRequest request, CancellationToken cancellationToken)
        => Ok(new { reviewId = await sender.Send(new AddReviewCommand(request.PropertyId, request.UserId, request.Rating, request.Comment), cancellationToken) });

    public sealed record AddReviewRequest(Guid PropertyId, Guid UserId, int Rating, string Comment);
}
