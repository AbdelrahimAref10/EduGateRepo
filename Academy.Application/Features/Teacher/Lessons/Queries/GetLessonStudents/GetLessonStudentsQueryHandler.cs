using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonStudents;

public sealed class GetLessonStudentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetLessonStudentsQuery, Result<IReadOnlyList<LessonStudentDto>>>
{
    public async Task<Result<IReadOnlyList<LessonStudentDto>>> Handle(
        GetLessonStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LessonStudentDto>>.NotFound("Teacher profile was not found.");

        var exists = await dbContext.Lessons
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.LessonId && x.TeacherId == teacherId, cancellationToken);

        if (!exists)
            return Result<IReadOnlyList<LessonStudentDto>>.NotFound("Lesson was not found.");

        var bookings = await dbContext.LessonBookings
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                StudentName = x.Student.User.FullName,
                Photo = x.Student.User.ProfilePhoto,
                x.Student.StudentCode,
                x.Status,
                x.CreatedAtUtc,
                x.ReviewedAtUtc
            })
            .ToListAsync(cancellationToken);

        var assignments = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(x => x.LessonGroup.LessonId == request.LessonId)
            .Select(x => new
            {
                x.StudentId,
                x.LessonGroupId,
                GroupName = x.LessonGroup.Name
            })
            .ToListAsync(cancellationToken);

        var byStudent = assignments.ToDictionary(x => x.StudentId);

        var students = bookings.Select(x =>
        {
            byStudent.TryGetValue(x.StudentId, out var assigned);
            return new LessonStudentDto
            {
                BookingId = x.Id,
                StudentId = x.StudentId,
                StudentName = x.StudentName,
                PhotoUrl = ImageService.DisplayValue(x.Photo),
                StudentCode = x.StudentCode,
                Status = x.Status.ToString(),
                CreatedAtUtc = x.CreatedAtUtc,
                ReviewedAtUtc = x.ReviewedAtUtc,
                AssignedGroupId = assigned?.LessonGroupId,
                AssignedGroupName = assigned?.GroupName
            };
        }).ToList();

        return Result<IReadOnlyList<LessonStudentDto>>.Success(students);
    }
}
