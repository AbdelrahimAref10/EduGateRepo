using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingPayments;

public sealed record GetTeacherBillingPaymentsQuery(
    int UserId,
    int? StudentId = null,
    int? AcademicYearId = null,
    int? EducationStageId = null,
    int? LessonId = null,
    int? GroupId = null,
    int? SessionId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = LedgerPaging.PageSize)
    : IRequest<Result<PagedResult<LedgerPaymentRowDto>>>;

public sealed class GetTeacherBillingPaymentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingPaymentsQuery, Result<PagedResult<LedgerPaymentRowDto>>>
{
    public async Task<Result<PagedResult<LedgerPaymentRowDto>>> Handle(
        GetTeacherBillingPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = LedgerPaging.Normalize(request.Page, request.PageSize);

        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<PagedResult<LedgerPaymentRowDto>>.NotFound("Teacher profile was not found.");

        var (fromUtc, toUtc) = LedgerCalendar.Range(request.From, request.To);

        var query = dbContext.Payments
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .ApplyPaymentFilters(
                request.StudentId,
                request.LessonId,
                request.GroupId,
                fromUtc,
                toUtc,
                request.AcademicYearId,
                request.EducationStageId,
                request.SessionId);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<LedgerPaymentRowDto>>.Success(
                PagedResult<LedgerPaymentRowDto>.Empty(page, pageSize));

        var rows = await query
            .OrderByDescending(x => x.PaidAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.PaidAtUtc,
                x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                x.Student.StudentCode,
                x.LessonId,
                LessonTitle = x.Lesson.Subject,
                GroupId = (int?)null,
                GroupName = (string?)null,
                x.Amount,
                x.Method,
                x.ReceiptNumber,
                x.Note
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new LedgerPaymentRowDto
        {
            Id = x.Id,
            PaidAtUtc = x.PaidAtUtc,
            StudentId = x.StudentId,
            StudentName = x.StudentName,
            StudentCode = x.StudentCode,
            LessonId = x.LessonId,
            LessonTitle = x.LessonTitle,
            GroupId = x.GroupId,
            GroupName = x.GroupName,
            Amount = x.Amount,
            Method = x.Method.ToString(),
            ReceiptNumber = x.ReceiptNumber,
            Note = x.Note
        }).ToList();

        return Result<PagedResult<LedgerPaymentRowDto>>.Success(
            PagedResult<LedgerPaymentRowDto>.Create(items, totalCount, page, pageSize));
    }
}
