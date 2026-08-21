using Academy.Application.Contracts.Localization;
using Academy.Domain.Enums;

namespace Academy.Application.Common.Localization;

public sealed class RequestLanguage : IRequestLanguage
{
    public AppLanguage Current { get; private set; } = AppLanguage.Arabic;

    public void Set(AppLanguage language) => Current = language;
}
