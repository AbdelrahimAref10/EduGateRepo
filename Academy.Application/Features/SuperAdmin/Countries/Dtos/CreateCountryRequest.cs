namespace Academy.Application.Features.SuperAdmin.Countries.Dtos;

public sealed class CreateCountryRequest
{
    public string NameAr { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string Code { get; set; } = null!;
}
