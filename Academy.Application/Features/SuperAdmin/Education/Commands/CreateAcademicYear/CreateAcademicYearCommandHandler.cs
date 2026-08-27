using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using Academy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateAcademicYear;

public sealed class CreateAcademicYearCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateAcademicYearCommand, Result<AcademicYearDto>>
{
    public async Task<Result<AcademicYearDto>> Handle(
        CreateAcademicYearCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var exists = await dbContext.AcademicYears
            .AnyAsync(x => x.Name == name, cancellationToken);

        if (exists)
            return Result<AcademicYearDto>.Conflict("Academic year already exists.");

        var entity = new AcademicYear
        {
            Name = name,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.AcademicYears.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AcademicYearDto>.Success(EducationMappings.ToAcademicYearDto(entity, lessonsCount: 0));
    }
}
