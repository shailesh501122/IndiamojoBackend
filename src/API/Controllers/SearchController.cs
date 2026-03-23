using MediatR;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Listings;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? city, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] int? bhk, [FromQuery] PropertyType? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new SearchListingsQuery(city, minPrice, maxPrice, bhk, type, page, pageSize), cancellationToken));
}
