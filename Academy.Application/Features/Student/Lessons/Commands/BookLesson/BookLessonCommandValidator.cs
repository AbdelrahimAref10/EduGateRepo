using FluentValidation;

namespace Academy.Application.Features.Student.Lessons.Commands.BookLesson;

public sealed class BookLessonCommandValidator : AbstractValidator<BookLessonCommand>
{
    public BookLessonCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.LessonId).GreaterThan(0);
    }
}
