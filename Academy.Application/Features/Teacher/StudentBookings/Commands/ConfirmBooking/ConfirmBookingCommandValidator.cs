using FluentValidation;

namespace Academy.Application.Features.Teacher.StudentBookings.Commands.ConfirmBooking;

public sealed class ConfirmBookingCommandValidator : AbstractValidator<ConfirmBookingCommand>
{
    public ConfirmBookingCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.BookingId).GreaterThan(0);
    }
}
