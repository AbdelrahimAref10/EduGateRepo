using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Parent.Dtos;
using Academy.Application.Common.Images;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetMyChildren;

public sealed record GetMyChildrenQuery(int UserId) : IRequest<Result<IReadOnlyList<ParentChildDto>>>;

public sealed class GetMyChildrenQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyChildrenQuery, Result<IReadOnlyList<ParentChildDto>>>
{
    public async Task<Result<IReadOnlyList<ParentChildDto>>> Handle(
        GetMyChildrenQuery request,
        CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<IReadOnlyList<ParentChildDto>>.NotFound("Parent profile was not found.");

        var rows = await dbContext.ParentChildLinks
            .AsNoTracking()
            .Where(x => x.ParentStudentId == parentStudentId.Value)
            .OrderByDescending(x => x.LinkedAtUtc)
            .Select(x => new
            {
                x.ChildStudentId,
                FullName = x.ChildStudent.User.FullName,
                x.ChildStudent.StudentCode,
                Photo = x.ChildStudent.User.ProfilePhoto,
                x.LinkedAtUtc
            })
            .ToListAsync(cancellationToken);

        IReadOnlyList<ParentChildDto> children = rows
            .Select(x => new ParentChildDto
            {
                ChildStudentId = x.ChildStudentId,
                FullName = x.FullName,
                StudentCode = x.StudentCode,
                PhotoUrl = ImageService.DisplayValue(x.Photo),
                LinkedAtUtc = x.LinkedAtUtc
            })
            .ToList();

        return Result<IReadOnlyList<ParentChildDto>>.Success(children);
    }
}
