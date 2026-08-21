using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.StudentBookings.Commands.ConfirmBooking;

public sealed record ConfirmBookingCommand(int UserId, int BookingId)
    : IRequest<Result<TeacherBookingDto>>;
