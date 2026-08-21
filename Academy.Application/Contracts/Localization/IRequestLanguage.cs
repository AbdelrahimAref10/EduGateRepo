using Academy.Domain.Enums;

namespace Academy.Application.Contracts.Localization;

public interface IRequestLanguage
{
    AppLanguage Current { get; }

    void Set(AppLanguage language);
}
