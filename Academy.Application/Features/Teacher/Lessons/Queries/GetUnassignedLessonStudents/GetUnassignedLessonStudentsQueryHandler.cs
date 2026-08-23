using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetUnassignedLessonStudents;

public sealed class GetUnassignedLessonStudentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetUnassignedLessonStudentsQuery, Result<IReadOnlyList<LessonStudentDto>>>
{
    public async Task<Result<IReadOnlyList<LessonStudentDto>>> Handle(
        GetUnassignedLessonStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons
            .Where(x => x.Id == request.LessonId && x.Teacher.UserId == request.UserId)
            .Select(x => new
            {
                Students = x.Bookings
                    .Where(b =>
                        b.Status == BookingStatus.Confirmed
                        && !b.Student.GroupMemberships.Any(m => m.LessonGroup.LessonId == request.LessonId))
                    .OrderByDescending(b => b.CreatedAtUtc)
                    .Select(b => new
                    {
                        b.Id,
                        b.StudentId,
                        StudentName = b.Student.User.FullName,
                        Photo = b.Student.User.ProfilePhoto,
                        b.Student.StudentCode,
                        b.Status,
                        b.CreatedAtUtc,
                        b.ReviewedAtUtc
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return Result<IReadOnlyList<LessonStudentDto>>.NotFound("Lesson was not found.");

        var students = lesson.Students
            .Select(x => new LessonStudentDto
            {
                BookingId = x.Id,
                StudentId = x.StudentId,
                StudentName = x.StudentName,
                PhotoUrl = ImageService.DisplayValue(x.Photo),
                StudentCode = x.StudentCode,
                Status = x.Status.ToString(),
                CreatedAtUtc = x.CreatedAtUtc,
                ReviewedAtUtc = x.ReviewedAtUtc
            })
            .ToList();

        return Result<IReadOnlyList<LessonStudentDto>>.Success(students);
    }
}
