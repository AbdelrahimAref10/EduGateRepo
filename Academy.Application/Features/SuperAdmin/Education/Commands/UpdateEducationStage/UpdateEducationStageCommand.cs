using Academy.Application.Common.Models;
using Academy.Application.Features.SuperAdmin.Education.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.UpdateEducationStage;

public sealed record UpdateEducationStageCommand(int Id, string NameAr, string NameEn, int SortOrder)
    : IRequest<Result<EducationStageDto>>;
