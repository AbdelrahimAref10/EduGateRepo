using Academy.Application.Contracts.Billing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Academy.Infrastructure.Billing;

public sealed class PaymentReceiptPdfGenerator : IPaymentReceiptPdfGenerator
{
    private static readonly string LogoPath = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "Logo2.png");

    static PaymentReceiptPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(PaymentReceiptPdfModel model)
    {
        // Single continuous thermal-style strip (~80mm) — never splits to page 2.
        const float pageWidth = 226f;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.ContinuousSize(pageWidth);
                page.MarginHorizontal(12);
                page.MarginVertical(10);
                page.DefaultTextStyle(x => x
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken4)
                    .LineHeight(1.15f));

                page.Content().Column(col =>
                {
                    col.Spacing(2);

                    if (File.Exists(LogoPath))
                    {
                        col.Item().AlignCenter().Width(64).Image(LogoPath);
                    }
                    else
                    {
                        col.Item().AlignCenter().Text("eduGate")
                            .Bold().FontSize(14).FontColor(Colors.Teal.Darken2);
                    }

                    col.Item().AlignCenter().Text("eduGate")
                        .SemiBold().FontSize(10).FontColor(Colors.Teal.Darken2);

                    col.Item().AlignCenter().Text("إيصال دفع")
                        .Bold().FontSize(12);

                    col.Item().AlignCenter().Text("PAYMENT RECEIPT")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);

                    Divider(col);

                    RowLine(col, "رقم الإيصال", $"#{model.ReceiptNumber}");
                    RowLine(col, "التاريخ", model.PaidAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));

                    Divider(col);

                    RowLine(col, "المعلم", model.TeacherName);
                    RowLine(col, "الطالب", model.StudentName);
                    if (!string.IsNullOrWhiteSpace(model.StudentCode))
                        RowLine(col, "كود الطالب", model.StudentCode!);
                    RowLine(col, "المادة", model.Subject);

                    Divider(col);

                    col.Item().AlignCenter().Text("المبلغ المدفوع")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);

                    col.Item().AlignCenter().Text($"{model.Amount:0.##}")
                        .Bold().FontSize(18).FontColor(Colors.Teal.Darken2);

                    col.Item().AlignCenter().Text(MethodLabel(model.Method))
                        .SemiBold().FontSize(9);

                    if (!string.IsNullOrWhiteSpace(model.Note))
                    {
                        col.Item().PaddingTop(2).Text($"ملاحظة: {model.Note}")
                            .FontSize(7).FontColor(Colors.Grey.Darken1);
                    }

                    if (model.Allocations.Count > 0)
                    {
                        Divider(col);
                        col.Item().Text("التوزيع / Allocated").Bold().FontSize(8);

                        foreach (var line in model.Allocations)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text(ChargeTypeLabel(line.ChargeType)).FontSize(8);
                                row.ConstantItem(48).AlignRight()
                                    .Text($"{line.Amount:0.##}").SemiBold().FontSize(8);
                            });
                        }
                    }

                    Divider(col);

                    col.Item().AlignCenter().Text("شكراً لتعاملكم").FontSize(8);
                    col.Item().AlignCenter().Text("Thank you")
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                    col.Item().AlignCenter().Text("— eduGate —")
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();
    }

    private static void Divider(ColumnDescriptor col)
    {
        col.Item().PaddingVertical(3).BorderBottom(0.75f).BorderColor(Colors.Grey.Lighten1);
    }

    private static void RowLine(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(58).Text(label).FontSize(7).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().AlignRight().Text(value).SemiBold().FontSize(8);
        });
    }

    private static string MethodLabel(string method) => method switch
    {
        "Cash" => "نقدي / Cash",
        "VodafoneCash" => "فودافون كاش / Vodafone Cash",
        "InstaPay" => "إنستاباي / InstaPay",
        _ => method
    };

    private static string ChargeTypeLabel(string type) => type switch
    {
        "Session" => "حصة",
        "MonthlyCycle" => "اشتراك شهري",
        "Makeup" => "تعويض",
        _ => type
    };
}
