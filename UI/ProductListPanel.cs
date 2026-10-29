using StokTakipPro.Data;
using StokTakipPro.Models;

namespace StokTakipPro.UI;

public class ProductListPanel : UserControl
{
    private const string AllCategories = "Tümü";
    private const string NullText = "NULL";
    private const string CurrencyFormat = "#,##0.00 ₺";
    private const string NumberFormat = "#,##0.##";
    private const string IntegerFormat = "N0";

    private const int PagePadding = 24;
    private const int ToolbarHeight = 52;
    private const int SummaryHeight = 80;
    private const int CategoryFilterWidth = 220;
    private const int SearchDebounceMs = 250;

    private readonly IProductRepository _repository;
    private readonly TextBox _searchBox = new();
    private readonly ComboBox _categoryFilter = new();
    private readonly DataGridView _grid = new BufferedGrid();
    private readonly Label _countValueLabel = new();
    private readonly Label _criticalValueLabel = new();
    private readonly Label _stockValueLabel = new();
    private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = SearchDebounceMs };

    private IReadOnlyList<Product> _allProducts = [];

    public event EventHandler<Product>? EditRequested;

    public ProductListPanel(IProductRepository repository)
    {
        _repository = repository;
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            ApplyFilter();
        };
        BuildLayout();
    }

    // Sekmeye her geçişte liste veritabanından yenilenir; arama/filtre ise bellekteki listeden yapılır.
    public void Reload()
    {
        if (!Dialogs.TryDatabase(() => _allProducts = _repository.GetAll()))
        {
            return;
        }

        ApplyFilter();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            Reload();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _searchTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ApplyFilter()
    {
        var category = _categoryFilter.SelectedIndex > 0 ? _categoryFilter.SelectedItem as string : null;
        var products = ProductFilter.Apply(_allProducts, _searchBox.Text, category);

        _grid.DataSource = products;
        _grid.ClearSelection();
        UpdateSummary(products);
    }

    private void ScheduleFilter()
    {
        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private void BuildLayout()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        BackColor = Theme.Background;
        Padding = new Padding(PagePadding);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, ToolbarHeight));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, SummaryHeight));

        ConfigureGrid();

        layout.Controls.Add(BuildToolbar(), 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(BuildSummary(), 0, 2);

        Controls.Add(layout);
        ResumeLayout(false);
    }

    private TableLayoutPanel BuildToolbar()
    {
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 8)
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CategoryFilterWidth));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Theme.StyleTextBox(_searchBox);
        _searchBox.PlaceholderText = "Kod, ad, kategori, beden, kumaş, renk, tedarikçi veya parti no ile ara...";
        _searchBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _searchBox.Margin = new Padding(0);

        Theme.StyleComboBox(_categoryFilter);
        Theme.StyleCategoryComboBox(_categoryFilter);
        _categoryFilter.Items.Add(AllCategories);
        foreach (var category in ProductOptions.Categories)
        {
            _categoryFilter.Items.Add(category);
        }
        _categoryFilter.SelectedIndex = 0;
        _categoryFilter.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _categoryFilter.Margin = new Padding(8, 0, 0, 0);

        _searchBox.TextChanged += (_, _) => ScheduleFilter();
        _categoryFilter.SelectedIndexChanged += (_, _) => ApplyFilter();

        var editButton = new Button { Text = "Düzenle", Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0) };
        var deleteButton = new Button { Text = "Sil", Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0) };
        Theme.StyleButton(editButton, ButtonKind.Secondary);
        Theme.StyleButton(deleteButton, ButtonKind.Danger);
        editButton.Click += (_, _) => EditSelectedProduct();
        deleteButton.Click += (_, _) => DeleteSelectedProduct();

        toolbar.Controls.Add(_searchBox, 0, 0);
        toolbar.Controls.Add(_categoryFilter, 1, 0);
        toolbar.Controls.Add(editButton, 2, 0);
        toolbar.Controls.Add(deleteButton, 3, 0);
        return toolbar;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        Theme.StyleGrid(_grid);
        _grid.DefaultCellStyle.FormatProvider = Program.Turkish;

        AddColumn("Ürün Kodu", nameof(Product.ProductCode), 10);
        AddColumn("Ürün Adı", nameof(Product.Name), 14);
        AddColumn("Kategori", nameof(Product.Category), 13);
        AddColumn("Beden", nameof(Product.Size), 6);
        AddColumn("Kumaş Türü", nameof(Product.FabricType), 9);
        AddColumn("Renk", nameof(Product.Color), 8);
        AddColumn("Miktar", nameof(Product.Quantity), 8, NumberFormat, rightAligned: true);
        AddColumn("Birim", nameof(Product.Unit), 6);
        AddColumn("Min. Stok", nameof(Product.MinStock), 7, NumberFormat, rightAligned: true);
        AddColumn("Alış Fiyatı", nameof(Product.PurchasePrice), 10, CurrencyFormat, rightAligned: true);
        AddColumn("Satış Fiyatı", nameof(Product.SalePrice), 10, CurrencyFormat, rightAligned: true);
        AddColumn("Durum", nameof(Product.Status), 9);
        AddColumn("Tedarikçi", nameof(Product.Supplier), 10);

        _grid.CellFormatting += OnCellFormatting;
        _grid.CellPainting += OnCellPainting;
        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedProduct();
            }
        };
    }

    private void AddColumn(string header, string property, float fillWeight, string? format = null, bool rightAligned = false)
    {
        var column = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        if (format is not null)
        {
            column.DefaultCellStyle.Format = format;
        }

        if (rightAligned)
        {
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        _grid.Columns.Add(column);
    }

    private void OnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0
            || e.CellStyle is null
            || _grid.Rows[e.RowIndex].DataBoundItem is not Product product)
        {
            return;
        }

        if (product.IsCriticalStock)
        {
            e.CellStyle.BackColor = Theme.CriticalRow;
            e.CellStyle.SelectionBackColor = Theme.CriticalRowSelected;
        }

        if (e.Value is null or "")
        {
            e.Value = NullText;
            e.FormattingApplied = true;
            e.CellStyle.ForeColor = Theme.TextMuted;
            e.CellStyle.SelectionForeColor = Theme.TextMuted;
            return;
        }
        if (_grid.Columns[e.ColumnIndex].DataPropertyName == nameof(Product.Color)
            && Theme.FindColorByName(product.Color) is { } color)
        {
            e.CellStyle.ForeColor = color;
            e.CellStyle.SelectionForeColor = color;
            e.CellStyle.Font = Theme.LabelFont;
        }
    }

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics is null || e.Value is not string { Length: > 0 } text)
        {
            return;
        }

        var badgeColor = _grid.Columns[e.ColumnIndex].DataPropertyName switch
        {
            nameof(Product.Category) => Theme.FindCategoryColor(text) ?? Theme.TextMuted,
            nameof(Product.Status) => Theme.FindStatusColor(text) ?? Theme.TextMuted,
            _ => (Color?)null
        };

        if (badgeColor is not { } color)
        {
            return;
        }

        e.Paint(e.ClipBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
        Theme.DrawBadge(e.Graphics, e.CellBounds, text, color);
        e.Handled = true;
    }

    private Panel BuildSummary()
    {
        var summary = Theme.CreateCard();
        summary.Dock = DockStyle.Fill;
        summary.Margin = new Padding(0, 8, 0, 0);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        for (var column = 0; column < layout.ColumnCount; column++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / layout.ColumnCount));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreateStat("Toplam Ürün Çeşidi", _countValueLabel), 0, 0);
        layout.Controls.Add(CreateStat("Kritik Stoktaki Ürün", _criticalValueLabel), 1, 0);
        layout.Controls.Add(CreateStat("Toplam Stok Değeri", _stockValueLabel), 2, 0);

        summary.Controls.Add(layout);
        return summary;
    }

    private static TableLayoutPanel CreateStat(string caption, Label valueLabel)
    {
        var stat = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20, 8, 8, 8)
        };
        stat.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        stat.RowStyles.Add(new RowStyle(SizeType.Percent, 60));

        stat.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Font = Theme.BodyFont,
            ForeColor = Theme.TextMuted
        }, 0, 0);

        valueLabel.Dock = DockStyle.Fill;
        valueLabel.TextAlign = ContentAlignment.TopLeft;
        valueLabel.Font = Theme.HeadingFont;
        valueLabel.ForeColor = Theme.Accent;
        stat.Controls.Add(valueLabel, 0, 1);
        return stat;
    }

    private void UpdateSummary(IReadOnlyList<Product> products)
    {
        var criticalCount = products.Count(p => p.IsCriticalStock);

        _countValueLabel.Text = products.Count.ToString(IntegerFormat, Program.Turkish);
        _criticalValueLabel.Text = criticalCount.ToString(IntegerFormat, Program.Turkish);
        _criticalValueLabel.ForeColor = criticalCount > 0 ? Theme.Danger : Theme.Accent;
        _stockValueLabel.Text = products.Sum(p => p.TotalValue).ToString(CurrencyFormat, Program.Turkish);
    }

    private Product? GetSelectedProduct() =>
        _grid.SelectedRows.Count == 0 ? null : _grid.SelectedRows[0].DataBoundItem as Product;

    private void EditSelectedProduct()
    {
        if (GetSelectedProduct() is not { } product)
        {
            Dialogs.Warning("Lütfen düzenlemek için listeden bir ürün seçin.");
            return;
        }

        EditRequested?.Invoke(this, product);
    }

    private void DeleteSelectedProduct()
    {
        if (GetSelectedProduct() is not { } product)
        {
            Dialogs.Warning("Lütfen silmek için listeden bir ürün seçin.");
            return;
        }

        var confirmed = Dialogs.Confirm(
            $"\"{product.Name}\" ürününü silmek istediğinize emin misiniz?",
            "Silme Onayı");

        if (confirmed && Dialogs.TryDatabase(() => _repository.Delete(product.Id)))
        {
            Reload();
        }
    }

    private sealed class BufferedGrid : DataGridView
    {
        public BufferedGrid()
        {
            DoubleBuffered = true;
        }
    }
}
