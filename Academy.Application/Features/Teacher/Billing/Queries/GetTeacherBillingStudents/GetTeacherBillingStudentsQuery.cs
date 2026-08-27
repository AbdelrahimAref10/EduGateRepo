using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Common;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherBillingStudents;

public sealed record GetTeacherBillingStudentsQuery(int UserId, string? Search)
    : IRequest<Result<IReadOnlyList<BillingStudentSearchDto>>>;

public sealed class GetTeacherBillingStudentsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeacherBillingStudentsQuery, Result<IReadOnlyList<BillingStudentSearchDto>>>
{
    public async Task<Result<IReadOnlyList<BillingStudentSearchDto>>> Handle(
        GetTeacherBillingStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var teacherId = await TeacherBillingAccess.GetTeacherIdAsync(
            dbContext, request.UserId, cancellationToken);

        if (teacherId is null)
            return Result<IReadOnlyList<BillingStudentSearchDto>>.NotFound("Teacher profile was not found.");

        var studentIdsQuery = dbContext.LessonBookings
            .AsNoTracking()
            .Where(x => x.TeacherId == teacherId.Value && x.Status == BookingStatus.Confirmed)
            .Select(x => x.StudentId)
            .Distinct();

        var studentsQuery = dbContext.Students
            .AsNoTracking()
            .Where(s => studentIdsQuery.Contains(s.Id) && !s.IsParent);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            studentsQuery = studentsQuery.Where(s =>
                s.User.FirstName.Contains(term)
                || s.User.LastName.Contains(term)
                || (s.User.FirstName + " " + s.User.LastName).Contains(term)
                || (s.User.PhoneNumber != null && s.User.PhoneNumber.Contains(term))
                || (s.StudentCode != null && s.StudentCode.Contains(term)));
        }

        var rows = await studentsQuery
            .OrderBy(s => s.User.FirstName)
            .ThenBy(s => s.User.LastName)
            .Take(LedgerPaging.PageSize)
            .Select(s => new
            {
                s.Id,
                FullName = (s.User.FirstName + " " + s.User.LastName).Trim(),
                s.StudentCode,
                s.User.PhoneNumber,
                Photo = s.User.ProfilePhoto
            })
            .ToListAsync(cancellationToken);

        IReadOnlyList<BillingStudentSearchDto> items = rows.Select(s => new BillingStudentSearchDto
        {
            Id = s.Id,
            FullName = s.FullName,
            StudentCode = s.StudentCode,
            PhoneNumber = s.PhoneNumber,
            PhotoUrl = ImageService.DisplayValue(s.Photo)
        }).ToList();

        return Result<IReadOnlyList<BillingStudentSearchDto>>.Success(items);
    }
}
