using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.StudentBookings.Commands.RejectBooking;

public sealed record RejectBookingCommand(int UserId, int BookingId)
    : IRequest<Result<TeacherBookingDto>>;
