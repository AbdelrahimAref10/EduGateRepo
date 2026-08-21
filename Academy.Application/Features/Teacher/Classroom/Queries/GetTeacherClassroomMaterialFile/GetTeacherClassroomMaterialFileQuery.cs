using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroomMaterialFile;

public sealed record GetTeacherClassroomMaterialFileQuery(
    int UserId,
    int SessionId,
    int MaterialId) : IRequest<Result<ClassroomFileDownloadDto>>;
