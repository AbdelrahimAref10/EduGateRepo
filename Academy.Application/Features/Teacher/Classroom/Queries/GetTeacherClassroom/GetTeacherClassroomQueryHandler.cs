using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherClassroom;

public sealed class GetTeacherClassroomQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherClassroomQuery, Result<TeacherClassroomDto>>
{
    public async Task<Result<TeacherClassroomDto>> Handle(
        GetTeacherClassroomQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<TeacherClassroomDto>.NotFound("Teacher profile was not found.");

        var session = await TeacherClassroomLoader.LoadOwnedSessionAsync(
            dbContext,
            teacher.Id,
            request.SessionId,
            cancellationToken);

        if (session is null)
            return Result<TeacherClassroomDto>.NotFound("الحصة غير موجودة.");

        var dto = await TeacherClassroomLoader.BuildDtoAsync(dbContext, session, cancellationToken);
        return Result<TeacherClassroomDto>.Success(dto);
    }
}
