using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationYear;

public sealed record CreateEducationYearCommand(
    int EducationTypeId,
    string NameAr,
    string NameEn,
    int SortOrder) : IRequest<Result<EducationYearDto>>;
