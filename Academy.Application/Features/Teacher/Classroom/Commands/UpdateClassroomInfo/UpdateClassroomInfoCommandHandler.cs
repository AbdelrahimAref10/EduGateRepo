using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Classroom.Commands.UpdateClassroomInfo;

public sealed class UpdateClassroomInfoCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateClassroomInfoCommand, Result<TeacherClassroomDto>>
{
    public async Task<Result<TeacherClassroomDto>> Handle(
        UpdateClassroomInfoCommand request,
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
            cancellationToken,
            asTracking: true);

        if (session is null)
            return Result<TeacherClassroomDto>.NotFound("الحصة غير موجودة.");

        session.Topic = string.IsNullOrWhiteSpace(request.Topic) ? null : request.Topic.Trim();
        session.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await TeacherClassroomLoader.BuildDtoAsync(dbContext, session, cancellationToken);
        return Result<TeacherClassroomDto>.Success(dto);
    }
}
