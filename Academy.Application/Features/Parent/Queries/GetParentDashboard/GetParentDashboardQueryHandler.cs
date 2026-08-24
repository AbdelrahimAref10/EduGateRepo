using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Parent.Common;
using Academy.Application.Features.Parent.Dtos;
using Academy.Application.Common.Images;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Parent.Queries.GetParentDashboard;

public sealed record GetParentDashboardQuery(int UserId) : IRequest<Result<ParentDashboardDto>>;

public sealed class GetParentDashboardQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetParentDashboardQuery, Result<ParentDashboardDto>>
{
    public async Task<Result<ParentDashboardDto>> Handle(
        GetParentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var parentStudentId = await ParentAccess.GetParentStudentIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (parentStudentId is null)
            return Result<ParentDashboardDto>.NotFound("Parent profile was not found.");

        var children = await dbContext.ParentChildLinks
            .AsNoTracking()
            .Where(x => x.ParentStudentId == parentStudentId.Value)
            .OrderByDescending(x => x.LinkedAtUtc)
            .Select(x => new
            {
                x.ChildStudentId,
                FullName = x.ChildStudent.User.FullName,
                x.ChildStudent.StudentCode,
                Photo = x.ChildStudent.User.ProfilePhoto,
                x.LinkedAtUtc
            })
            .ToListAsync(cancellationToken);

        var childDtos = children
            .Select(x => new ParentChildDto
            {
                ChildStudentId = x.ChildStudentId,
                FullName = x.FullName,
                StudentCode = x.StudentCode,
                PhotoUrl = ImageService.DisplayValue(x.Photo),
                LinkedAtUtc = x.LinkedAtUtc
            })
            .ToList();

        var childIds = children.Select(x => x.ChildStudentId).ToList();
        if (childIds.Count == 0)
        {
            return Result<ParentDashboardDto>.Success(new ParentDashboardDto
            {
                Children = childDtos,
                UpcomingSessions = [],
                UnpaidCharges = []
            });
        }

        var nameByChild = children.ToDictionary(x => x.ChildStudentId, x => x.FullName);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var memberships = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(m => childIds.Contains(m.StudentId) && m.LessonGroup.EndedAtUtc == null)
            .Select(m => new { m.StudentId, m.LessonGroupId })
            .ToListAsync(cancellationToken);

        var groupIds = memberships.Select(m => m.LessonGroupId).Distinct().ToList();
        var childIdsByGroup = memberships
            .GroupBy(m => m.LessonGroupId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.StudentId).Distinct().ToList());

        IReadOnlyList<ParentUpcomingSessionDto> upcoming = [];
        if (groupIds.Count > 0)
        {
            var sessions = await dbContext.LessonGroupSessions
                .AsNoTracking()
                .Where(s =>
                    groupIds.Contains(s.LessonGroupId)
                    && s.EndedAtUtc == null
                    && s.SessionDate >= today)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .Take(40)
                .Select(s => new
                {
                    s.Id,
                    s.LessonGroupId,
                    Subject = s.LessonGroup.Lesson.Subject,
                    GroupName = s.LessonGroup.Name,
                    TeacherFirst = s.LessonGroup.Lesson.Teacher.User.FirstName,
                    TeacherLast = s.LessonGroup.Lesson.Teacher.User.LastName,
                    s.SessionDate,
                    s.StartTime,
                    s.Topic,
                    HasStarted = s.StartedAtUtc != null
                })
                .ToListAsync(cancellationToken);

            upcoming = sessions
                .SelectMany(s =>
                {
                    if (!childIdsByGroup.TryGetValue(s.LessonGroupId, out var kids))
                        return Enumerable.Empty<ParentUpcomingSessionDto>();

                    return kids.Select(childId => new ParentUpcomingSessionDto
                    {
                        SessionId = s.Id,
                        ChildStudentId = childId,
                        ChildName = nameByChild.GetValueOrDefault(childId, ""),
                        Subject = s.Subject,
                        GroupName = s.GroupName,
                        TeacherName = $"{s.TeacherFirst} {s.TeacherLast}".Trim(),
                        SessionDate = s.SessionDate,
                        StartTime = s.StartTime,
                        Topic = s.Topic,
                        HasStarted = s.HasStarted
                    });
                })
                .OrderBy(x => x.SessionDate)
                .ThenBy(x => x.StartTime)
                .Take(20)
                .ToList();
        }

        var unpaid = await dbContext.Charges
            .AsNoTracking()
            .Where(c =>
                childIds.Contains(c.StudentId)
                && c.Status != ChargeStatus.Deferred
                && c.Status != ChargeStatus.Paid
                && c.Amount > c.AllocatedAmount)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(30)
            .Select(c => new ParentUnpaidChargeDto
            {
                ChargeId = c.Id,
                ChildStudentId = c.StudentId,
                ChildName = c.Student.User.FullName,
                Subject = c.Lesson.Subject,
                Type = c.Type.ToString(),
                Amount = c.Amount,
                Remaining = c.Amount - c.AllocatedAmount,
                Status = c.Status.ToString(),
                CreatedAtUtc = c.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<ParentDashboardDto>.Success(new ParentDashboardDto
        {
            Children = childDtos,
            UpcomingSessions = upcoming,
            UnpaidCharges = unpaid
        });
    }
}
