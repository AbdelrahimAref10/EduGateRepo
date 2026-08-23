using Academy.Application.Common.Images;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.Billing.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.Billing.Queries.GetTeacherLessonBillingDetail;

public sealed record GetTeacherLessonBillingDetailQuery(int UserId, int LessonId)
    : IRequest<Result<LessonBillingDetailDto>>;

public sealed class LessonBillingDetailDto
{
    public required int LessonId { get; init; }

    public required string Subject { get; init; }

    public required string BillingType { get; init; }

    public decimal? SessionPrice { get; init; }

    public decimal? MonthlyPrice { get; init; }

    public required string EducationTypeName { get; init; }

    public required string EducationStageName { get; init; }

    public required string EducationYearName { get; init; }

    public required IReadOnlyList<GroupBillingDto> Groups { get; init; }
}

public sealed class GroupBillingDto
{
    public required int GroupId { get; init; }

    public required string Name { get; init; }

    public required int MembersCount { get; init; }

    public required decimal OutstandingAmount { get; init; }

    public required IReadOnlyList<LedgerStudentRowDto> Students { get; init; }
}

public sealed class GetTeacherLessonBillingDetailQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetTeacherLessonBillingDetailQuery, Result<LessonBillingDetailDto>>
{
    public async Task<Result<LessonBillingDetailDto>> Handle(
        GetTeacherLessonBillingDetailQuery request,
        CancellationToken cancellationToken)
    {
        var isArabic = requestLanguage.Current == AppLanguage.Arabic;

        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId && x.Teacher.UserId == request.UserId)
            .Select(x => new
            {
                x.Id,
                x.Subject,
                BillingType = x.BillingType.ToString(),
                x.SessionPrice,
                x.MonthlyPrice,
                EducationTypeName = isArabic ? x.EducationType.NameAr : x.EducationType.NameEn,
                EducationStageName = isArabic ? x.EducationStage.NameAr : x.EducationStage.NameEn,
                EducationYearName = isArabic ? x.EducationYear.NameAr : x.EducationYear.NameEn
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return Result<LessonBillingDetailDto>.NotFound("الدرس غير موجود.");

        var groups = await dbContext.LessonGroups
            .AsNoTracking()
            .Where(x => x.LessonId == lesson.Id)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, MembersCount = x.Members.Count })
            .ToListAsync(cancellationToken);

        if (groups.Count == 0)
        {
            return Result<LessonBillingDetailDto>.Success(new LessonBillingDetailDto
            {
                LessonId = lesson.Id,
                Subject = lesson.Subject,
                BillingType = lesson.BillingType,
                SessionPrice = lesson.SessionPrice,
                MonthlyPrice = lesson.MonthlyPrice,
                EducationTypeName = lesson.EducationTypeName,
                EducationStageName = lesson.EducationStageName,
                EducationYearName = lesson.EducationYearName,
                Groups = []
            });
        }

        var groupIds = groups.Select(g => g.Id).ToList();

        var members = await dbContext.LessonGroupMembers
            .AsNoTracking()
            .Where(x => groupIds.Contains(x.LessonGroupId))
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => new
            {
                x.LessonGroupId,
                x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                x.Student.StudentCode,
                Photo = x.Student.User.ProfilePhoto
            })
            .ToListAsync(cancellationToken);

        var studentIds = members.Select(m => m.StudentId).Distinct().ToList();

        Dictionary<int, (decimal Outstanding, int OpenCount)> chargeMap;
        Dictionary<int, (DateTime LastPaidAt, decimal LastAmount)> payMap;

        if (studentIds.Count == 0)
        {
            chargeMap = new Dictionary<int, (decimal, int)>();
            payMap = new Dictionary<int, (DateTime, decimal)>();
        }
        else
        {
            var chargeAgg = await dbContext.Charges
                .AsNoTracking()
                .Where(x =>
                    x.LessonId == lesson.Id
                    && studentIds.Contains(x.StudentId)
                    && x.Status != ChargeStatus.Deferred)
                .GroupBy(x => x.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                Outstanding = g.Sum(c => c.Amount - c.Allocations.Sum(a => a.Amount)),
                OpenCount = g.Count(c =>
                    c.Status != ChargeStatus.Paid
                    && c.Allocations.Sum(a => a.Amount) < c.Amount)
                })
                .ToListAsync(cancellationToken);

            var paymentRows = await dbContext.Payments
                .AsNoTracking()
                .Where(x => x.LessonId == lesson.Id && studentIds.Contains(x.StudentId))
                .Select(x => new { x.StudentId, x.PaidAtUtc, x.Amount })
                .ToListAsync(cancellationToken);

            chargeMap = chargeAgg.ToDictionary(
                x => x.StudentId,
                x => (x.Outstanding, x.OpenCount));
            payMap = paymentRows
                .GroupBy(x => x.StudentId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var last = g.OrderByDescending(p => p.PaidAtUtc).First();
                        return (last.PaidAtUtc, last.Amount);
                    });
        }

        var groupDtos = groups.Select(g =>
        {
            var rows = members
                .Where(m => m.LessonGroupId == g.Id)
                .Select(m =>
                {
                    chargeMap.TryGetValue(m.StudentId, out var c);
                    payMap.TryGetValue(m.StudentId, out var p);
                    return new LedgerStudentRowDto
                    {
                        StudentId = m.StudentId,
                        StudentName = m.StudentName,
                        StudentCode = m.StudentCode,
                        PhotoUrl = ImageService.DisplayValue(m.Photo),
                        OutstandingAmount = c.Outstanding,
                        OpenChargesCount = c.OpenCount,
                        LastPaymentAtUtc = p.LastPaidAt == default ? null : p.LastPaidAt,
                        LastPaymentAmount = p.LastPaidAt == default ? null : p.LastAmount
                    };
                })
                .ToList();

            return new GroupBillingDto
            {
                GroupId = g.Id,
                Name = g.Name,
                MembersCount = g.MembersCount,
                OutstandingAmount = rows.Sum(r => r.OutstandingAmount),
                Students = rows
            };
        }).ToList();

        return Result<LessonBillingDetailDto>.Success(new LessonBillingDetailDto
        {
            LessonId = lesson.Id,
            Subject = lesson.Subject,
            BillingType = lesson.BillingType,
            SessionPrice = lesson.SessionPrice,
            MonthlyPrice = lesson.MonthlyPrice,
            EducationTypeName = lesson.EducationTypeName,
            EducationStageName = lesson.EducationStageName,
            EducationYearName = lesson.EducationYearName,
            Groups = groupDtos
        });
    }
}
