using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;

public sealed class GetStudentSessionExamQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStudentSessionExamQuery, Result<StudentExamDto?>>
{
    public async Task<Result<StudentExamDto?>> Handle(
        GetStudentSessionExamQuery request,
        CancellationToken cancellationToken)
    {
        var access = await StudentExamAccess.ResolveAsync(dbContext, request.UserId, request.SessionId, cancellationToken);
        if (!access.IsSuccess)
            return Result<StudentExamDto?>.Failure(access.Error, access.StatusCode);

        var exam = await dbContext.Exams
            .AsTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Attempts.Where(a => a.StudentId == access.Value!.StudentId))
                .ThenInclude(a => a.Answers)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null || exam.Status != ExamStatus.Published)
            return Result<StudentExamDto?>.Success(null);

        var attempt = exam.Attempts.FirstOrDefault();
        if (attempt is not null)
            await StudentExamProgress.ApplyExpiredQuestionsAsync(dbContext, exam, attempt, cancellationToken);

        return Result<StudentExamDto?>.Success(StudentExamProgress.ToDto(exam, attempt));
    }
}
