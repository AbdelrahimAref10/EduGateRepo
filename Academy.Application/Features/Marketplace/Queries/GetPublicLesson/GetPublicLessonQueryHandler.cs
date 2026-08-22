using Academy.Application.Common.Models;
using Academy.Application.Contracts.Persistence;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicLesson;

public sealed class GetPublicLessonQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPublicLessonQuery, Result<PublicLessonDeepLinkDto>>
{
    public async Task<Result<PublicLessonDeepLinkDto>> Handle(
        GetPublicLessonQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.Id == request.LessonId && x.IsActive)
            .Select(x => new PublicLessonDeepLinkDto
            {
                LessonId = x.Id,
                TeacherId = x.TeacherId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson is null)
            return Result<PublicLessonDeepLinkDto>.NotFound("Lesson was not found.");

        return Result<PublicLessonDeepLinkDto>.Success(lesson);
    }
}
