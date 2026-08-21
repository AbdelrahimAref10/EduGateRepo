using Academy.Domain.Enums;

namespace Academy.Application.Common.Localization;

public static class LocalizedNames
{
    public static string Pick(string nameAr, string nameEn, AppLanguage language)
        => language == AppLanguage.Arabic ? nameAr : nameEn;

    public static string? PickOptional(string? nameAr, string? nameEn, AppLanguage language)
    {
        if (nameAr is null && nameEn is null)
            return null;

        return language == AppLanguage.Arabic
            ? nameAr ?? nameEn
            : nameEn ?? nameAr;
    }

    public static bool TryParse(int languageId, out AppLanguage language)
    {
        if (Enum.IsDefined(typeof(AppLanguage), languageId))
        {
            language = (AppLanguage)languageId;
            return true;
        }

        language = AppLanguage.Arabic;
        return false;
    }
}
