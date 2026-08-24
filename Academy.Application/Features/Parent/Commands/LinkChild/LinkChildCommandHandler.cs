using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Parent.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentEntity = Academy.Domain.Entities.Student;

namespace Academy.Application.Features.Parent.Commands.LinkChild;

public sealed record LinkChildCommand(int UserId, string StudentCode)
    : IRequest<Result<ParentChildDto>>;

public sealed class LinkChildCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<LinkChildCommand, Result<ParentChildDto>>
{
    public async Task<Result<ParentChildDto>> Handle(
        LinkChildCommand request,
        CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<ParentChildDto>.NotFound("Parent profile was not found.");

        var code = request.StudentCode.Trim();
        var child = await ResolveChildByCodeAsync(code, cancellationToken);
        if (child is null)
            return Result<ParentChildDto>.NotFound("No student was found with that code.");

        if (child.Id == parentStudentId.Value)
            return Result<ParentChildDto>.Failure("You cannot link your own profile as a child.");

        var alreadyLinked = await ParentAccess.IsLinkedAsync(
            dbContext, parentStudentId.Value, child.Id, cancellationToken);

        if (alreadyLinked)
            return Result<ParentChildDto>.Conflict("This child is already linked.");

        var link = new Academy.Domain.Entities.ParentChildLink
        {
            ParentStudentId = parentStudentId.Value,
            ChildStudentId = child.Id,
            LinkedAtUtc = DateTime.UtcNow
        };

        dbContext.ParentChildLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<ParentChildDto>.Success(new ParentChildDto
        {
            ChildStudentId = child.Id,
            FullName = child.User.FullName,
            StudentCode = child.StudentCode,
            PhotoUrl = ImageService.DisplayValue(child.User.ProfilePhoto),
            LinkedAtUtc = link.LinkedAtUtc
        });
    }

    private async Task<StudentEntity?> ResolveChildByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await dbContext.Students
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.StudentCode != null && x.StudentCode!.ToUpper() == code.ToUpperInvariant() && !x.IsParent,
                cancellationToken);

    }
}
