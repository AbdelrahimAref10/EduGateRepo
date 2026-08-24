using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Parent.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentAttendance;

public sealed record GetParentAttendanceQuery(
    int UserId,
    int? ChildStudentId = null,
    int? Page = null,
    int? PageSize = null) : IRequest<Result<PagedResult<ParentAttendanceItemDto>>>;

public sealed class GetParentAttendanceQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentAttendanceQuery, Result<PagedResult<ParentAttendanceItemDto>>>
{
    public async Task<Result<PagedResult<ParentAttendanceItemDto>>> Handle(
        GetParentAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize, skip) = Paging.Normalize(request.Page, request.PageSize);

        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<PagedResult<ParentAttendanceItemDto>>.NotFound("Parent profile was not found.");

        var linkedIds = await ParentAccess.GetLinkedChildStudentIdsAsync(
            dbContext, parentStudentId.Value, cancellationToken);

        if (linkedIds.Count == 0)
            return Result<PagedResult<ParentAttendanceItemDto>>.Success(
                PagedResult<ParentAttendanceItemDto>.Empty(page, pageSize));

        IReadOnlyList<int> childIds = linkedIds;
        if (request.ChildStudentId is > 0)
        {
            if (!linkedIds.Contains(request.ChildStudentId.Value))
                return Result<PagedResult<ParentAttendanceItemDto>>.Failure(
                    "This child is not linked to your account.", 403);

            childIds = [request.ChildStudentId.Value];
        }

        var baseQuery = dbContext.LessonSessionStudentDetails
            .AsNoTracking()
            .Where(d => childIds.Contains(d.StudentId)
                        && d.LessonGroupSession.StartedAtUtc != null);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        if (totalCount == 0)
            return Result<PagedResult<ParentAttendanceItemDto>>.Success(
                PagedResult<ParentAttendanceItemDto>.Empty(page, pageSize));

        var items = await baseQuery
            .OrderByDescending(d => d.LessonGroupSession.SessionDate)
            .ThenByDescending(d => d.LessonGroupSession.StartTime)
            .Skip(skip)
            .Take(pageSize)
            .Select(d => new ParentAttendanceItemDto
            {
                SessionId = d.LessonGroupSessionId,
                ChildStudentId = d.StudentId,
                ChildName = d.Student.User.FullName,
                Subject = d.LessonGroupSession.LessonGroup.Lesson.Subject,
                GroupName = d.LessonGroupSession.LessonGroup.Name,
                SessionDate = d.LessonGroupSession.SessionDate,
                StartTime = d.LessonGroupSession.StartTime,
                IsPresent = d.IsPresent,
                TeacherNotes = d.TeacherNotes
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ParentAttendanceItemDto>>.Success(
            PagedResult<ParentAttendanceItemDto>.Create(items, totalCount, page, pageSize));
    }
}
