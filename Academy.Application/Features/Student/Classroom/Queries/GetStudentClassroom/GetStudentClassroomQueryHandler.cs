using Academy.Application.Common.Models;
using Academy.Application.Common.Images;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Application.Features.Teacher.Classroom;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentClassroom;

public sealed class GetStudentClassroomQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStudentClassroomQuery, Result<StudentClassroomDto>>
{
    public async Task<Result<StudentClassroomDto>> Handle(
        GetStudentClassroomQuery request,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && !x.IsParent, cancellationToken);

        if (student is null)
            return Result<StudentClassroomDto>.NotFound("Student profile was not found.");

        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Include(x => x.LessonGroup)
                .ThenInclude(x => x.Lesson)
                    .ThenInclude(x => x.Teacher)
                        .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.SessionId, cancellationToken);

        if (session is null)
            return Result<StudentClassroomDto>.NotFound("الحصة غير موجودة.");

        var isMember = session.IsMakeup
            ? await dbContext.LessonSessionStudentDetails.AnyAsync(
                x => x.LessonGroupSessionId == session.Id && x.StudentId == student.Id,
                cancellationToken)
            : await dbContext.LessonGroupMembers.AnyAsync(
                x => x.LessonGroupId == session.LessonGroupId && x.StudentId == student.Id,
                cancellationToken);

        if (!isMember)
            return Result<StudentClassroomDto>.NotFound("الحصة غير موجودة.");

        if (!session.StartedAtUtc.HasValue)
            return Result<StudentClassroomDto>.Conflict("لم يتم بدء الحصة بعد.");

        if (!session.IsMakeup)
            await ClassroomSeeding.EnsureStudentDetailsAsync(dbContext, session, cancellationToken);

        var members = session.IsMakeup
            ? []
            : await dbContext.LessonGroupMembers
                .AsNoTracking()
                .Include(x => x.Student)
                    .ThenInclude(x => x.User)
                .Where(x => x.LessonGroupId == session.LessonGroupId)
                .OrderBy(x => x.AddedAtUtc)
                .ToListAsync(cancellationToken);

        var myDetailEntity = await dbContext.LessonSessionStudentDetails
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.LessonGroupSessionId == session.Id && x.StudentId == student.Id,
                cancellationToken);

        var materials = await dbContext.LessonSessionMaterials
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Where(x => x.LessonGroupSessionId == session.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var classmates = members
            .Where(m => m.StudentId != student.Id)
            .Select(m => new StudentClassroomClassmateDto
            {
                StudentName = m.Student.User.FullName,
                PhotoUrl = ImageService.DisplayValue(m.Student.User.ProfilePhoto),
                StudentCode = m.Student.StudentCode
            })
            .ToList();

        var lesson = session.LessonGroup.Lesson;
        decimal outstanding = 0;
        var billingStatus = "None";
        if (myDetailEntity is not null)
        {
            var charges = await ClassroomChargeQuery.ForStudentAsync(
                dbContext,
                lesson,
                session,
                student.Id,
                cancellationToken);
            (outstanding, billingStatus) = Charge.Summarize(charges);
        }

        var dto = new StudentClassroomDto
        {
            SessionId = session.Id,
            LessonId = lesson.Id,
            LessonGroupId = session.LessonGroupId,
            GroupName = session.LessonGroup.Name,
            Subject = lesson.Subject,
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
            MyDetail = myDetailEntity is null
                ? null
                : ClassroomMappings.ToStudentDetailDto(myDetailEntity, outstanding, billingStatus),
            Classmates = classmates,
            Materials = materials.Select(ClassroomMappings.ToMaterialDto).ToList()
        };

        return Result<StudentClassroomDto>.Success(dto);
    }
}
