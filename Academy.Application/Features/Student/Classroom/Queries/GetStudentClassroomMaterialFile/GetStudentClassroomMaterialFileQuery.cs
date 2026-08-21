using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Student.Classroom.Queries.GetStudentClassroomMaterialFile;

public sealed record GetStudentClassroomMaterialFileQuery(
    int UserId,
    int SessionId,
    int MaterialId) : IRequest<Result<ClassroomFileDownloadDto>>;
