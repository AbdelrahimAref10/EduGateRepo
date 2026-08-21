namespace Academy.Application.Features.Account.Dtos;

public sealed class UpdatePreferredLanguageRequest
{
    /// <summary>
    /// 1 = Arabic, 2 = English.
    /// </summary>
    public int LanguageId { get; set; }
}
