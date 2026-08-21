using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.CreateEducationType;

public sealed record CreateEducationTypeCommand(
    string NameAr,
    string NameEn,
    int SortOrder) : IRequest<Result<EducationTypeDto>>;
