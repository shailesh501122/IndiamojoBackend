using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Properties;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Properties;

public sealed record CreatePropertyCommand(Guid OwnerId, string Title, string Description, PropertyType Type, int BHK, decimal Price, string City, decimal Latitude, decimal Longitude, bool IsFeatured, IReadOnlyCollection<string> Amenities, IReadOnlyCollection<string> Images) : IRequest<PropertyResponse>;
public sealed record UpdatePropertyStatusCommand(Guid PropertyId, PropertyStatus Status) : IRequest;
public sealed record GetPropertyByIdQuery(Guid PropertyId) : IRequest<PropertyResponse>;
public sealed record GetOwnerPropertiesQuery(Guid OwnerId) : IRequest<IReadOnlyCollection<PropertyResponse>>;

public sealed class CreatePropertyValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.BHK).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreatePropertyHandler(IApplicationDbContext context, IMapper mapper, ISubscriptionPolicyService subscriptionPolicyService)
    : IRequestHandler<CreatePropertyCommand, PropertyResponse>
{
    public async Task<PropertyResponse> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var entitlement = await subscriptionPolicyService.GetEntitlementAsync(request.OwnerId, cancellationToken);
        var existingCount = await context.Properties.CountAsync(x => x.OwnerId == request.OwnerId, cancellationToken);

        if (!entitlement.UnlimitedListings && existingCount >= entitlement.ListingLimit)
        {
            throw new InvalidOperationException($"Listing limit reached for plan {entitlement.PlanName}.");
        }

        if (request.IsFeatured && !entitlement.CanFeatureListings)
        {
            throw new InvalidOperationException("Featured listing is only available for premium plans.");
        }

        var property = new Property(
            request.OwnerId,
            request.Title,
            request.Description,
            request.Type,
            request.BHK,
            request.Price,
            new GeoLocation(request.City, request.Latitude, request.Longitude),
            request.IsFeatured);

        foreach (var amenity in request.Amenities)
        {
            property.AddAmenity(amenity);
        }

        foreach (var image in request.Images)
        {
            property.AddImage(image);
        }

        context.Properties.Add(property);
        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<PropertyResponse>(property);
    }
}

public sealed class UpdatePropertyStatusHandler(IApplicationDbContext context) : IRequestHandler<UpdatePropertyStatusCommand>
{
    public async Task Handle(UpdatePropertyStatusCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties.FirstOrDefaultAsync(x => x.Id == request.PropertyId, cancellationToken)
            ?? throw new KeyNotFoundException("Property not found.");
        property.SetStatus(request.Status);
        await context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetPropertyByIdHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPropertyByIdQuery, PropertyResponse>
{
    public async Task<PropertyResponse> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties.Include(x => x.Amenities).Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == request.PropertyId, cancellationToken)
            ?? throw new KeyNotFoundException("Property not found.");
        return mapper.Map<PropertyResponse>(property);
    }
}

public sealed class GetOwnerPropertiesHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetOwnerPropertiesQuery, IReadOnlyCollection<PropertyResponse>>
{
    public async Task<IReadOnlyCollection<PropertyResponse>> Handle(GetOwnerPropertiesQuery request, CancellationToken cancellationToken)
    {
        var properties = await context.Properties.Include(x => x.Amenities).Include(x => x.Images).Where(x => x.OwnerId == request.OwnerId).ToListAsync(cancellationToken);
        return properties.Select(x => mapper.Map<PropertyResponse>(x)).ToArray();
    }
}
