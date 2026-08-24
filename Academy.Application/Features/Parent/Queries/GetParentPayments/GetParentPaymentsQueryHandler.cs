using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Parent.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentPayments;

public sealed record GetParentPaymentsQuery(
    int UserId,
    int? ChildStudentId = null,
    int? Page = null,
    int? PageSize = null) : IRequest<Result<PagedResult<ParentPaymentItemDto>>>;

public sealed class GetParentPaymentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentPaymentsQuery, Result<PagedResult<ParentPaymentItemDto>>>
{
    public async Task<Result<PagedResult<ParentPaymentItemDto>>> Handle(
        GetParentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = Paging.Normalize(request.Page, request.PageSize);

        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<PagedResult<ParentPaymentItemDto>>.NotFound("Parent profile was not found.");

        var linkedIds = await ParentAccess.GetLinkedChildStudentIdsAsync(
            dbContext, parentStudentId.Value, cancellationToken);

        if (linkedIds.Count == 0)
            return Result<PagedResult<ParentPaymentItemDto>>.Success(
                PagedResult<ParentPaymentItemDto>.Empty(page, pageSize));

        IReadOnlyList<int> childIds = linkedIds;
        if (request.ChildStudentId is > 0)
        {
            if (!linkedIds.Contains(request.ChildStudentId.Value))
                return Result<PagedResult<ParentPaymentItemDto>>.Failure(
                    "This child is not linked to your account.", 403);

            childIds = [request.ChildStudentId.Value];
        }

        var baseQuery = dbContext.Payments
            .AsNoTracking()
            .Where(p => childIds.Contains(p.StudentId));

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<ParentPaymentItemDto>>.Success(
                PagedResult<ParentPaymentItemDto>.Empty(page, pageSize));

        var items = await baseQuery
            .OrderByDescending(p => p.PaidAtUtc)
            .ThenByDescending(p => p.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new ParentPaymentItemDto
            {
                PaymentId = p.Id,
                ChildStudentId = p.StudentId,
                ChildName = p.Student.User.FullName,
                Subject = p.Lesson.Subject,
                Amount = p.Amount,
                Method = p.Method.ToString(),
                ReceiptNumber = p.ReceiptNumber,
                PaidAtUtc = p.PaidAtUtc,
                Note = p.Note
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ParentPaymentItemDto>>.Success(
            PagedResult<ParentPaymentItemDto>.Create(items, totalCount, page, pageSize));
    }
}
