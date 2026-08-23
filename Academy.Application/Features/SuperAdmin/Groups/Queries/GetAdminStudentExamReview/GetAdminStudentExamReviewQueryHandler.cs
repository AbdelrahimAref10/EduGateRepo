using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminStudentExamReview;

public sealed class GetAdminStudentExamReviewQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminStudentExamReviewQuery, Result<TeacherStudentExamReviewDto>>
{
    public async Task<Result<TeacherStudentExamReviewDto>> Handle(
        GetAdminStudentExamReviewQuery request,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.LessonGroupSessions
            .AsNoTracking()
            .Where(x => x.Id == request.SessionId)
            .Select(x => new { x.Id, x.LessonGroupId, x.StartedAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return Result<TeacherStudentExamReviewDto>.NotFound("الحصة غير موجودة.");

        if (session.StartedAtUtc is null)
            return Result<TeacherStudentExamReviewDto>.Conflict("لم يتم بدء الحصة بعد.");

        var exam = await dbContext.Exams
            .AsNoTracking()
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.LessonGroupSessionId == request.SessionId, cancellationToken);

        if (exam is null)
            return Result<TeacherStudentExamReviewDto>.NotFound("لا يوجد امتحان لهذه الحصة.");

        var attempt = await dbContext.ExamAttempts
            .AsNoTracking()
            .Include(x => x.Answers)
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(
                x => x.ExamId == exam.Id && x.StudentId == request.StudentId,
                cancellationToken);

        if (attempt is null || attempt.SubmittedAtUtc is null)
        {
            var isMember = await dbContext.LessonGroupMembers
                .AsNoTracking()
                .AnyAsync(
                    x => x.LessonGroupId == session.LessonGroupId && x.StudentId == request.StudentId,
                    cancellationToken);

            if (!isMember)
                return Result<TeacherStudentExamReviewDto>.NotFound("الطالب غير موجود في هذه المجموعة.");

            return Result<TeacherStudentExamReviewDto>.NotFound("الطالب لم يسلّم الامتحان بعد.");
        }

        var student = attempt.Student;
        var questions = TeacherExamReviewMapper.ToQuestions(exam, attempt);

        return Result<TeacherStudentExamReviewDto>.Success(new TeacherStudentExamReviewDto
        {
            StudentId = student.Id,
            StudentName = student.User.FullName,
            StudentCode = student.StudentCode,
            Title = exam.Title,
            HasSubmitted = true,
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            Percentage = attempt.MaxScore > 0
                ? Math.Round(attempt.Score * 100m / attempt.MaxScore, 1)
                : 0,
            SubmittedAtUtc = attempt.SubmittedAtUtc,
            Questions = questions
        });
    }
}
