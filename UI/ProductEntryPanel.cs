using System.Globalization;
using StokTakipPro.Data;
using StokTakipPro.Models;

namespace StokTakipPro.UI;

public class ProductEntryPanel : UserControl
{
    private const string AddTitleText = "Yeni Ürün Ekle";
    private const string EditTitleText = "Ürünü Düzenle";
    private const string ClearText = "Temizle";
    private const string CancelEditText = "Vazgeç";
    private const string NumberFormat = "G29";
    private const string IntegerFormat = "N0";

    private const int PagePadding = 24;
    private const int TitleHeight = 48;
    private const int ButtonBarHeight = 72;
    private const int SectionSpacing = 16;
    private const int FieldGap = 12;
    private const int FieldSpacing = 10;
    private const int FieldColumnCount = 4;
    private const int NotesHeight = 64;

    private const int MaxCodeLength = 50;
    private const int MaxTextLength = 100;
    private const int MaxNotesLength = 500;
    private const decimal MaxNumber = 1_000_000_000m;

    private readonly IProductRepository _repository;

    private readonly Label _titleLabel = new();
    private readonly Button _saveButton = new() { Text = "Kaydet" };
    private readonly Button _clearButton = new() { Text = ClearText };

    private readonly TextBox _codeBox = new();
    private readonly TextBox _nameBox = new();
    private readonly TextBox _supplierBox = new();
    private readonly TextBox _notesBox = new();
    private readonly TextBox _compositionBox = new();
    private readonly TextBox _gsmBox = new();
    private readonly TextBox _widthBox = new();
    private readonly TextBox _colorBox = new();
    private readonly TextBox _colorCodeBox = new();
    private readonly TextBox _patternBox = new();
    private readonly TextBox _quantityBox = new();
    private readonly TextBox _rollCountBox = new();
    private readonly TextBox _totalWeightBox = new();
    private readonly TextBox _lotBox = new();
    private readonly TextBox _locationBox = new();
    private readonly TextBox _minStockBox = new();
    private readonly TextBox _purchasePriceBox = new();
    private readonly TextBox _salePriceBox = new();

    private readonly ComboBox _categoryBox = new();
    private readonly ComboBox _sizeBox = new();
    private readonly ComboBox _fabricTypeBox = new();
    private readonly ComboBox _seasonBox = new();
    private readonly ComboBox _unitBox = new();
    private readonly ComboBox _statusBox = new();
    private readonly ComboBox _vatBox = new();

    private int? _editingId;

    public event EventHandler? EditCompleted;

    public ProductEntryPanel(IProductRepository repository)
    {
        _repository = repository;
        BuildLayout();
        ClearFields();
        SetEditMode(null);
    }

    private TextBox[] TextBoxes =>
    [
        _codeBox, _nameBox, _supplierBox, _notesBox, _compositionBox, _gsmBox, _widthBox,
        _colorBox, _colorCodeBox, _patternBox, _quantityBox, _rollCountBox, _totalWeightBox,
        _lotBox, _locationBox, _minStockBox, _purchasePriceBox, _salePriceBox
    ];

    private ComboBox[] ComboBoxes =>
        [_categoryBox, _sizeBox, _fabricTypeBox, _seasonBox, _unitBox, _statusBox, _vatBox];

    public void LoadForEdit(Product product)
    {
        _codeBox.Text = product.ProductCode;
        _nameBox.Text = product.Name;
        SelectCombo(_categoryBox, product.Category);
        _supplierBox.Text = product.Supplier;
        SelectCombo(_seasonBox, product.Season);
        _notesBox.Text = product.Notes;

        SelectCombo(_fabricTypeBox, product.FabricType);
        _compositionBox.Text = product.Composition;
        _gsmBox.Text = FormatNumber(product.GramsPerSquareMeter);
        _widthBox.Text = FormatNumber(product.WidthCm);
        _colorBox.Text = product.Color;
        _colorCodeBox.Text = product.ColorCode;
        _patternBox.Text = product.Pattern;
        SelectCombo(_sizeBox, product.Size);

        SelectCombo(_unitBox, product.Unit);
        _quantityBox.Text = FormatNumber(product.Quantity);
        _rollCountBox.Text = product.RollCount?.ToString(Program.Turkish) ?? string.Empty;
        _totalWeightBox.Text = FormatNumber(product.TotalWeightKg);
        _lotBox.Text = product.LotNo;
        _locationBox.Text = product.Location;
        SelectCombo(_statusBox, product.Status);
        _minStockBox.Text = FormatNumber(product.MinStock);
        _purchasePriceBox.Text = FormatNumber(product.PurchasePrice);
        _salePriceBox.Text = FormatNumber(product.SalePrice);
        SelectCombo(_vatBox, product.VatRate is { } vat ? ProductOptions.FormatVat(vat) : string.Empty);

        SetEditMode(product.Id);
        _codeBox.Focus();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            _codeBox.Focus();
        }
        else if (_editingId is not null)
        {
            // Düzenleme yarım bırakılıp sekmeden çıkılırsa sonraki kayıt yanlışlıkla güncelleme olmasın.
            ClearFields();
            SetEditMode(null);
        }
    }

    #region Düzen

    private void BuildLayout()
    {
        SuspendLayout();
        Dock = DockStyle.Fill;
        BackColor = Theme.Background;
        Padding = new Padding(PagePadding);
        ConfigureInputs();

        var sections = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
        sections.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddSection(sections, "Genel Bilgi", BuildGeneralFields());
        AddSection(sections, "Teknik Bilgi", BuildTechnicalFields());
        AddSection(sections, "Stok ve Fiyat", BuildStockFields());

        var scrollHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scrollHost.Controls.Add(sections);

        _titleLabel.Dock = DockStyle.Top;
        _titleLabel.Height = TitleHeight;
        _titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        _titleLabel.Font = Theme.HeadingFont;
        _titleLabel.ForeColor = Theme.Text;

        Controls.Add(scrollHost);
        Controls.Add(BuildButtonBar());
        Controls.Add(_titleLabel);
        ResumeLayout(false);
    }

    private void ConfigureInputs()
    {
        foreach (var box in TextBoxes)
        {
            Theme.StyleTextBox(box);
            box.MaxLength = MaxTextLength;
        }

        _codeBox.MaxLength = MaxCodeLength;
        _notesBox.MaxLength = MaxNotesLength;
        _notesBox.Multiline = true;
        _notesBox.AcceptsReturn = true;
        _notesBox.ScrollBars = ScrollBars.Vertical;
        _notesBox.Height = NotesHeight;
        _notesBox.MinimumSize = new Size(0, NotesHeight);

        foreach (var combo in ComboBoxes)
        {
            Theme.StyleComboBox(combo);
        }

        Theme.StyleCategoryComboBox(_categoryBox);
        _colorBox.TextChanged += (_, _) => _colorBox.ForeColor = Theme.FindColorByName(_colorBox.Text) ?? Theme.Text;

        FillCombo(_categoryBox, ProductOptions.Categories);
        FillCombo(_sizeBox, ProductOptions.Sizes);
        FillCombo(_fabricTypeBox, ProductOptions.FabricTypes);
        FillCombo(_seasonBox, ProductOptions.Seasons);
        FillCombo(_unitBox, ProductOptions.Units);
        FillCombo(_statusBox, ProductOptions.Statuses);
        FillCombo(_vatBox, ProductOptions.VatLabels);

        Theme.StyleButton(_saveButton, ButtonKind.Primary);
        Theme.StyleButton(_clearButton, ButtonKind.Secondary);
        _saveButton.Click += (_, _) => Save();
        _clearButton.Click += (_, _) => ResetForm();
    }

    private TableLayoutPanel BuildGeneralFields()
    {
        var grid = CreateFieldGrid();
        Place(grid, "Ürün Kodu *", _codeBox, 0, 0);
        Place(grid, "Ürün Adı *", _nameBox, 1, 0, 2);
        Place(grid, "Kategori", _categoryBox, 3, 0);
        Place(grid, "Tedarikçi", _supplierBox, 0, 1);
        Place(grid, "Sezon", _seasonBox, 1, 1);
        Place(grid, "Notlar", _notesBox, 2, 1, 2);
        return grid;
    }

    private TableLayoutPanel BuildTechnicalFields()
    {
        var grid = CreateFieldGrid();
        Place(grid, "Kumaş Türü", _fabricTypeBox, 0, 0);
        Place(grid, "Kumaş Bileşimi", _compositionBox, 1, 0);
        Place(grid, "Gramaj (gr/m²)", _gsmBox, 2, 0);
        Place(grid, "En (cm)", _widthBox, 3, 0);
        Place(grid, "Renk", _colorBox, 0, 1);
        Place(grid, "Renk Kodu", _colorCodeBox, 1, 1);
        Place(grid, "Desen", _patternBox, 2, 1);
        Place(grid, "Beden", _sizeBox, 3, 1);
        return grid;
    }

    private TableLayoutPanel BuildStockFields()
    {
        var grid = CreateFieldGrid();
        Place(grid, "Birim", _unitBox, 0, 0);
        Place(grid, "Miktar", _quantityBox, 1, 0);
        Place(grid, "Top/Rulo Sayısı", _rollCountBox, 2, 0);
        Place(grid, "Toplam Ağırlık (kg)", _totalWeightBox, 3, 0);
        Place(grid, "Parti/Lot No", _lotBox, 0, 1);
        Place(grid, "Depo/Raf", _locationBox, 1, 1);
        Place(grid, "Durum", _statusBox, 2, 1);
        Place(grid, "Minimum Stok", _minStockBox, 3, 1);
        Place(grid, "Alış Fiyatı (TL)", _purchasePriceBox, 0, 2);
        Place(grid, "Satış Fiyatı (TL)", _salePriceBox, 1, 2);
        Place(grid, "KDV Oranı", _vatBox, 2, 2);
        return grid;
    }

    private FlowLayoutPanel BuildButtonBar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = ButtonBarHeight,
            Padding = new Padding(0, 16, 0, 0)
        };
        _saveButton.Margin = new Padding(0, 0, 10, 0);
        _clearButton.Margin = new Padding(0);
        bar.Controls.Add(_saveButton);
        bar.Controls.Add(_clearButton);
        return bar;
    }

    private static void AddSection(TableLayoutPanel host, string title, TableLayoutPanel fields)
    {
        var group = new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, SectionSpacing)
        };
        Theme.StyleGroupBox(group);
        group.Controls.Add(fields);

        host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        host.Controls.Add(group);
    }

    private static TableLayoutPanel CreateFieldGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = FieldColumnCount
        };

        for (var column = 0; column < FieldColumnCount; column++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / FieldColumnCount));
        }

        return grid;
    }

    private static void Place(TableLayoutPanel grid, string caption, Control input, int column, int row, int columnSpan = 1)
    {
        var field = CreateField(caption, input);
        grid.Controls.Add(field, column, row);
        if (columnSpan > 1)
        {
            grid.SetColumnSpan(field, columnSpan);
        }
    }

    private static TableLayoutPanel CreateField(string caption, Control input)
    {
        var field = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, FieldGap, FieldSpacing)
        };
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        field.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        field.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        field.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            Font = Theme.LabelFont,
            ForeColor = Theme.TextMuted,
            Margin = new Padding(0, 0, 0, 4)
        }, 0, 0);

        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0);
        field.Controls.Add(input, 0, 1);
        return field;
    }

    #endregion

    #region Kaydetme

    private void Save()
    {
        if (!TryReadProduct(out var product))
        {
            return;
        }

        var wasEditing = _editingId is not null;
        if (!TryPersist(product))
        {
            return;
        }

        Dialogs.Info(wasEditing ? "Ürün başarıyla güncellendi." : "Ürün başarıyla kaydedildi.");
        ResetForm();
        if (wasEditing)
        {
            EditCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool TryPersist(Product product)
    {
        try
        {
            if (_editingId is int id)
            {
                product.Id = id;
                _repository.Update(product);
            }
            else
            {
                _repository.Add(product);
            }

            return true;
        }
        catch (DuplicateProductCodeException ex)
        {
            Dialogs.Warning(ex.Message);
            _codeBox.Focus();
            return false;
        }
        catch (Exception ex)
        {
            Dialogs.DatabaseError(ex);
            return false;
        }
    }

    private bool TryReadProduct(out Product product)
    {
        product = new Product();

        var code = _codeBox.Text.Trim();
        if (code.Length == 0)
        {
            return Reject(_codeBox, "Ürün kodu boş bırakılamaz.");
        }

        var name = _nameBox.Text.Trim();
        if (name.Length == 0)
        {
            return Reject(_nameBox, "Ürün adı boş bırakılamaz.");
        }

        if (!TryReadDecimal(_gsmBox, "Gramaj", out var gsm)
            || !TryReadDecimal(_widthBox, "En", out var width)
            || !TryReadDecimal(_quantityBox, "Miktar", out var quantity)
            || !TryReadInteger(_rollCountBox, "Top/Rulo sayısı", out var rollCount)
            || !TryReadDecimal(_totalWeightBox, "Toplam ağırlık", out var totalWeight)
            || !TryReadDecimal(_minStockBox, "Minimum stok", out var minStock)
            || !TryReadDecimal(_purchasePriceBox, "Alış fiyatı", out var purchasePrice)
            || !TryReadDecimal(_salePriceBox, "Satış fiyatı", out var salePrice))
        {
            return false;
        }

        product = new Product
        {
            ProductCode = code,
            Name = name,
            Category = ComboText(_categoryBox),
            FabricType = ComboText(_fabricTypeBox),
            Composition = _compositionBox.Text.Trim(),
            GramsPerSquareMeter = gsm,
            WidthCm = width,
            Color = _colorBox.Text.Trim(),
            ColorCode = _colorCodeBox.Text.Trim(),
            Pattern = _patternBox.Text.Trim(),
            Unit = ComboText(_unitBox),
            Quantity = quantity,
            RollCount = rollCount,
            TotalWeightKg = totalWeight,
            LotNo = _lotBox.Text.Trim(),
            Supplier = _supplierBox.Text.Trim(),
            Location = _locationBox.Text.Trim(),
            Season = ComboText(_seasonBox),
            Size = ComboText(_sizeBox),
            Status = ComboText(_statusBox),
            MinStock = minStock,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            VatRate = ProductOptions.ParseVat(ComboText(_vatBox)),
            Notes = _notesBox.Text.Trim()
        };
        return true;
    }

    private static bool TryReadInteger(TextBox box, string caption, out int? value)
    {
        value = null;
        var text = box.Text.Trim();
        if (text.Length == 0)
        {
            return true;
        }

        if (!int.TryParse(text, NumberStyles.AllowLeadingSign, Program.Turkish, out var parsed))
        {
            return Reject(box, $"{caption} geçerli bir tam sayı olmalıdır.");
        }

        if (parsed < 0)
        {
            return Reject(box, $"{caption} negatif olamaz.");
        }

        value = parsed;
        return true;
    }

    // Binlik ayracı kabul edilmez; aksi halde "1.5" sessizce 15 olarak okunurdu.
    private static bool TryReadDecimal(TextBox box, string caption, out decimal? value)
    {
        value = null;
        var text = box.Text.Trim();
        if (text.Length == 0)
        {
            return true;
        }

        const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (!decimal.TryParse(text, styles, Program.Turkish, out var parsed))
        {
            return Reject(box, $"{caption} geçerli bir sayı olmalıdır (örn. 12,50).");
        }

        if (parsed < 0)
        {
            return Reject(box, $"{caption} negatif olamaz.");
        }

        if (parsed > MaxNumber)
        {
            return Reject(box, $"{caption} en fazla {MaxNumber.ToString(IntegerFormat, Program.Turkish)} olabilir.");
        }

        value = parsed;
        return true;
    }

    private static bool Reject(Control control, string message)
    {
        Dialogs.Warning(message);
        control.Focus();
        return false;
    }

    #endregion

    #region Form durumu

    private void ResetForm()
    {
        ClearFields();
        SetEditMode(null);
        _codeBox.Focus();
    }

    private void ClearFields()
    {
        foreach (var box in TextBoxes)
        {
            box.Clear();
        }

        foreach (var combo in ComboBoxes)
        {
            combo.SelectedIndex = 0;
        }

        SelectCombo(_unitBox, ProductOptions.Units[0]);
        SelectCombo(_statusBox, ProductOptions.Statuses[0]);
    }

    private void SetEditMode(int? productId)
    {
        _editingId = productId;
        var editing = productId is not null;
        _titleLabel.Text = editing ? EditTitleText : AddTitleText;
        _clearButton.Text = editing ? CancelEditText : ClearText;
    }

    private static string FormatNumber(decimal? value) =>
        value?.ToString(NumberFormat, Program.Turkish) ?? string.Empty;

    private static void FillCombo(ComboBox combo, IEnumerable<string> values)
    {
        combo.Items.Add(string.Empty);
        foreach (var value in values)
        {
            combo.Items.Add(value);
        }
    }

    // Listede olmayan (eski/özel) bir değer düzenlemede kaybolmasın diye listeye eklenir.
    private static void SelectCombo(ComboBox combo, string value)
    {
        if (value.Length > 0 && !combo.Items.Contains(value))
        {
            combo.Items.Add(value);
        }

        combo.SelectedIndex = combo.Items.IndexOf(value);
    }

    private static string ComboText(ComboBox combo) => combo.SelectedItem as string ?? string.Empty;

    #endregion
}
