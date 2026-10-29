namespace StokTakipPro.UI;

public static class Dialogs
{
    public const string AppTitle = "Stok Takip PRO";

    public static void Info(string message) =>
        MessageBox.Show(message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Warning(string message) =>
        MessageBox.Show(message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static bool Confirm(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
        == DialogResult.Yes;

    public static void DatabaseError(Exception exception) =>
        MessageBox.Show(
            $"Veritabanı işlemi sırasında bir hata oluştu:\n{exception.Message}",
            AppTitle,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

    public static bool TryDatabase(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex)
        {
            DatabaseError(ex);
            return false;
        }
    }
}
