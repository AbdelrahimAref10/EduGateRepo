using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Education.Commands.DeleteEducationType;

public sealed record DeleteEducationTypeCommand(int Id) : IRequest<Result>;
