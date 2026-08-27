using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingFilterGroups;

public sealed record GetTeacherBillingFilterGroupsQuery(int UserId, int LessonId)
    : IRequest<Result<IReadOnlyList<LedgerFilterOptionDto>>>;

public sealed class GetTeacherBillingFilterGroupsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingFilterGroupsQuery, Result<IReadOnlyList<LedgerFilterOptionDto>>>
{
    public async Task<Result<IReadOnlyList<LedgerFilterOptionDto>>> Handle(
        GetTeacherBillingFilterGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<LedgerFilterOptionDto>>.NotFound("Teacher profile was not found.");

        var lessonOk = await dbContext.Lessons.AnyAsync(
            x => x.Id == request.LessonId && x.TeacherId == teacherId.Value,
            cancellationToken);

        if (!lessonOk)
            return Result<IReadOnlyList<LedgerFilterOptionDto>>.NotFound("الدرس غير موجود.");

        var items = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x => x.LessonId == request.LessonId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new LedgerFilterOptionDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LedgerFilterOptionDto>>.Success(items);
    }
}
