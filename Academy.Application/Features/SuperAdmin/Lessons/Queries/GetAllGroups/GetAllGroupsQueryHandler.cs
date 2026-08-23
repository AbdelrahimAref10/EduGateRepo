using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Lessons.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Lessons.Queries.GetAllGroups;

public sealed class GetAllGroupsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetAllGroupsQuery, Result<IReadOnlyList<AdminGroupListItemDto>>>
{
    public async Task<Result<IReadOnlyList<AdminGroupListItemDto>>> Handle(
        GetAllGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var language = requestLanguage.Current;

        var groups = await dbContext.LessonGroups
            .AsNoTracking()
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Teacher)
                    .ThenInclude(x => x.User)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.EducationSubject)
            .Include(x => x.Area)
                .ThenInclude(x => x.City)
            .Include(x => x.Members)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var sessionCounts = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .GroupBy(x => x.LessonGroupId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var items = groups.Select(group =>
        {
            var members = group.Members
                .OrderBy(x => x.AddedAtUtc)
                .Select(x => new AdminGroupMemberDto
                {
                    Id = x.Id,
                    StudentId = x.StudentId,
                    StudentName = x.Student.User.FullName,
                    StudentCode = x.Student.StudentCode,
                    AddedAtUtc = x.AddedAtUtc
                })
                .ToList();

            return new AdminGroupListItemDto
            {
                Id = group.Id,
                Name = group.Name,
                LessonId = group.LessonId,
                LessonSubject = LocalizedNames.Pick(
                    group.Lesson.EducationSubject.NameAr,
                    group.Lesson.EducationSubject.NameEn,
                    language),
                TeacherId = group.Lesson.TeacherId,
                TeacherName = group.Lesson.Teacher.User.FullName,
                AreaName = LocalizedNames.Pick(group.Area.NameAr, group.Area.NameEn, language),
                CityName = LocalizedNames.Pick(
                    group.Area.City.NameAr,
                    group.Area.City.NameEn,
                    language),
                BillingType = group.Lesson.BillingType.ToString(),
                SessionPrice = group.Lesson.SessionPrice,
                MonthlyPrice = group.Lesson.MonthlyPrice,
                SessionsCount = sessionCounts.GetValueOrDefault(group.Id),
                StartedAtUtc = group.StartedAtUtc,
                EndedAtUtc = group.EndedAtUtc,
                HasStarted = group.StartedAtUtc.HasValue,
                HasEnded = group.EndedAtUtc.HasValue,
                MembersCount = members.Count,
                CreatedAtUtc = group.CreatedAtUtc,
                Members = members
            };
        }).ToList();

        return Result<IReadOnlyList<AdminGroupListItemDto>>.Success(items);
    }
}
