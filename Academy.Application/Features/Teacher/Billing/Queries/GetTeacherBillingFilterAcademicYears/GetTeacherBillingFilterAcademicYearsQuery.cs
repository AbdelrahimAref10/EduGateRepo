using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterAcademicYears;

public sealed record GetTeacherBillingFilterAcademicYearsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<LedgerFilterOptionDto>>>;

public sealed class GetTeacherBillingFilterAcademicYearsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingFilterAcademicYearsQuery, Result<IReadOnlyList<LedgerFilterOptionDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerFilterOptionDto>>> Handle(
        GetTeacherBillingFilterAcademicYearsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LedgerFilterOptionDto>>.NotFound("Teacher profile was not found.");

        var items = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value)
            .Select(x => x.AcademicYear)
            .Distinct()
            .OrderByDescending(x => x.SortOrder)
            .ThenByDescending(x => x.Name)
            .Select(x => new LedgerFilterOptionDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LedgerFilterOptionDto>>.Success(items);
    }
}
