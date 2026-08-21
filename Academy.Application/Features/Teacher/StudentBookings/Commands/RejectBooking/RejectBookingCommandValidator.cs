using FluentValidation;

namespace Academy.Application.Features.Teacher.StudentBookings.Commands.RejectBooking;

public sealed class RejectBookingCommandValidator : AbstractValidator<RejectBookingCommand>
{
    public RejectBookingCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.BookingId).GreaterThan(0);
    }
}
