using Academy.Application.Features.Teacher.Students.Dtos;
using Academy.Domain.Entities;

namespace Academy.Application.Features.Teacher.Students.Common;

internal static class TeacherStudentGroupProjections
{
    public static IQueryable<TeacherStudentGroupDto> ToDto(
        this IQueryable<LessonGroup> query,
        int studentId,
        bool isArabic)
    {
        return query.Select(x => new TeacherStudentGroupDto
        {
            Id = x.Id,
            LessonId = x.LessonId,
            Name = x.Name,
            Dates = x.Dates
                .OrderBy(d => d.DayOfWeek)
                .ThenBy(d => d.StartTime)
                .Select(d => new TeacherStudentGroupDateDto
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
            MembersCount = x.Members.Count,
            RemainingCapacity = x.MaxCapacity == null
                ? null
                : x.MaxCapacity - x.Members.Count,
            IsFull = x.MaxCapacity != null && x.Members.Count >= x.MaxCapacity,
            IsEmpty = x.Members.Count == 0,
            IsCurrentStudentGroup = x.Members.Any(m => m.StudentId == studentId),
            StartedAtUtc = x.StartedAtUtc,
            EndedAtUtc = x.EndedAtUtc,
            HasStarted = x.StartedAtUtc != null,
            HasEnded = x.EndedAtUtc != null,
            CreatedAtUtc = x.CreatedAtUtc
        });
    }
}
