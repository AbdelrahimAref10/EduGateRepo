using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroup;

public sealed class GetLessonGroupQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonGroupQuery, Result<LessonGroupDto>>
{
    public async Task<Result<LessonGroupDto>> Handle(
        GetLessonGroupQuery request,
        CancellationToken cancellationToken)
    {
        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var group = await dbContext.LessonGroups
            .Where(x =>
                x.Id == request.GroupId
                && x.LessonId == request.LessonId
                && x.Lesson.Teacher.UserId == request.UserId)
            .Select(x => new LessonGroupDto
            {
                Id = x.Id,
                LessonId = x.LessonId,
                Name = x.Name,
                Dates = x.Dates
                    .OrderBy(d => d.DayOfWeek)
                    .ThenBy(d => d.StartTime)
                    .Select(d => new LessonGroupDateDto
                    {
                        Id = d.Id,
                        DayOfWeek = d.DayOfWeek,
                        StartTime = d.StartTime
                    })
                    .ToList(),
                PeriodStartDate = x.PeriodStartDate,
                PeriodEndDate = x.PeriodEndDate,
                AreaId = x.AreaId,
                AreaName = isArabic ? x.Area.NameAr : x.Area.NameEn,
                CityId = x.Area.CityId,
                CityName = isArabic ? x.Area.City.NameAr : x.Area.City.NameEn,
                Address = x.Address,
                Notes = x.Notes,
                MaxCapacity = x.MaxCapacity,
                StartedAtUtc = x.StartedAtUtc,
                EndedAtUtc = x.EndedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                HasEnded = x.EndedAtUtc != null,
                CanEdit = x.StartedAtUtc == null,
                CanDelete = x.StartedAtUtc == null || x.EndedAtUtc != null,
                MembersCount = x.Members.Count,
                SessionsCount = x.Sessions.Count,
                CreatedAtUtc = x.CreatedAtUtc,
                Members = x.Members
                    .OrderBy(m => m.AddedAtUtc)
                    .Select(m => new LessonGroupMemberDto
                    {
                        Id = m.Id,
                        StudentId = m.StudentId,
                        StudentName = m.Student.User.FullName,
                        StudentCode = m.Student.StudentCode,
                        AddedAtUtc = m.AddedAtUtc
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return group is null
            ? Result<LessonGroupDto>.NotFound("Group was not found.")
            : Result<LessonGroupDto>.Success(group);
    }
}
