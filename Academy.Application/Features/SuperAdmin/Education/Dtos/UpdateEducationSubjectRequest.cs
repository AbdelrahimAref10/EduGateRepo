namespace Academy.Application.Features.SuperAdmin.Education.Dtos;

public sealed class UpdateEducationSubjectRequest
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public int SortOrder { get; set; }
}
