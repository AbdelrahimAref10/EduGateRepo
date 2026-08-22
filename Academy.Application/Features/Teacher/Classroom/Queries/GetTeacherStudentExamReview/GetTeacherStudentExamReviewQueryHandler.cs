using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherStudentExamReview;

public sealed class GetTeacherStudentExamReviewQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherStudentExamReviewQuery, Result<TeacherStudentExamReviewDto>>
{
    public async Task<Result<TeacherStudentExamReviewDto>> Handle(
        GetTeacherStudentExamReviewQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherStudentExamReviewDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<TeacherStudentExamReviewDto>.NotFound("الحصة غير موجودة.");

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
