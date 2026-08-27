using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateAcademicYear;

public sealed class UpdateAcademicYearCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateAcademicYearCommand, Result<AcademicYearDto>>
{
    public async Task<Result<AcademicYearDto>> Handle(
        UpdateAcademicYearCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.AcademicYears
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<AcademicYearDto>.NotFound("Academic year was not found.");

        var name = request.Name.Trim();

        var nameTaken = await dbContext.AcademicYears
            .AnyAsync(x => x.Id != request.Id && x.Name == name, cancellationToken);

        if (nameTaken)
            return Result<AcademicYearDto>.Conflict("Academic year already exists.");

        entity.Name = name;
        entity.SortOrder = request.SortOrder;

        await dbContext.SaveChangesAsync(cancellationToken);

        var lessonsCount = await dbContext.Lessons
            .CountAsync(x => x.AcademicYearId == entity.Id, cancellationToken);

        return Result<AcademicYearDto>.Success(EducationMappings.ToAcademicYearDto(entity, lessonsCount));
    }
}
