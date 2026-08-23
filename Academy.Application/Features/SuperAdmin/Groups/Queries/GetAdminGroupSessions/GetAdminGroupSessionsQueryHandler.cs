using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminGroupSessions;

public sealed class GetAdminGroupSessionsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetAdminGroupSessionsQuery, Result<AdminGroupSessionsDto>>
{
    public async Task<Result<AdminGroupSessionsDto>> Handle(
        GetAdminGroupSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var language = requestLanguage.Current;

        var group = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x => x.Id == request.GroupId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.LessonId,
                LessonSubjectAr = x.Lesson.EducationSubject.NameAr,
                LessonSubjectEn = x.Lesson.EducationSubject.NameEn,
                TeacherName = x.Lesson.Teacher.User.FirstName + " " + x.Lesson.Teacher.User.LastName,
                BillingType = x.Lesson.BillingType.ToString(),
                x.Lesson.SessionPrice,
                x.Lesson.MonthlyPrice
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (group is null)
            return Result<AdminGroupSessionsDto>.NotFound("المجموعة غير موجودة.");

        var rows = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.LessonGroupId == request.GroupId)
            .OrderBy(x => x.SessionDate)
            .ThenBy(x => x.StartTime)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.LessonGroupId,
                x.SessionDate,
                x.StartTime,
                x.Topic,
                x.StartedAtUtc,
                x.EndedAtUtc,
                x.RatingCount,
                x.RatingAverage,
                x.CreatedAtUtc,
                HasExam = x.Exam != null,
                ExamStatus = x.Exam != null ? (int?)x.Exam.Status : null
            })
            .ToListAsync(cancellationToken);

        var sessions = rows
            .Select((x, index) => new AdminGroupSessionDto
            {
                Id = x.Id,
                LessonGroupId = x.LessonGroupId,
                SessionNumber = index + 1,
                SessionDate = x.SessionDate,
                StartTime = x.StartTime,
                Topic = x.Topic,
                StartedAtUtc = x.StartedAtUtc,
                EndedAtUtc = x.EndedAtUtc,
                HasStarted = x.StartedAtUtc != null,
                HasEnded = x.EndedAtUtc != null,
                CanOpenClassroom = x.StartedAtUtc != null,
                HasExam = x.HasExam,
                ExamStatus = x.ExamStatus,
                ReviewCount = x.RatingCount,
                RatingAverage = x.RatingAverage,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToList();

        return Result<AdminGroupSessionsDto>.Success(new AdminGroupSessionsDto
        {
            GroupId = group.Id,
            GroupName = group.Name,
            LessonId = group.LessonId,
            LessonSubject = LocalizedNames.Pick(group.LessonSubjectAr, group.LessonSubjectEn, language),
            TeacherName = group.TeacherName.Trim(),
            BillingType = group.BillingType,
            SessionPrice = group.SessionPrice,
            MonthlyPrice = group.MonthlyPrice,
            Sessions = sessions
        });
    }
}
