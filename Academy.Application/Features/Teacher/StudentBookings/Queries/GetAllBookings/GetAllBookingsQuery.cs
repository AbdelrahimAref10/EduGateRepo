using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.StudentBookings.Queries.GetAllBookings;

public sealed record GetAllBookingsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<TeacherBookingDto>>>;
