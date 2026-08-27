using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherStudentOutstanding;

public sealed record GetTeacherStudentOutstandingQuery(int UserId, int StudentId)
    : IRequest<Result<StudentOutstandingDto>>;

public sealed class GetTeacherStudentOutstandingQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherStudentOutstandingQuery, Result<StudentOutstandingDto>>
{
    public async Task<Result<StudentOutstandingDto>> Handle(
        GetTeacherStudentOutstandingQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<StudentOutstandingDto>.NotFound("Teacher profile was not found.");

        var student = await dbContext.Students
            .AsNoTracking()
            .Where(x => x.Id == request.StudentId && !x.IsParent)
            .Select(x => new
            {
                x.Id,
                Name = (x.User.FirstName + " " + x.User.LastName).Trim(),
                x.StudentCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (student is null)
            return Result<StudentOutstandingDto>.NotFound("الطالب غير موجود.");

        var isTeachersStudent = await dbContext.LessonBookings.AnyAsync(
                x => x.TeacherId == teacherId.Value
                     && x.StudentId == student.Id
                     && x.Status == BookingStatus.Confirmed,
                cancellationToken)
            || await dbContext.Charges.AnyAsync(
                x => x.TeacherId == teacherId.Value && x.StudentId == student.Id,
                cancellationToken);

        if (!isTeachersStudent)
            return Result<StudentOutstandingDto>.NotFound("الطالب غير موجود.");

        var rows = await LedgerChargeRows.SelectRows(
                dbContext.Charges
                    .AsNoTracking()
                    .Where(x =>
                        x.TeacherId == teacherId.Value
                        && x.StudentId == student.Id
                        && x.Status != ChargeStatus.Deferred
                        && x.Amount > x.AllocatedAmount)
                    .OrderBy(x => x.LessonId)
                    .ThenBy(x => x.CreatedAtUtc)
                    .ThenBy(x => x.Id))
            .ToListAsync(cancellationToken);

        var lessons = rows
            .GroupBy(x => new { x.LessonId, x.LessonTitle })
            .Select(g =>
            {
                var charges = g.Select(LedgerChargeRows.ToDto).ToList();
                return new StudentOutstandingLessonDto
                {
                    LessonId = g.Key.LessonId,
                    LessonTitle = g.Key.LessonTitle,
                    Remaining = charges.Sum(c => c.Remaining),
                    Charges = charges
                };
            })
            .ToList();

        return Result<StudentOutstandingDto>.Success(new StudentOutstandingDto
        {
            StudentId = student.Id,
            StudentName = student.Name,
            StudentCode = student.StudentCode,
            Remaining = lessons.Sum(x => x.Remaining),
            Lessons = lessons
        });
    }
}
