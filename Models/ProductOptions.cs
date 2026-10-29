using System.Globalization;

namespace StokTakipPro.Models;

public static class ProductOptions
{
    public static readonly IReadOnlyList<string> Categories =
    [
        "Tişört", "Gömlek", "Pantolon", "Kot Pantolon", "Ceket", "Mont", "Kaban", "Trençkot", "Kazak", "Hırka",
        "Sweatshirt", "Elbise", "Etek", "Şort", "Eşofman Altı", "Bluz", "Tunik", "Yelek", "Takım Elbise", "Tulum"
    ];

    public static readonly IReadOnlyList<string> Sizes =
    [
        "XS", "S", "M", "L", "XL", "XXL", "3XL", "4XL", "34", "36", "38", "40", "42", "44", "46", "48", "50", "52",
        "Standart"
    ];

    public static readonly IReadOnlyList<string> FabricTypes =
    [
        "Pamuk", "Polyester", "Viskon", "Keten", "Yün", "Denim", "Saten", "Poplin", "Süprem", "Kaşkorse", "Penye",
        "Polar", "Krep", "Şifon", "Kadife", "Gabardin", "İpek", "Naylon", "Akrilik", "Elastan Karışımlı"
    ];

    public static readonly IReadOnlyList<string> Units = ["Metre", "Kg", "Adet", "Top", "Rulo", "Takım", "Çift"];

    public static readonly IReadOnlyList<string> Statuses = ["Stokta", "Rezerve", "Üretimde", "Tükendi"];

    public static readonly IReadOnlyList<string> Seasons = ["İlkbahar/Yaz", "Sonbahar/Kış", "Dört Mevsim"];

    public static readonly IReadOnlyList<decimal> VatRates = [1m, 10m, 20m];

    public static IEnumerable<string> VatLabels => VatRates.Select(FormatVat);

    public static string FormatVat(decimal rate) =>
        "%" + rate.ToString("0.##", CultureInfo.InvariantCulture);

    public static decimal? ParseVat(string text) =>
        decimal.TryParse(text.TrimStart('%'), NumberStyles.Number, CultureInfo.InvariantCulture, out var rate)
            ? rate
            : null;
}
