using Academy.Application.Common.Images;
using Academy.Application.Common.Localization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.SuperAdmin.Groups.Dtos;
using Academy.Application.Features.Teacher.Classroom;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminClassroom;

public sealed class GetAdminClassroomQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetAdminClassroomQuery, Result<AdminClassroomDto>>
{
    public async Task<Result<AdminClassroomDto>> Handle(
        GetAdminClassroomQuery request,
        CancellationToken cancellationToken)
    {
        var session = await AdminSessionAccess.LoadSessionAsync(
            dbContext,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<AdminClassroomDto>.NotFound("الحصة غير موجودة.");

        if (session.StartedAtUtc is null)
            return Result<AdminClassroomDto>.Conflict("لم يتم بدء الحصة بعد.");

        var language = requestLanguage.Current;
        var lesson = session.LessonGroup.Lesson;

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .OrderBy(x => x.AddedAtUtc)
            .ToListAsync(cancellationToken);

        var details = await dbContext.LessonSessionStudentDetails
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.LessonGroupSessionId == session.Id)
            .ToListAsync(cancellationToken);

        var detailByStudentId = details.ToDictionary(x => x.StudentId);

        var students = new List<ClassroomStudentDetailDto>();
        foreach (var member in members)
        {
            var charges = await ClassroomChargeQuery.ForStudentAsync(
                dbContext,
                lesson,
                session,
                member.StudentId,
                cancellationToken);
            var (outstanding, status) = Charge.Summarize(charges);

            if (detailByStudentId.TryGetValue(member.StudentId, out var detail))
            {
                students.Add(ClassroomMappings.ToStudentDetailDto(detail, outstanding, status));
                continue;
            }

            students.Add(new ClassroomStudentDetailDto
            {
                Id = 0,
                StudentId = member.StudentId,
                StudentName = member.Student.User.FullName,
                PhotoUrl = ImageService.DisplayValue(member.Student.User.ProfilePhoto),
                StudentCode = member.Student.StudentCode,
                IsPresent = false,
                OutstandingAmount = outstanding,
                BillingStatus = status
            });
        }

        var materialRows = await dbContext.LessonSessionMaterials
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Where(x => x.LessonGroupSessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var materials = materialRows.Select(ClassroomMappings.ToMaterialDto).ToList();

        var exam = await dbContext.Exams
            .AsNoTracking()
            .Where(x => x.LessonGroupSessionId == session.Id)
            .Select(x => new { x.Title, Status = (int)x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        var sessionNumber = await SessionNumbers.RankAsync(dbContext, session, cancellationToken);

        return Result<AdminClassroomDto>.Success(new AdminClassroomDto
        {
            SessionId = session.Id,
            LessonId = lesson.Id,
            LessonGroupId = session.LessonGroupId,
            SessionNumber = sessionNumber,
            GroupName = session.LessonGroup.Name,
            Subject = LocalizedNames.Pick(
                lesson.EducationSubject.NameAr,
                lesson.EducationSubject.NameEn,
                language),
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            Topic = session.Topic,
            Description = session.Description,
            HasStarted = session.StartedAtUtc.HasValue,
            HasEnded = session.EndedAtUtc.HasValue,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = session.EndedAtUtc,
            TeacherName = lesson.Teacher.User.FullName,
            TeacherPhotoUrl = ImageService.DisplayValue(lesson.Teacher.User.ProfilePhoto),
            BillingType = lesson.BillingType.ToString(),
            SessionPrice = lesson.SessionPrice,
            MonthlyPrice = lesson.MonthlyPrice,
            HasExam = exam is not null,
            ExamStatus = exam?.Status,
            ExamTitle = exam?.Title,
            ReviewCount = session.RatingCount,
            RatingAverage = session.RatingAverage,
            Students = students,
            Materials = materials
        });
    }
}
