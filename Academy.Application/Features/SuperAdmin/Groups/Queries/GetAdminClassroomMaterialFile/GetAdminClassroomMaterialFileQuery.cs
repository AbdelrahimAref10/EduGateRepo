using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminClassroomMaterialFile;

public sealed record GetAdminClassroomMaterialFileQuery(int SessionId, int MaterialId)
    : IRequest<Result<ClassroomFileDownloadDto>>;
