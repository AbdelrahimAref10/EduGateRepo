using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Billing.Queries.GetAdminLessonDebts;

public sealed record GetAdminLessonDebtsQuery(int? LessonId)
    : IRequest<Result<IReadOnlyList<AdminDebtRowDto>>>;

public sealed class AdminDebtRowDto
{
    public required int LessonId { get; init; }

    public required string Subject { get; init; }

    public required string TeacherName { get; init; }

    public required int StudentId { get; init; }

    public required string StudentName { get; init; }

    public string? StudentCode { get; init; }

    public string? PhotoUrl { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required int OpenChargesCount { get; init; }
}

public sealed class GetAdminLessonDebtsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAdminLessonDebtsQuery, Result<IReadOnlyList<AdminDebtRowDto>>>
{
    public async Task<Result<IReadOnlyList<AdminDebtRowDto>>> Handle(
        GetAdminLessonDebtsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Charges
            .AsNoTracking()
            .Where(x =>
                x.Status != ChargeStatus.Deferred
                && x.Allocations.Sum(a => a.Amount) < x.Amount);

        if (request.LessonId is int lessonId)
            query = query.Where(x => x.LessonId == lessonId);

        var raw = await query
            .GroupBy(x => new
            {
                x.LessonId,
                Subject = x.Lesson.Subject,
                TeacherName = (x.Lesson.Teacher.User.FirstName + " " + x.Lesson.Teacher.User.LastName).Trim(),
                x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                x.Student.StudentCode,
                Photo = x.Student.User.ProfilePhoto
            })
            .Select(g => new
            {
                g.Key.LessonId,
                g.Key.Subject,
                g.Key.TeacherName,
                g.Key.StudentId,
                g.Key.StudentName,
                g.Key.StudentCode,
                g.Key.Photo,
                OutstandingAmount = g.Sum(c => c.Amount - c.Allocations.Sum(a => a.Amount)),
                OpenChargesCount = g.Count()
            })
            .Where(x => x.OutstandingAmount > 0)
            .OrderByDescending(x => x.OutstandingAmount)
            .ToListAsync(cancellationToken);

        var rows = raw.Select(x => new AdminDebtRowDto
        {
            LessonId = x.LessonId,
            Subject = x.Subject,
            TeacherName = x.TeacherName,
            StudentId = x.StudentId,
            StudentName = x.StudentName,
            StudentCode = x.StudentCode,
            PhotoUrl = ImageService.DisplayValue(x.Photo),
            OutstandingAmount = x.OutstandingAmount,
            OpenChargesCount = x.OpenChargesCount
        }).ToList();

        return Result<IReadOnlyList<AdminDebtRowDto>>.Success(rows);
    }
}
