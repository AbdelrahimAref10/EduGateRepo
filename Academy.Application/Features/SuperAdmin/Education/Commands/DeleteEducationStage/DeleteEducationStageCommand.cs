using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationStage;

public sealed record DeleteEducationStageCommand(int EducationTypeId, int StageId) : IRequest<Result>;
