using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Classroom;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateStudentSessionDetail;

public sealed class UpdateStudentSessionDetailCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateStudentSessionDetailCommand, Result<ClassroomStudentDetailDto>>
{
    public async Task<Result<ClassroomStudentDetailDto>> Handle(
        UpdateStudentSessionDetailCommand request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<ClassroomStudentDetailDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken,
            asTracking: true);

        if (session is null)
            return Result<ClassroomStudentDetailDto>.NotFound("الحصة غير موجودة.");

        await ClassroomSeeding.EnsureStudentDetailsAsync(dbContext, session, cancellationToken);

        var isMember = await dbContext.LessonGroupMembers
            .AnyAsync(
                x => x.LessonGroupId == session.LessonGroupId && x.StudentId == request.StudentId,
                cancellationToken);

        if (!isMember)
            return Result<ClassroomStudentDetailDto>.NotFound("الطالب غير موجود في هذه المجموعة.");

        var detail = await dbContext.LessonSessionStudentDetails
            .AsTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.LessonGroupSessionId == request.SessionId && x.StudentId == request.StudentId,
                cancellationToken);

        if (detail is null)
        {
            detail = new Domain.Entities.LessonSessionStudentDetail
            {
                LessonGroupSessionId = request.SessionId,
                StudentId = request.StudentId,
                CreatedAtUtc = DateTime.UtcNow
            };
            dbContext.LessonSessionStudentDetails.Add(detail);
        }

        detail.IsPresent = request.IsPresent;
        detail.IsPaid = request.IsPaid;
        detail.TeacherNotes = string.IsNullOrWhiteSpace(request.TeacherNotes)
            ? null
            : request.TeacherNotes.Trim();
        detail.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        if (detail.Student?.User is null)
        {
            detail = await dbContext.LessonSessionStudentDetails
                .AsNoTracking()
                .Include(x => x.Student)
                    .ThenInclude(x => x.User)
                .FirstAsync(x => x.Id == detail.Id, cancellationToken);
        }

        return Result<ClassroomStudentDetailDto>.Success(ClassroomMappings.ToStudentDetailDto(detail));
    }
}
