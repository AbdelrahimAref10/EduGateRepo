using Academy.Application.Common.Models;
using Academy.Application.Contracts.Localization;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Teacher.StudentBookings.Dtos;
using Academy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Teacher.StudentBookings.Queries.GetPendingBookings;

public sealed class GetPendingBookingsQueryHandler(
    IApplicationDbContext dbContext,
    IRequestLanguage requestLanguage)
    : IRequestHandler<GetPendingBookingsQuery, Result<IReadOnlyList<TeacherBookingDto>>>
{
    public async Task<Result<IReadOnlyList<TeacherBookingDto>>> Handle(
        GetPendingBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (teacher is null)
            return Result<IReadOnlyList<TeacherBookingDto>>.NotFound("Teacher profile was not found.");

        var language = requestLanguage.Current;

        var items = await dbContext.LessonBookings
            .Where(x => x.TeacherId == teacher.Id && x.Status == BookingStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new TeacherBookingDto
            {
                Id = x.Id,
                LessonId = x.LessonId,
                TeacherId = x.TeacherId,
                StudentId = x.StudentId,
                StudentName = (x.Student.User.FirstName + " " + x.Student.User.LastName).Trim(),
                StudentPhotoUrl = x.Student.User.ProfilePhoto,
                StudentCode = x.Student.StudentCode,
                Subject = x.Lesson.Subject,
                AcademicYearName = x.Lesson.AcademicYear.Name,
                EducationStageName = language == AppLanguage.Arabic
                    ? x.Lesson.EducationStage.NameAr
                    : x.Lesson.EducationStage.NameEn,
                EducationYearName = language == AppLanguage.Arabic
                    ? x.Lesson.EducationYear.NameAr
                    : x.Lesson.EducationYear.NameEn,
                StartDate = x.Lesson.StartDate,
                Status = x.Status.ToString(),
                CreatedAtUtc = x.CreatedAtUtc,
                ReviewedAtUtc = x.ReviewedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TeacherBookingDto>>.Success(items);
    }
}
