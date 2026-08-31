using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.LearningPath.Common;
using Academy.Application.Features.LearningPath.Dtos;
using Academy.Application.Features.Teacher.Students.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Students.Queries.GetTeacherStudentProgress;

public sealed record GetTeacherStudentProgressQuery(int UserId, int StudentId)
    : IRequest<Result<ProgressReportDto>>;

public sealed class GetTeacherStudentProgressQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherStudentProgressQuery, Result<ProgressReportDto>>
{
    public async Task<Result<ProgressReportDto>> Handle(
        GetTeacherStudentProgressQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherStudentAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<ProgressReportDto>.NotFound("Teacher profile was not found.");

        var allowed = await TeacherStudentAccess.IsTeachersConfirmedStudentAsync(
            dbContext, teacherId.Value, request.StudentId, cancellationToken);

        if (!allowed)
            return Result<ProgressReportDto>.NotFound("Student was not found.");

        var name = await dbContext.Students
            .AsNoTracking()
            .Where(s => s.Id == request.StudentId && !s.IsParent)
            .Select(s => s.User.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        if (name is null)
            return Result<ProgressReportDto>.NotFound("Student was not found.");

        var names = new Dictionary<int, string> { [request.StudentId] = name };
        var lessons = await LearningPathQueries.BuildProgressAsync(
            dbContext, [request.StudentId], names, teacherId.Value, lessonId: null, cancellationToken);

        return Result<ProgressReportDto>.Success(new ProgressReportDto { Lessons = lessons });
    }
}
