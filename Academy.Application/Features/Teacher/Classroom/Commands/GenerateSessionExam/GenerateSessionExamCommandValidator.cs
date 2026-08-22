using Academy.Application.Features.Classroom.Exams;
using FluentValidation;

namespace Academy.Application.Features.Teacher.Classroom.Commands.GenerateSessionExam;

public sealed class GenerateSessionExamCommandValidator : AbstractValidator<GenerateSessionExamCommand>
{
    public GenerateSessionExamCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(ExamRules.MinQuestionCount, ExamRules.MaxQuestionCount);

        RuleFor(x => x.MinutesPerQuestion)
            .InclusiveBetween(ExamRules.MinMinutesPerQuestion, ExamRules.MaxMinutesPerQuestion)
            .WithMessage($"حدد وقت كل سؤال من {ExamRules.MinMinutesPerQuestion} إلى {ExamRules.MaxMinutesPerQuestion} دقيقة.");

        RuleFor(x => x.Files)
            .NotEmpty()
            .WithMessage("ارفع ملف PDF أو Word أو صورة لتوليد الامتحان.");

        RuleFor(x => x.Files)
            .Must(files => files.Count <= ExamRules.MaxFileCount)
            .WithMessage($"يمكنك رفع {ExamRules.MaxFileCount} ملفات كحد أقصى.");

        RuleForEach(x => x.Files).ChildRules(file =>
        {
            file.RuleFor(x => x.FileName)
                .Must(ExamRules.IsAllowedFileName)
                .WithMessage("الصيغ المسموحة: PDF و Word و JPG و PNG و WEBP.");

            file.RuleFor(x => x.Content)
                .Must(content => content.Length > 0 && content.Length <= ExamRules.MaxFileBytes)
                .WithMessage("كل ملف يجب ألا يتجاوز 12 ميجابايت.");
        });
    }
}
