using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Listings;

public sealed record SearchListingsQuery(string? City, decimal? MinPrice, decimal? MaxPrice, int? BHK, PropertyType? Type, int Page = 1, int PageSize = 10) : IRequest<PagedResult<PropertyResponse>>;

public sealed class SearchListingsHandler(IApplicationDbContext context, IMapper mapper, ICacheService cacheService)
    : IRequestHandler<SearchListingsQuery, PagedResult<PropertyResponse>>
{
    public async Task<PagedResult<PropertyResponse>> Handle(SearchListingsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"listings:{request.City}:{request.MinPrice}:{request.MaxPrice}:{request.BHK}:{request.Type}:{request.Page}:{request.PageSize}";
        var cached = await cacheService.GetAsync<PagedResult<PropertyResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var query = context.Properties
            .Include(x => x.Amenities)
            .Include(x => x.Images)
            .Where(x => x.Status == PropertyStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.City)) query = query.Where(x => x.Location.City == request.City);
        if (request.MinPrice.HasValue) query = query.Where(x => x.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(x => x.Price <= request.MaxPrice.Value);
        if (request.BHK.HasValue) query = query.Where(x => x.BHK == request.BHK.Value);
        if (request.Type.HasValue) query = query.Where(x => x.Type == request.Type.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<PropertyResponse>(items.Select(x => mapper.Map<PropertyResponse>(x)).ToArray(), request.Page, request.PageSize, totalCount);
        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
        return result;
    }
}
