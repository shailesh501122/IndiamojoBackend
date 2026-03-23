using FluentValidation;
using MediatR;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Reviews;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Reviews;

public sealed record AddReviewCommand(Guid PropertyId, Guid UserId, int Rating, string Comment) : IRequest<Guid>;

public sealed class AddReviewValidator : AbstractValidator<AddReviewCommand>
{
    public AddReviewValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000);
    }
}

public sealed class AddReviewHandler(IApplicationDbContext context) : IRequestHandler<AddReviewCommand, Guid>
{
    public async Task<Guid> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        var review = new Review(request.PropertyId, request.UserId, request.Rating, request.Comment);
        context.Reviews.Add(review);
        await context.SaveChangesAsync(cancellationToken);
        return review.Id;
    }
}
