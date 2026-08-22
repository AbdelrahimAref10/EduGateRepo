using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Student.Classroom.Dtos;
using Academy.Application.Features.Student.Classroom.Queries.GetStudentSessionExam;
using Academy.Domain.Entities;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Student.Classroom.Commands.StartStudentSessionExam;

public sealed class StartStudentSessionExamCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<StartStudentSessionExamCommand, Result<StudentExamDto>>
{
    public async Task<Result<StudentExamDto>> Handle(
        StartStudentSessionExamCommand request,
        CancellationToken cancellationToken)
    {
        var access = await StudentExamAccess.ResolveAsync(dbContext, request.UserId, request.SessionId, cancellationToken);
        if (!access.IsSuccess)
            return Result<StudentExamDto>.Failure(access.Error, access.StatusCode);

        var exam = await dbContext.Exams
            .AsTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Attempts.Where(a => a.StudentId == access.Value!.StudentId))
                .ThenInclude(a => a.Answers)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null || exam.Status != ExamStatus.Published)
            return Result<StudentExamDto>.NotFound("الامتحان غير متاح بعد.");

        var attempt = exam.Attempts.FirstOrDefault();
        if (attempt is null)
        {
            var now = DateTime.UtcNow;
            attempt = new ExamAttempt
            {
                ExamId = exam.Id,
                StudentId = access.Value!.StudentId,
                MaxScore = exam.Questions.Count,
                CurrentQuestionIndex = 0,
                CurrentQuestionStartedAtUtc = now,
                StartedAtUtc = now
            };
            dbContext.ExamAttempts.Add(attempt);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await StudentExamProgress.ApplyExpiredQuestionsAsync(dbContext, exam, attempt, cancellationToken);
        }

        return Result<StudentExamDto>.Success(StudentExamProgress.ToDto(exam, attempt));
    }
}
