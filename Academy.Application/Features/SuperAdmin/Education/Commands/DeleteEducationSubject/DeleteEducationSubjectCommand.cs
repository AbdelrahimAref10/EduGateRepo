using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationSubject;

public sealed record DeleteEducationSubjectCommand(
    int EducationTypeId,
    int EducationStageId,
    int EducationYearId,
    int SubjectId) : IRequest<Result>;
