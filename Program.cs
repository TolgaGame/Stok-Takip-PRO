using System.Globalization;
using StokTakipPro.Data;
using StokTakipPro.Services;
using StokTakipPro.UI;

namespace StokTakipPro;

internal static class Program
{
    private const string AppTitle = "Stok Takip PRO";

    internal static readonly CultureInfo Turkish = new("tr-TR");

    [STAThread]
    private static void Main()
    {
        ApplyCulture();
        ApplicationConfiguration.Initialize();

        var database = TryCreateDatabase();
        if (database is null)
        {
            return;
        }

        using (var loginForm = new LoginForm(new AuthService()))
        {
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        Application.Run(new MainForm(new ProductRepository(database)));
    }

    private static void ApplyCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = Turkish;
        CultureInfo.DefaultThreadCurrentUICulture = Turkish;
        CultureInfo.CurrentCulture = Turkish;
        CultureInfo.CurrentUICulture = Turkish;
    }

    private static Database? TryCreateDatabase()
    {
        try
        {
            var database = new Database();
            database.EnsureCreated();
            return database;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Veritabanı hazırlanamadı:\n{ex.Message}",
                AppTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return null;
        }
    }
}