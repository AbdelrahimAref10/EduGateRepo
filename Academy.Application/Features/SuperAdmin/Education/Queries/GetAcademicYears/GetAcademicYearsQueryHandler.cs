using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Queries.GetAcademicYears;

public sealed class GetAcademicYearsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAcademicYearsQuery, Result<IReadOnlyList<AcademicYearDto>>>
{
    public async Task<Result<IReadOnlyList<AcademicYearDto>>> Handle(
        GetAcademicYearsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<AcademicYear> query = dbContext.AcademicYears;

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var items = await query
            .OrderByDescending(x => x.SortOrder)
            .ThenByDescending(x => x.Name)
            .Select(x => new AcademicYearDto
            {
                Id = x.Id,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                LessonsCount = x.Lessons.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AcademicYearDto>>.Success(items);
    }
}
