using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Students.Common;
using Academy.Application.Features.Teacher.Students.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Queries.GetMyStudents;

public sealed class GetMyStudentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyStudentsQuery, Result<IReadOnlyList<TeacherStudentListItemDto>>>
{
    public async Task<Result<IReadOnlyList<TeacherStudentListItemDto>>> Handle(
        GetMyStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherStudentAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<TeacherStudentListItemDto>>.NotFound("Teacher profile was not found.");

        var studentIdsQuery = dbContext.LessonBookings
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value && x.Status == BookingStatus.Confirmed)
            .Select(x => x.StudentId)
            .Distinct();

        var studentsQuery = dbContext.Students
            .AsNoTracking()
            .Where(s => studentIdsQuery.Contains(s.Id) && !s.IsParent);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            studentsQuery = studentsQuery.Where(s =>
                s.User.FirstName.Contains(term)
                || s.User.LastName.Contains(term)
                || (s.User.FirstName + " " + s.User.LastName).Contains(term)
                || (s.User.PhoneNumber != null && s.User.PhoneNumber.Contains(term))
                || (s.StudentCode != null && s.StudentCode.Contains(term))
                || dbContext.ParentChildLinks.Any(l =>
                    l.ChildStudentId == s.Id
                    && (
                        l.ParentStudent.User.FirstName.Contains(term)
                        || l.ParentStudent.User.LastName.Contains(term)
                        || (l.ParentStudent.User.FirstName + " " + l.ParentStudent.User.LastName).Contains(term)
                        || (l.ParentStudent.User.PhoneNumber != null && l.ParentStudent.User.PhoneNumber.Contains(term)))));
        }

        var rows = await studentsQuery
            .OrderBy(s => s.User.FirstName)
            .ThenBy(s => s.User.LastName)
            .Select(s => new
            {
                StudentId = s.Id,
                FullName = (s.User.FirstName + " " + s.User.LastName).Trim(),
                Photo = s.User.ProfilePhoto,
                s.StudentCode,
                s.User.PhoneNumber,
                LessonsCount = dbContext.LessonBookings.Count(b =>
                    b.TeacherId == teacherId.Value
                    && b.StudentId == s.Id
                    && b.Status == BookingStatus.Confirmed)
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return Result<IReadOnlyList<TeacherStudentListItemDto>>.Success([]);

        var studentIds = rows.Select(x => x.StudentId).ToList();

        var parentRows = await dbContext.ParentChildLinks
            .AsNoTracking()
            .Where(x => studentIds.Contains(x.ChildStudentId))
            .Select(x => new
            {
                x.ChildStudentId,
                x.ParentStudentId,
                FullName = (x.ParentStudent.User.FirstName + " " + x.ParentStudent.User.LastName).Trim(),
                x.ParentStudent.User.PhoneNumber
            })
            .ToListAsync(cancellationToken);

        var parentsByChild = parentRows
            .GroupBy(x => x.ChildStudentId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<TeacherStudentParentDto>)g
                    .Select(p => new TeacherStudentParentDto
                    {
                        ParentStudentId = p.ParentStudentId,
                        FullName = p.FullName,
                        PhoneNumber = p.PhoneNumber
                    })
                    .ToList());

        IReadOnlyList<TeacherStudentListItemDto> items = rows
            .Select(x => new TeacherStudentListItemDto
            {
                StudentId = x.StudentId,
                FullName = x.FullName,
                PhotoUrl = ImageService.DisplayValue(x.Photo),
                StudentCode = x.StudentCode,
                PhoneNumber = x.PhoneNumber,
                LessonsCount = x.LessonsCount,
                Parents = parentsByChild.TryGetValue(x.StudentId, out var parents)
                    ? parents
                    : []
            })
            .ToList();

        return Result<IReadOnlyList<TeacherStudentListItemDto>>.Success(items);
    }
}
