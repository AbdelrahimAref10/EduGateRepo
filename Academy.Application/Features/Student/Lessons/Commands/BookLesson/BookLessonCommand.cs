using Academy.Application.Common.Models;
using Academy.Application.Features.Student.Lessons.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Lessons.Commands.BookLesson;

public sealed record BookLessonCommand(int UserId, int LessonId)
    : IRequest<Result<BookingDto>>;
