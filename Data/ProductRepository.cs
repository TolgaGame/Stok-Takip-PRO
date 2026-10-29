using System.Globalization;
using Microsoft.Data.Sqlite;
using StokTakipPro.Models;

namespace StokTakipPro.Data;

public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();

    IReadOnlyList<Product> Search(string term, string? category = null);

    void Add(Product product);

    void Update(Product product);

    void Delete(int id);
}

public sealed class DuplicateProductCodeException : Exception
{
    public DuplicateProductCodeException(string productCode)
        : base($"\"{productCode}\" ürün kodu zaten kayıtlı. Lütfen farklı bir kod girin.")
    {
    }
}

public class ProductRepository : IProductRepository
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const int SqliteConstraintErrorCode = 19;

    // Yeni alan eklerken: buraya sütun adı, AddFieldParameters'a bağlama ve ReadProduct'a okuma satırı eklenir.
    private static readonly string[] FieldColumns =
    [
        "ProductCode", "Name", "Category", "FabricType", "Composition", "GramsPerSquareMeter", "WidthCm",
        "Color", "ColorCode", "Pattern", "Unit", "StockQuantity", "RollCount", "TotalWeightKg", "LotNo",
        "Supplier", "Location", "Season", "Size", "Status", "MinStock", "PurchasePrice", "SalePrice",
        "VatRate", "Notes"
    ];

    private static readonly string SelectAllSql =
        $"SELECT Id, {string.Join(", ", FieldColumns)}, CreatedAt FROM Products ORDER BY CreatedAt DESC, Id DESC;";

    private static readonly string InsertSql =
        $"INSERT INTO Products ({string.Join(", ", FieldColumns)}, CreatedAt) " +
        $"VALUES ({string.Join(", ", FieldColumns.Select(c => "@" + c))}, @CreatedAt); " +
        "SELECT last_insert_rowid();";

    private static readonly string UpdateSql =
        $"UPDATE Products SET {string.Join(", ", FieldColumns.Select(c => $"{c} = @{c}"))} WHERE Id = @Id;";

    private const string DeleteSql = "DELETE FROM Products WHERE Id = @Id;";

    private readonly Database _database;

    public ProductRepository(Database database)
    {
        _database = database;
    }

    public IReadOnlyList<Product> GetAll()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = SelectAllSql;
        return ReadProducts(command);
    }

    public IReadOnlyList<Product> Search(string term, string? category = null) =>
        ProductFilter.Apply(GetAll(), term, category);

    public void Add(Product product)
    {
        try
        {
            using var connection = _database.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = InsertSql;
            AddFieldParameters(command, product);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
            product.Id = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        }
        catch (SqliteException ex) when (IsUniqueViolation(ex))
        {
            throw new DuplicateProductCodeException(product.ProductCode);
        }
    }

    public void Update(Product product)
    {
        try
        {
            using var connection = _database.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = UpdateSql;
            AddFieldParameters(command, product);
            command.Parameters.AddWithValue("@Id", product.Id);

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException("Güncellenecek ürün bulunamadı. Silinmiş olabilir.");
            }
        }
        catch (SqliteException ex) when (IsUniqueViolation(ex))
        {
            throw new DuplicateProductCodeException(product.ProductCode);
        }
    }

    public void Delete(int id)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = DeleteSql;
        command.Parameters.AddWithValue("@Id", id);
        command.ExecuteNonQuery();
    }

    private static bool IsUniqueViolation(SqliteException ex) =>
        ex.SqliteErrorCode == SqliteConstraintErrorCode
        && ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);

    private static void AddFieldParameters(SqliteCommand command, Product product)
    {
        Bind(command, "ProductCode", product.ProductCode);
        Bind(command, "Name", product.Name);
        Bind(command, "Category", product.Category);
        Bind(command, "FabricType", product.FabricType);
        Bind(command, "Composition", product.Composition);
        Bind(command, "GramsPerSquareMeter", ToDb(product.GramsPerSquareMeter));
        Bind(command, "WidthCm", ToDb(product.WidthCm));
        Bind(command, "Color", product.Color);
        Bind(command, "ColorCode", product.ColorCode);
        Bind(command, "Pattern", product.Pattern);
        Bind(command, "Unit", product.Unit);
        Bind(command, "StockQuantity", ToDb(product.Quantity));
        Bind(command, "RollCount", ToDb(product.RollCount));
        Bind(command, "TotalWeightKg", ToDb(product.TotalWeightKg));
        Bind(command, "LotNo", product.LotNo);
        Bind(command, "Supplier", product.Supplier);
        Bind(command, "Location", product.Location);
        Bind(command, "Season", product.Season);
        Bind(command, "Size", product.Size);
        Bind(command, "Status", product.Status);
        Bind(command, "MinStock", ToDb(product.MinStock));
        Bind(command, "PurchasePrice", ToDb(product.PurchasePrice));
        Bind(command, "SalePrice", ToDb(product.SalePrice));
        Bind(command, "VatRate", ToDb(product.VatRate));
        Bind(command, "Notes", product.Notes);
    }

    private static void Bind(SqliteCommand command, string column, object value) =>
        command.Parameters.AddWithValue("@" + column, value);

    private static object ToDb(decimal? value)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        return value.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static object ToDb(int? value)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        return value.Value;
    }

    private readonly record struct RowReader(SqliteDataReader Reader, Dictionary<string, int> Ordinals);

    private static List<Product> ReadProducts(SqliteCommand command)
    {
        var products = new List<Product>();
        using var reader = command.ExecuteReader();
        var ordinals = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName);
        var row = new RowReader(reader, ordinals);

        while (reader.Read())
        {
            products.Add(ReadProduct(row));
        }

        return products;
    }

    private static Product ReadProduct(RowReader reader) => new()
    {
        Id = reader.Reader.GetInt32(reader.Ordinals["Id"]),
        ProductCode = ReadText(reader, "ProductCode"),
        Name = ReadText(reader, "Name"),
        Category = ReadText(reader, "Category"),
        FabricType = ReadText(reader, "FabricType"),
        Composition = ReadText(reader, "Composition"),
        GramsPerSquareMeter = ReadDecimal(reader, "GramsPerSquareMeter"),
        WidthCm = ReadDecimal(reader, "WidthCm"),
        Color = ReadText(reader, "Color"),
        ColorCode = ReadText(reader, "ColorCode"),
        Pattern = ReadText(reader, "Pattern"),
        Unit = ReadText(reader, "Unit"),
        Quantity = ReadDecimal(reader, "StockQuantity"),
        RollCount = ReadInt(reader, "RollCount"),
        TotalWeightKg = ReadDecimal(reader, "TotalWeightKg"),
        LotNo = ReadText(reader, "LotNo"),
        Supplier = ReadText(reader, "Supplier"),
        Location = ReadText(reader, "Location"),
        Season = ReadText(reader, "Season"),
        Size = ReadText(reader, "Size"),
        Status = ReadText(reader, "Status"),
        MinStock = ReadDecimal(reader, "MinStock"),
        PurchasePrice = ReadDecimal(reader, "PurchasePrice"),
        SalePrice = ReadDecimal(reader, "SalePrice"),
        VatRate = ReadDecimal(reader, "VatRate"),
        Notes = ReadText(reader, "Notes"),
        CreatedAt = DateTime.ParseExact(ReadText(reader, "CreatedAt"), DateTimeFormat, CultureInfo.InvariantCulture)
    };

    private static string ReadText(RowReader row, string column)
    {
        var ordinal = row.Ordinals[column];
        return row.Reader.IsDBNull(ordinal) ? string.Empty : row.Reader.GetString(ordinal);
    }

    private static decimal? ReadDecimal(RowReader row, string column)
    {
        var ordinal = row.Ordinals[column];
        if (row.Reader.IsDBNull(ordinal))
        {
            return null;
        }

        return decimal.Parse(row.Reader.GetString(ordinal), CultureInfo.InvariantCulture);
    }

    private static int? ReadInt(RowReader row, string column)
    {
        var ordinal = row.Ordinals[column];
        if (row.Reader.IsDBNull(ordinal))
        {
            return null;
        }

        return row.Reader.GetInt32(ordinal);
    }
}
