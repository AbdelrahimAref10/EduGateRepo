using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterLessons;

public sealed record GetTeacherBillingFilterLessonsQuery(
    int UserId,
    int AcademicYearId,
    int EducationStageId)
    : IRequest<Result<IReadOnlyList<LedgerFilterOptionDto>>>;

public sealed class GetTeacherBillingFilterLessonsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingFilterLessonsQuery, Result<IReadOnlyList<LedgerFilterOptionDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerFilterOptionDto>>> Handle(
        GetTeacherBillingFilterLessonsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LedgerFilterOptionDto>>.NotFound("Teacher profile was not found.");

        var items = await dbContext.Lessons
            .AsNoTracking()
            .Where(x =>
                x.TeacherId == teacherId.Value
                && x.AcademicYearId == request.AcademicYearId
                && x.EducationStageId == request.EducationStageId)
            .OrderBy(x => x.Subject)
            .ThenBy(x => x.Id)
            .Select(x => new LedgerFilterOptionDto
            {
                Id = x.Id,
                Name = x.Subject
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LedgerFilterOptionDto>>.Success(items);
    }
}
