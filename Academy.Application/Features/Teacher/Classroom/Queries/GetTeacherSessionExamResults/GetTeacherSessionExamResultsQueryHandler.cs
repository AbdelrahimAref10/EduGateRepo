using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherSessionExamResults;

public sealed class GetTeacherSessionExamResultsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherSessionExamResultsQuery, Result<TeacherExamResultsDto>>
{
    public async Task<Result<TeacherExamResultsDto>> Handle(
        GetTeacherSessionExamResultsQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherExamResultsDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<TeacherExamResultsDto>.NotFound("الحصة غير موجودة.");

        var exam = await dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .Include(x => x.Attempts)
                .ThenInclude(x => x.Answers)
            .Include(x => x.Attempts)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null)
            return Result<TeacherExamResultsDto>.NotFound("لا يوجد امتحان لهذه الحصة.");

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .Where(x => x.LessonGroupId == session.LessonGroupId)
            .OrderBy(x => x.AddedAtUtc)
            .ToListAsync(cancellationToken);

        var attemptByStudentId = exam.Attempts.ToDictionary(x => x.StudentId);

        var rows = members.Select(member =>
        {
            attemptByStudentId.TryGetValue(member.StudentId, out var attempt);
            var submitted = attempt?.SubmittedAtUtc is not null;
            return new TeacherExamResultRowDto
            {
                StudentId = member.StudentId,
                StudentName = member.Student.User.FullName,
                StudentCode = member.Student.StudentCode,
                HasSubmitted = submitted,
                Score = submitted ? attempt!.Score : null,
                MaxScore = submitted ? attempt!.MaxScore : null,
                Percentage = submitted && attempt!.MaxScore > 0
                    ? Math.Round(attempt.Score * 100m / attempt.MaxScore, 1)
                    : null,
                SubmittedAtUtc = attempt?.SubmittedAtUtc,
                Questions = submitted ? TeacherExamReviewMapper.ToQuestions(exam, attempt) : []
            };
        }).ToList();

        return Result<TeacherExamResultsDto>.Success(new TeacherExamResultsDto
        {
            ExamId = exam.Id,
            Title = exam.Title,
            Status = (int)exam.Status,
            SubmittedCount = rows.Count(x => x.HasSubmitted),
            StudentCount = rows.Count,
            Students = rows
        });
    }
}
