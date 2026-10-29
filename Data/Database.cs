using System.Globalization;
using Microsoft.Data.Sqlite;

namespace StokTakipPro.Data;

public class Database
{
    private const string AppFolderName = "StokTakipPRO";
    private const string DatabaseFileName = "stok.db";

    private const string CreateProductsTable = """
        CREATE TABLE IF NOT EXISTS Products (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            Name      TEXT NOT NULL,
            Category  TEXT NOT NULL DEFAULT '',
            Quantity  INTEGER NOT NULL DEFAULT 0,
            WeightKg  TEXT NOT NULL DEFAULT '0',
            UnitPrice TEXT NOT NULL DEFAULT '0',
            Notes     TEXT NOT NULL DEFAULT '',
            CreatedAt TEXT NOT NULL
        );
        """;

    // Eski Quantity/WeightKg/UnitPrice sütunları veri kaybı olmasın diye tabloda kalır; yeni kod bunları kullanmaz.
    private const string AddTextileFields = """
        ALTER TABLE Products ADD COLUMN ProductCode TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN FabricType TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Composition TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN GramsPerSquareMeter TEXT NULL;
        ALTER TABLE Products ADD COLUMN WidthCm TEXT NULL;
        ALTER TABLE Products ADD COLUMN Color TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN ColorCode TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Pattern TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Unit TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN StockQuantity TEXT NULL;
        ALTER TABLE Products ADD COLUMN RollCount INTEGER NULL;
        ALTER TABLE Products ADD COLUMN TotalWeightKg TEXT NULL;
        ALTER TABLE Products ADD COLUMN LotNo TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Supplier TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Location TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Season TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Size TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN Status TEXT NOT NULL DEFAULT '';
        ALTER TABLE Products ADD COLUMN MinStock TEXT NULL;
        ALTER TABLE Products ADD COLUMN PurchasePrice TEXT NULL;
        ALTER TABLE Products ADD COLUMN SalePrice TEXT NULL;
        ALTER TABLE Products ADD COLUMN VatRate TEXT NULL;

        UPDATE Products SET
            ProductCode   = 'URN-' || printf('%05d', Id),
            Unit          = 'Adet',
            Status        = 'Stokta',
            StockQuantity = Quantity,
            TotalWeightKg = WeightKg,
            PurchasePrice = UnitPrice;

        CREATE UNIQUE INDEX IF NOT EXISTS IX_Products_ProductCode ON Products (ProductCode COLLATE NOCASE);
        """;

    private sealed record Migration(int Version, string Sql);

    // Yeni şema değişikliği = listeye sıradaki sürüm numarasıyla tek satır eklemek.
    private static readonly Migration[] Migrations =
    [
        new(1, CreateProductsTable),
        new(2, AddTextileFields)
    ];

    private readonly string _connectionString;

    public Database()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);
        Directory.CreateDirectory(folder);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(folder, DatabaseFileName),
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void EnsureCreated()
    {
        using var connection = OpenConnection();
        var currentVersion = ReadSchemaVersion(connection);

        foreach (var migration in Migrations.Where(m => m.Version > currentVersion).OrderBy(m => m.Version))
        {
            Apply(connection, migration);
        }
    }

    private static int ReadSchemaVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    // Migration ve sürüm güncellemesi aynı transaction'da; yarım kalırsa geri alınır.
    private static void Apply(SqliteConnection connection, Migration migration)
    {
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = FormattableString.Invariant(
            $"{migration.Sql}\nPRAGMA user_version = {migration.Version};");
        command.ExecuteNonQuery();
        transaction.Commit();
    }
}
