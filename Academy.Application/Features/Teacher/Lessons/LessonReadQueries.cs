using Academy.Application.Common.Images;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Lessons.Dtos;
using Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Lessons;

internal static class LessonReadQueries
{
    public static async Task<int?> GetTeacherIdAsync(
        IApplicationDbContext dbContext,
        int userId,
        CancellationToken cancellationToken)
    {
        var teacherId = await dbContext.Teachers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return teacherId == 0 ? null : teacherId;
    }

    public static async Task<LessonDto?> GetLessonHeaderAsync(
        IApplicationDbContext dbContext,
        int teacherId,
        int lessonId,
        AppLanguage language,
        CancellationToken cancellationToken)
    {
        var isArabic = language == AppLanguage.Arabic;

        return await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Id == lessonId && x.TeacherId == teacherId)
            .Select(x => new LessonDto
            {
                Id = x.Id,
                TeacherId = x.TeacherId,
                Subject = isArabic ? x.EducationSubject.NameAr : x.EducationSubject.NameEn,
                EducationSubjectId = x.EducationSubjectId,
                EducationTypeId = x.EducationTypeId,
                EducationTypeName = isArabic ? x.EducationType.NameAr : x.EducationType.NameEn,
                EducationStageId = x.EducationStageId,
                EducationStageName = isArabic ? x.EducationStage.NameAr : x.EducationStage.NameEn,
                EducationYearId = x.EducationYearId,
                EducationYearName = isArabic ? x.EducationYear.NameAr : x.EducationYear.NameEn,
                BillingType = x.BillingType.ToString(),
                SessionPrice = x.SessionPrice,
                MonthlyPrice = x.MonthlyPrice,
                StartDate = x.StartDate,
                CountryId = x.CountryId,
                CountryName = isArabic ? x.Country.NameAr : x.Country.NameEn,
                AreaId = x.AreaId,
                AreaName = isArabic ? x.Area.NameAr : x.Area.NameEn,
                CityId = x.Area.CityId,
                CityName = isArabic ? x.Area.City.NameAr : x.Area.City.NameEn,
                IsActive = x.IsActive,
                StartedAtUtc = x.StartedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                CanEdit = !x.Groups.Any(g => g.StartedAtUtc != null),
                GroupsCount = x.Groups.Count,
                BookingsCount = x.Bookings.Count,
                ConfirmedBookingsCount = x.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<IReadOnlyList<LessonStudentDto>> GetLessonStudentsAsync(
        IApplicationDbContext dbContext,
        int lessonId,
        bool confirmedOnly,
        CancellationToken cancellationToken)
    {
        var bookingsQuery = dbContext.LessonBookings
            .AsNoTracking()
            .Where(x => x.LessonId == lessonId);

        if (confirmedOnly)
            bookingsQuery = bookingsQuery.Where(x => x.Status == BookingStatus.Confirmed);

        var bookings = await bookingsQuery
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

        if (bookings.Count == 0)
            return [];

        var assignments = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(x => x.LessonGroup.LessonId == lessonId)
            .Select(x => new
            {
                x.StudentId,
                x.LessonGroupId,
                GroupName = x.LessonGroup.Name
            })
            .ToListAsync(cancellationToken);

        var byStudent = assignments.ToDictionary(x => x.StudentId);

        return bookings.Select(x =>
        {
            byStudent.TryGetValue(x.StudentId, out var group);
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
                AssignedGroupId = group?.LessonGroupId,
                AssignedGroupName = group?.GroupName
            };
        }).ToList();
    }

    public static async Task<IReadOnlyList<LessonGroupDto>> GetLessonGroupsAsync(
        IApplicationDbContext dbContext,
        int lessonId,
        AppLanguage language,
        CancellationToken cancellationToken)
    {
        var headers = await QueryGroupHeaders(dbContext, language)
            .Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (headers.Count == 0)
            return [];

        var groupIds = headers.Select(x => x.Id).ToList();

        var dateRows = await dbContext.LessonGroupDates
            .AsNoTracking()
            .Where(x => groupIds.Contains(x.LessonGroupId))
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .Select(x => new
            {
                x.LessonGroupId,
                x.Id,
                x.DayOfWeek,
                x.StartTime
            })
            .ToListAsync(cancellationToken);

        var members = await QueryGroupMembers(dbContext)
            .Where(x => groupIds.Contains(x.LessonGroupId))
            .OrderBy(x => x.AddedAtUtc)
            .ToListAsync(cancellationToken);

        var datesByGroup = dateRows
            .GroupBy(x => x.LessonGroupId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<LessonGroupDateDto>)g
                    .Select(x => new LessonGroupDateDto
                    {
                        Id = x.Id,
                        DayOfWeek = x.DayOfWeek,
                        StartTime = x.StartTime
                    })
                    .ToList());

        var membersByGroup = members
            .GroupBy(x => x.LessonGroupId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<LessonGroupMemberDto>)g.Select(ToMemberDto).ToList());

        return headers
            .Select(header => ToGroupDto(
                header,
                datesByGroup.GetValueOrDefault(header.Id, []),
                membersByGroup.GetValueOrDefault(header.Id, [])))
            .ToList();
    }

    public static async Task<LessonGroupDto?> GetGroupAsync(
        IApplicationDbContext dbContext,
        int teacherId,
        int lessonId,
        int groupId,
        AppLanguage language,
        CancellationToken cancellationToken)
    {
        var header = await QueryGroupHeaders(dbContext, language)
            .Where(x => x.Id == groupId && x.LessonId == lessonId && x.TeacherId == teacherId)
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
            return null;

        var dates = await dbContext.LessonGroupDates
            .AsNoTracking()
            .Where(x => x.LessonGroupId == groupId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .Select(x => new LessonGroupDateDto
            {
                Id = x.Id,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime
            })
            .ToListAsync(cancellationToken);

        var members = await QueryGroupMembers(dbContext)
            .Where(x => x.LessonGroupId == groupId)
            .OrderBy(x => x.AddedAtUtc)
            .ToListAsync(cancellationToken);

        return ToGroupDto(header, dates, members.Select(ToMemberDto).ToList());
    }

    public static async Task<bool> GroupExistsAsync(
        IApplicationDbContext dbContext,
        int teacherId,
        int lessonId,
        int groupId,
        CancellationToken cancellationToken)
    {
        return await dbContext.LessonGroups
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == groupId
                     && x.LessonId == lessonId
                     && x.Lesson.TeacherId == teacherId,
                cancellationToken);
    }

    public static async Task<IReadOnlyList<LessonGroupSessionDto>> GetGroupSessionsAsync(
        IApplicationDbContext dbContext,
        int groupId,
        CancellationToken cancellationToken)
    {
        return await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.LessonGroupId == groupId)
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ThenBy(x => x.Id)
            .Select(x => new LessonGroupSessionDto
            {
                Id = x.Id,
                LessonGroupId = x.LessonGroupId,
                SessionDate = x.SessionDate,
                StartTime = x.StartTime,
                Topic = x.Topic,
                Description = x.Description,
                StartedAtUtc = x.StartedAtUtc,
                EndedAtUtc = x.EndedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                HasEnded = x.EndedAtUtc != null,
                CanStart = x.StartedAtUtc == null
                    && x.EndedAtUtc == null
                    && x.LessonGroup.EndedAtUtc == null,
                CanOpenClassroom = x.StartedAtUtc != null,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<GroupHeaderRow> QueryGroupHeaders(
        IApplicationDbContext dbContext,
        AppLanguage language)
    {
        var isArabic = language == AppLanguage.Arabic;

        return dbContext.LessonGroups
            .AsNoTracking()
            .Select(x => new GroupHeaderRow
            {
                Id = x.Id,
                LessonId = x.LessonId,
                TeacherId = x.Lesson.TeacherId,
                Name = x.Name,
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
                CreatedAtUtc = x.CreatedAtUtc,
                MembersCount = x.Members.Count,
                SessionsCount = x.Sessions.Count
            });
    }

    private static IQueryable<GroupMemberRow> QueryGroupMembers(IApplicationDbContext dbContext) =>
        dbContext.LessonGroupMembers
            .AsNoTracking()
            .Select(x => new GroupMemberRow
            {
                Id = x.Id,
                LessonGroupId = x.LessonGroupId,
                StudentId = x.StudentId,
                StudentName = x.Student.User.FullName,
                Photo = x.Student.User.ProfilePhoto,
                StudentCode = x.Student.StudentCode,
                AddedAtUtc = x.AddedAtUtc
            });

    private static LessonGroupDto ToGroupDto(
        GroupHeaderRow header,
        IReadOnlyList<LessonGroupDateDto> dates,
        IReadOnlyList<LessonGroupMemberDto> members)
    {
        var hasStarted = header.StartedAtUtc.HasValue;
        var hasEnded = header.EndedAtUtc.HasValue;

        return new LessonGroupDto
        {
            Id = header.Id,
            LessonId = header.LessonId,
            Name = header.Name,
            Dates = dates,
            PeriodStartDate = header.PeriodStartDate,
            PeriodEndDate = header.PeriodEndDate,
            AreaId = header.AreaId,
            AreaName = header.AreaName,
            CityId = header.CityId,
            CityName = header.CityName,
            Address = header.Address,
            Notes = header.Notes,
            MaxCapacity = header.MaxCapacity,
            StartedAtUtc = header.StartedAtUtc,
            EndedAtUtc = header.EndedAtUtc,
            HasStarted = hasStarted,
            HasEnded = hasEnded,
            CanEdit = !hasStarted,
            CanDelete = !hasStarted || hasEnded,
            MembersCount = header.MembersCount,
            SessionsCount = header.SessionsCount,
            CreatedAtUtc = header.CreatedAtUtc,
            Members = members,
            Sessions = []
        };
    }

    private static LessonGroupMemberDto ToMemberDto(GroupMemberRow row) =>
        new()
        {
            Id = row.Id,
            StudentId = row.StudentId,
            StudentName = row.StudentName,
            PhotoUrl = ImageService.DisplayValue(row.Photo),
            StudentCode = row.StudentCode,
            AddedAtUtc = row.AddedAtUtc
        };

    private sealed class GroupHeaderRow
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int TeacherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? MaxCapacity { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int MembersCount { get; set; }
        public int SessionsCount { get; set; }
    }

    private sealed class GroupMemberRow
    {
        public int Id { get; set; }
        public int LessonGroupId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public string? StudentCode { get; set; }
        public DateTime AddedAtUtc { get; set; }
    }
}
