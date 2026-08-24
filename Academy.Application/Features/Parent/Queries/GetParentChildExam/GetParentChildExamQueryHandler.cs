using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentChildExam;

public sealed record GetParentChildExamQuery(int UserId, int ChildStudentId, int SessionId)
    : IRequest<Result<StudentExamDto?>>;

public sealed class GetParentChildExamQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentChildExamQuery, Result<StudentExamDto?>>
{
    public async Task<Result<StudentExamDto?>> Handle(
        GetParentChildExamQuery request,
        CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<StudentExamDto?>.NotFound("Parent profile was not found.");

        var linked = await ParentAccess.IsLinkedAsync(
            dbContext, parentStudentId.Value, request.ChildStudentId, cancellationToken);

        if (!linked)
            return Result<StudentExamDto?>.Failure("This child is not linked to your account.", 403);

        var isMember = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .AnyAsync(
                m => m.StudentId == request.ChildStudentId
                     && m.LessonGroup.Sessions.Any(s => s.Id == request.SessionId),
                cancellationToken);

        if (!isMember)
            return Result<StudentExamDto?>.NotFound("Exam session was not found for this child.");

        var exam = await dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Attempts.Where(a => a.StudentId == request.ChildStudentId))
                .ThenInclude(a => a.Answers)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null || exam.Status != ExamStatus.Published)
            return Result<StudentExamDto?>.Success(null);

        var attempt = exam.Attempts.FirstOrDefault();
        return Result<StudentExamDto?>.Success(StudentExamProgress.ToDto(exam, attempt));
    }
}
