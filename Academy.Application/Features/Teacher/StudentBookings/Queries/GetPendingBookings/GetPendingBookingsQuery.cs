using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.StudentBookings.Queries.GetPendingBookings;

public sealed record GetPendingBookingsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<TeacherBookingDto>>>;
