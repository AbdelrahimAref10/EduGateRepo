using Academy.Application.Common.Localization;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Entities;
using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Lessons;

internal static class LessonMappings
{
    private const int MaxGeneratedSessions = 400;

    public static LessonDto ToLessonDto(
        Lesson lesson,
        int groupsCount,
        int bookingsCount,
        int confirmedBookingsCount,
        bool hasStartedGroup,
        AppLanguage language)
    {
        return new LessonDto
        {
            Id = lesson.Id,
            TeacherId = lesson.TeacherId,
            Subject = lesson.Subject,
            EducationTypeId = lesson.EducationTypeId,
            EducationTypeName = LocalizedNames.Pick(
                lesson.EducationType.NameAr,
                lesson.EducationType.NameEn,
                language),
            EducationYearId = lesson.EducationYearId,
            EducationYearName = LocalizedNames.Pick(
                lesson.EducationYear.NameAr,
                lesson.EducationYear.NameEn,
                language),
            BillingType = lesson.BillingType.ToString(),
            SessionPrice = lesson.SessionPrice,
            MonthlyPrice = lesson.MonthlyPrice,
            StartDate = lesson.StartDate,
            CountryId = lesson.CountryId,
            CountryName = LocalizedNames.Pick(
                lesson.Country.NameAr,
                lesson.Country.NameEn,
                language),
            AreaId = lesson.AreaId,
            AreaName = LocalizedNames.Pick(
                lesson.Area.NameAr,
                lesson.Area.NameEn,
                language),
            CityId = lesson.Area.CityId,
            CityName = LocalizedNames.Pick(
                lesson.Area.City.NameAr,
                lesson.Area.City.NameEn,
                language),
            IsActive = lesson.IsActive,
            StartedAtUtc = lesson.StartedAtUtc,
            HasStarted = lesson.StartedAtUtc.HasValue,
            CanEdit = !hasStartedGroup,
            GroupsCount = groupsCount,
            BookingsCount = bookingsCount,
            ConfirmedBookingsCount = confirmedBookingsCount,
            CreatedAtUtc = lesson.CreatedAtUtc
        };
    }

    public static LessonGroupDto ToGroupDto(
        LessonGroup group,
        AppLanguage language,
        bool includeMembers = true,
        bool includeSessions = false)
    {
        var hasStarted = group.StartedAtUtc.HasValue;
        var hasEnded = group.EndedAtUtc.HasValue;

        return new LessonGroupDto
        {
            Id = group.Id,
            LessonId = group.LessonId,
            Name = group.Name,
            Dates = group.Dates
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .Select(x => new LessonGroupDateDto
                {
                    Id = x.Id,
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime
                })
                .ToList(),
            PeriodStartDate = group.PeriodStartDate,
            PeriodEndDate = group.PeriodEndDate,
            AreaId = group.AreaId,
            AreaName = LocalizedNames.Pick(group.Area.NameAr, group.Area.NameEn, language),
            CityId = group.Area.CityId,
            CityName = LocalizedNames.Pick(
                group.Area.City.NameAr,
                group.Area.City.NameEn,
                language),
            Address = group.Address,
            Notes = group.Notes,
            MaxCapacity = group.MaxCapacity,
            StartedAtUtc = group.StartedAtUtc,
            EndedAtUtc = group.EndedAtUtc,
            HasStarted = hasStarted,
            HasEnded = hasEnded,
            CanEdit = !hasStarted,
            CanDelete = !hasStarted || hasEnded,
            MembersCount = group.Members.Count,
            SessionsCount = group.Sessions.Count,
            CreatedAtUtc = group.CreatedAtUtc,
            Members = includeMembers
                ? group.Members
                    .OrderBy(x => x.AddedAtUtc)
                    .Select(x => new LessonGroupMemberDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        StudentName = x.Student.User.FullName,
                        StudentCode = x.Student.StudentCode,
                        AddedAtUtc = x.AddedAtUtc
                    })
                    .ToList()
                : [],
            Sessions = includeSessions
                ? group.Sessions
                    .OrderBy(x => x.SessionDate)
                    .ThenBy(x => x.StartTime)
                    .Select(x => ToSessionDto(x, group.EndedAtUtc.HasValue))
                    .ToList()
                : []
        };
    }

    public static LessonGroupSessionDto ToSessionDto(LessonGroupSession session, bool groupHasEnded = false)
    {
        var hasStarted = session.StartedAtUtc.HasValue;
        var hasEnded = session.EndedAtUtc.HasValue;

        return new LessonGroupSessionDto
        {
            Id = session.Id,
            LessonGroupId = session.LessonGroupId,
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            Topic = session.Topic,
            Description = session.Description,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            HasStarted = hasStarted,
            HasEnded = hasEnded,
            CanStart = !hasStarted && !hasEnded && !groupHasEnded,
            CanOpenClassroom = hasStarted,
            CreatedAtUtc = session.CreatedAtUtc
        };
    }

    public static string? TryBuildDates(
        IReadOnlyList<LessonGroupDateInputDto>? inputs,
        out List<LessonGroupDate> dates)
    {
        dates = [];

        if (inputs is null || inputs.Count == 0)
            return "أضف يوماً واحداً على الأقل للجدول.";

        var seen = new HashSet<DayOfWeek>();

        foreach (var input in inputs)
        {
            if (!Enum.IsDefined(input.DayOfWeek))
                return "يوم غير صالح في الجدول.";

            if (!seen.Add(input.DayOfWeek))
                return "لا يمكن تكرار نفس اليوم في الجدول.";

            dates.Add(new LessonGroupDate
            {
                DayOfWeek = input.DayOfWeek,
                StartTime = input.StartTime
            });
        }

        return null;
    }

    public static string? TryBuildSessions(
        DateOnly periodStart,
        DateOnly periodEnd,
        IReadOnlyList<LessonGroupDate> dates,
        out List<LessonGroupSession> sessions)
    {
        sessions = [];

        if (periodEnd < periodStart)
            return "تاريخ انتهاء المجموعة يجب أن يكون بعد أو يساوي تاريخ البداية.";

        if (periodEnd > periodStart.AddYears(2))
            return "مدة المجموعة لا يمكن أن تزيد عن سنتين.";

        var byDay = dates
            .GroupBy(x => x.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.First().StartTime);

        for (var day = periodStart; day <= periodEnd; day = day.AddDays(1))
        {
            if (!byDay.TryGetValue(day.DayOfWeek, out var startTime))
                continue;

            sessions.Add(new LessonGroupSession
            {
                SessionDate = day,
                StartTime = startTime,
                CreatedAtUtc = DateTime.UtcNow
            });

            if (sessions.Count > MaxGeneratedSessions)
                return $"عدد الحصص كبير جداً (الحد الأقصى {MaxGeneratedSessions}). قلّل المدة أو الأيام.";
        }

        if (sessions.Count == 0)
            return "لا توجد أيام مطابقة للجدول داخل الفترة المحددة.";

        return null;
    }
}
