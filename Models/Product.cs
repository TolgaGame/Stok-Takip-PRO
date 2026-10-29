namespace StokTakipPro.Models;

public class Product
{
    public int Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string FabricType { get; set; } = string.Empty;

    public string Composition { get; set; } = string.Empty;

    public decimal? GramsPerSquareMeter { get; set; }

    public decimal? WidthCm { get; set; }

    public string Color { get; set; } = string.Empty;

    public string ColorCode { get; set; } = string.Empty;

    public string Pattern { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal? Quantity { get; set; }

    public int? RollCount { get; set; }

    public decimal? TotalWeightKg { get; set; }

    public string LotNo { get; set; } = string.Empty;

    public string Supplier { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Season { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal? MinStock { get; set; }

    public decimal? PurchasePrice { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? VatRate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Boş (null) sayısal alanlar hesaplamalarda 0 kabul edilir.
    public decimal TotalValue => Quantity.GetValueOrDefault() * PurchasePrice.GetValueOrDefault();

    public bool IsCriticalStock =>
        MinStock.GetValueOrDefault() > 0 && Quantity.GetValueOrDefault() < MinStock.GetValueOrDefault();
}

public static class ProductFilter
{
    // SQLite LIKE Türkçe harflerde büyük/küçük harf ayrımı yapmadığı için filtre kültüre duyarlı uygulanır.
    public static List<Product> Apply(IEnumerable<Product> products, string term, string? category)
    {
        var compareInfo = System.Globalization.CultureInfo.CurrentCulture.CompareInfo;
        var trimmedTerm = term.Trim();

        if (!string.IsNullOrEmpty(category))
        {
            products = products.Where(p => string.Equals(p.Category, category, StringComparison.Ordinal));
        }

        if (trimmedTerm.Length > 0)
        {
            products = products.Where(p => Matches(compareInfo, p, trimmedTerm));
        }

        return products.ToList();
    }

    private static bool Matches(System.Globalization.CompareInfo compareInfo, Product product, string term) =>
        Contains(compareInfo, product.ProductCode, term)
        || Contains(compareInfo, product.Name, term)
        || Contains(compareInfo, product.Category, term)
        || Contains(compareInfo, product.Color, term)
        || Contains(compareInfo, product.Supplier, term)
        || Contains(compareInfo, product.LotNo, term)
        || Contains(compareInfo, product.Size, term)
        || Contains(compareInfo, product.FabricType, term)
        || Contains(compareInfo, product.Composition, term);

    private static bool Contains(System.Globalization.CompareInfo compareInfo, string text, string term) =>
        compareInfo.IndexOf(text, term, System.Globalization.CompareOptions.IgnoreCase) >= 0;
}
