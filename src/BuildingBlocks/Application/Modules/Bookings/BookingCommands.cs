using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Bookings;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Notifications;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Bookings;

public sealed record ScheduleVisitCommand(Guid PropertyId, Guid TenantId, DateTime VisitDateUtc) : IRequest<BookingResponse>;
public sealed record ReviewBookingCommand(Guid BookingId, BookingStatus Status) : IRequest;

public sealed class ScheduleVisitValidator : AbstractValidator<ScheduleVisitCommand>
{
    public ScheduleVisitValidator()
    {
        RuleFor(x => x.VisitDateUtc).GreaterThan(DateTime.UtcNow);
    }
}

public sealed class ScheduleVisitHandler(IApplicationDbContext context, IMapper mapper, INotificationService notificationService)
    : IRequestHandler<ScheduleVisitCommand, BookingResponse>
{
    public async Task<BookingResponse> Handle(ScheduleVisitCommand request, CancellationToken cancellationToken)
    {
        var booking = new Booking(request.PropertyId, request.TenantId, request.VisitDateUtc);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(cancellationToken);
        await notificationService.SendAsync(request.TenantId, NotificationChannel.Email, "Visit scheduled", $"Visit scheduled for {request.VisitDateUtc:u}.", cancellationToken);
        return mapper.Map<BookingResponse>(booking);
    }
}

public sealed class ReviewBookingHandler(IApplicationDbContext context, INotificationService notificationService)
    : IRequestHandler<ReviewBookingCommand>
{
    public async Task Handle(ReviewBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await context.Bookings.FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking not found.");

        booking.Review(request.Status);
        await context.SaveChangesAsync(cancellationToken);
        await notificationService.SendAsync(booking.TenantId, NotificationChannel.Email, "Booking updated", $"Booking status changed to {request.Status}.", cancellationToken);
    }
}
