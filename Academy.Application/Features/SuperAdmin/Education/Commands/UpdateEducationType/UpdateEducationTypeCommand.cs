using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationType;

public sealed record UpdateEducationTypeCommand(
    int Id,
    string NameAr,
    string NameEn,
    int SortOrder) : IRequest<Result<EducationTypeDto>>;
