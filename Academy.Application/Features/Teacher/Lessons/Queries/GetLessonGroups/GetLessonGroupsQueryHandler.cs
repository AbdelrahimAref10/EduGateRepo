using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons.Queries.GetLessonGroups;

public sealed class GetLessonGroupsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetLessonGroupsQuery, Result<IReadOnlyList<LessonGroupDto>>>
{
    public async Task<Result<IReadOnlyList<LessonGroupDto>>> Handle(
        GetLessonGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var lesson = await dbContext.Lessons
            .Include(x => x.Groups)
                .ThenInclude(x => x.Dates)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Area)
                    .ThenInclude(x => x.City)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Members)
            .Include(x => x.Groups)
                .ThenInclude(x => x.Sessions)
            .FirstOrDefaultAsync(
                x => x.Id == request.LessonId && x.Teacher.UserId == request.UserId,
                cancellationToken);

        if (lesson is null)
            return Result<IReadOnlyList<LessonGroupDto>>.NotFound("Lesson was not found.");

        var groups = lesson.Groups
            .OrderBy(x => x.CreatedAtUtc)
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
                HasStarted = x.StartedAtUtc.HasValue,
                HasEnded = x.EndedAtUtc.HasValue,
                CanEdit = x.StartedAtUtc is null,
                CanDelete = x.StartedAtUtc is null || x.EndedAtUtc.HasValue,
                MembersCount = x.Members.Count,
                SessionsCount = x.Sessions.Count,
                CreatedAtUtc = x.CreatedAtUtc,
                Members = [],
                Sessions = []
            })
            .ToList();

        return Result<IReadOnlyList<LessonGroupDto>>.Success(groups);
    }
}
