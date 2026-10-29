using System.Drawing.Drawing2D;

namespace StokTakipPro.UI;

public enum ButtonKind
{
    Primary,
    Secondary,
    Danger
}

public static class Theme
{
    public const string FontFamily = "Segoe UI";

    private const int ButtonHeight = 42;
    private const int ButtonMinWidth = 110;
    private const int GridRowHeight = 42;
    private const int GridHeaderHeight = 46;
    private const int GridCellPaddingX = 8;
    private const int TabHeaderWidth = 180;
    private const int TabHeaderHeight = 44;
    private const int GroupPadding = 16;
    private const int BadgeHeight = 24;
    private const int BadgePaddingX = 10;
    private const int BadgeDotSize = 8;
    private const int BadgeGap = 6;
    private const int BadgeFillAlpha = 30;
    private const int ComboItemPaddingX = 6;
    private const int ComboItemExtraHeight = 2;

    public static readonly Color Header = Color.FromArgb(22, 32, 53);
    public static readonly Color HeaderMuted = Color.FromArgb(180, 190, 208);
    public static readonly Color Background = Color.FromArgb(240, 242, 246);
    public static readonly Color Surface = Color.White;
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentHover = Color.FromArgb(29, 78, 216);
    public static readonly Color AccentLight = Color.FromArgb(219, 234, 254);
    public static readonly Color Danger = Color.FromArgb(220, 38, 38);
    public static readonly Color DangerHover = Color.FromArgb(185, 28, 28);
    public static readonly Color Text = Color.FromArgb(31, 41, 55);
    public static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
    public static readonly Color Border = Color.FromArgb(209, 213, 219);
    public static readonly Color HoverNeutral = Color.FromArgb(229, 231, 235);
    public static readonly Color RowAlternate = Color.FromArgb(246, 248, 251);
    public static readonly Color GridLine = Color.FromArgb(229, 231, 235);
    public static readonly Color CriticalRow = Color.FromArgb(254, 226, 226);
    public static readonly Color CriticalRowSelected = Color.FromArgb(252, 205, 205);

    private static readonly Dictionary<string, Color> CategoryColors = new(StringComparer.Ordinal)
    {
        ["Tişört"] = Color.FromArgb(14, 165, 233),
        ["Gömlek"] = Color.FromArgb(220, 38, 38),
        ["Pantolon"] = Color.FromArgb(37, 99, 235),
        ["Kot Pantolon"] = Color.FromArgb(67, 56, 202),
        ["Ceket"] = Color.FromArgb(217, 119, 6),
        ["Mont"] = Color.FromArgb(234, 88, 12),
        ["Kaban"] = Color.FromArgb(120, 72, 40),
        ["Trençkot"] = Color.FromArgb(161, 98, 7),
        ["Kazak"] = Color.FromArgb(147, 51, 234),
        ["Hırka"] = Color.FromArgb(192, 38, 211),
        ["Sweatshirt"] = Color.FromArgb(13, 148, 136),
        ["Elbise"] = Color.FromArgb(236, 72, 153),
        ["Etek"] = Color.FromArgb(244, 63, 94),
        ["Şort"] = Color.FromArgb(101, 163, 13),
        ["Eşofman Altı"] = Color.FromArgb(71, 85, 105),
        ["Bluz"] = Color.FromArgb(139, 92, 246),
        ["Tunik"] = Color.FromArgb(8, 145, 178),
        ["Yelek"] = Color.FromArgb(22, 163, 74),
        ["Takım Elbise"] = Color.FromArgb(30, 41, 59),
        ["Tulum"] = Color.FromArgb(190, 24, 93)
    };

    private static readonly Dictionary<string, Color> StatusColors = new(StringComparer.Ordinal)
    {
        ["Stokta"] = Color.FromArgb(22, 163, 74),
        ["Rezerve"] = Color.FromArgb(217, 119, 6),
        ["Üretimde"] = Color.FromArgb(37, 99, 235),
        ["Tükendi"] = Color.FromArgb(220, 38, 38)
    };

    // Özel adlar ("Zeytin Yeşili") genel adlardan ("Yeşil") önce gelir; açık renkler okunabilsin diye biraz koyulaştırılmıştır.
    private static readonly (string Name, Color Color)[] NamedColors =
    [
        ("Lacivert", Color.FromArgb(30, 58, 138)),
        ("Zeytin", Color.FromArgb(101, 115, 30)),
        ("Haki", Color.FromArgb(120, 113, 60)),
        ("Antrasit", Color.FromArgb(55, 65, 81)),
        ("Turkuaz", Color.FromArgb(8, 145, 178)),
        ("Kahverengi", Color.FromArgb(120, 72, 40)),
        ("Kırmızı", Color.FromArgb(220, 38, 38)),
        ("Bordo", Color.FromArgb(136, 19, 55)),
        ("Vişne", Color.FromArgb(157, 23, 77)),
        ("Mercan", Color.FromArgb(240, 98, 80)),
        ("Somon", Color.FromArgb(240, 120, 100)),
        ("Pembe", Color.FromArgb(236, 72, 153)),
        ("Fuşya", Color.FromArgb(192, 38, 211)),
        ("Mor", Color.FromArgb(126, 34, 206)),
        ("Lila", Color.FromArgb(167, 139, 250)),
        ("Mavi", Color.FromArgb(37, 99, 235)),
        ("Petrol", Color.FromArgb(15, 92, 110)),
        ("Mint", Color.FromArgb(16, 185, 129)),
        ("Yeşil", Color.FromArgb(22, 163, 74)),
        ("Sarı", Color.FromArgb(202, 138, 4)),
        ("Altın", Color.FromArgb(180, 140, 20)),
        ("Turuncu", Color.FromArgb(234, 88, 12)),
        ("Taba", Color.FromArgb(180, 100, 40)),
        ("Camel", Color.FromArgb(180, 130, 70)),
        ("Bej", Color.FromArgb(168, 140, 100)),
        ("Krem", Color.FromArgb(190, 165, 120)),
        ("Ekru", Color.FromArgb(170, 155, 125)),
        ("Beyaz", Color.FromArgb(156, 163, 175)),
        ("Gümüş", Color.FromArgb(148, 163, 184)),
        ("Gri", Color.FromArgb(107, 114, 128)),
        ("Siyah", Color.FromArgb(17, 24, 39))
    ];

    public static readonly Font BadgeFont = new(FontFamily, 9f, FontStyle.Bold);
    public static readonly Font TitleFont = new(FontFamily, 18f, FontStyle.Bold);
    public static readonly Font HeadingFont = new(FontFamily, 14f, FontStyle.Bold);
    public static readonly Font BodyFont = new(FontFamily, 10.5f);
    public static readonly Font InputFont = new(FontFamily, 11f);
    public static readonly Font LabelFont = new(FontFamily, 10f, FontStyle.Bold);
    public static readonly Font ButtonFont = new(FontFamily, 10.5f, FontStyle.Bold);

    public static Color? FindCategoryColor(string category) =>
        CategoryColors.TryGetValue(category, out var color) ? color : null;

    public static Color? FindStatusColor(string status) =>
        StatusColors.TryGetValue(status, out var color) ? color : null;

    // "Koyu Kırmızı" gibi serbest girişlerde içinde geçen ilk bilinen renk adı kullanılır.
    public static Color? FindColorByName(string text)
    {
        var compareInfo = Program.Turkish.CompareInfo;
        foreach (var (name, color) in NamedColors)
        {
            if (compareInfo.IndexOf(text, name, System.Globalization.CompareOptions.IgnoreCase) >= 0)
            {
                return color;
            }
        }

        return null;
    }

    public static void StyleCategoryComboBox(ComboBox comboBox)
    {
        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        comboBox.ItemHeight = InputFont.Height + ComboItemExtraHeight;
        comboBox.DrawItem += DrawCategoryItem;
    }

    public static void DrawBadge(Graphics graphics, Rectangle cell, string text, Color color)
    {
        var textSize = TextRenderer.MeasureText(graphics, text, BadgeFont, Size.Empty, BadgeTextFlags);
        var contentWidth = BadgePaddingX * 2 + BadgeDotSize + BadgeGap + textSize.Width;
        var width = Math.Min(contentWidth, cell.Width - GridCellPaddingX * 2);
        var bounds = new Rectangle(
            cell.X + GridCellPaddingX,
            cell.Y + (cell.Height - BadgeHeight) / 2,
            width,
            BadgeHeight);

        using (var path = CreatePillPath(bounds))
        using (var fill = new SolidBrush(Color.FromArgb(BadgeFillAlpha, color)))
        {
            var previous = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillPath(fill, path);
            graphics.SmoothingMode = previous;
        }

        var dotX = bounds.X + BadgePaddingX;
        DrawDot(graphics, new Rectangle(dotX, bounds.Y + (bounds.Height - BadgeDotSize) / 2, BadgeDotSize, BadgeDotSize), color);

        var textX = dotX + BadgeDotSize + BadgeGap;
        var textBounds = new Rectangle(textX, bounds.Y, bounds.Right - BadgePaddingX - textX, bounds.Height);
        TextRenderer.DrawText(graphics, text, BadgeFont, textBounds, color, BadgeTextFlags | TextFormatFlags.EndEllipsis);
    }

    private const TextFormatFlags BadgeTextFlags =
        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

    private static GraphicsPath CreatePillPath(Rectangle bounds)
    {
        var diameter = bounds.Height;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 90, 180);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 180);
        path.CloseFigure();
        return path;
    }

    private static void DrawDot(Graphics graphics, Rectangle bounds, Color color)
    {
        using var brush = new SolidBrush(color);
        var previous = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.FillEllipse(brush, bounds);
        graphics.SmoothingMode = previous;
    }

    private static void DrawCategoryItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox combo || e.Index < 0)
        {
            return;
        }

        e.DrawBackground();

        var text = combo.Items[e.Index] as string ?? string.Empty;
        var textX = e.Bounds.X + ComboItemPaddingX;
        if (FindCategoryColor(text) is { } color)
        {
            var dot = new Rectangle(textX, e.Bounds.Y + (e.Bounds.Height - BadgeDotSize) / 2, BadgeDotSize, BadgeDotSize);
            DrawDot(e.Graphics, dot, color);
            textX += BadgeDotSize + BadgeGap;
        }

        var textBounds = new Rectangle(textX, e.Bounds.Y, e.Bounds.Right - textX, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, text, combo.Font, textBounds, e.ForeColor, BadgeTextFlags | TextFormatFlags.EndEllipsis);
        e.DrawFocusRectangle();
    }

    public static Panel CreateCard() => new()
    {
        BackColor = Surface
    };

    public static void StyleButton(Button button, ButtonKind kind)
    {
        var (back, hover, fore) = kind switch
        {
            ButtonKind.Primary => (Accent, AccentHover, Color.White),
            ButtonKind.Danger => (Danger, DangerHover, Color.White),
            _ => (Surface, HoverNeutral, Text)
        };

        ApplyFlatStyle(button, back, hover, fore);
        button.FlatAppearance.BorderSize = kind == ButtonKind.Secondary ? 1 : 0;
        button.FlatAppearance.BorderColor = Border;
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowOnly;
        button.MinimumSize = new Size(ButtonMinWidth, ButtonHeight);
        button.Padding = new Padding(14, 4, 14, 4);
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = InputFont;
        textBox.ForeColor = Text;
        textBox.BackColor = Surface;
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = InputFont;
        comboBox.ForeColor = Text;
        comboBox.BackColor = Surface;
    }

    public static void StyleGroupBox(GroupBox groupBox)
    {
        groupBox.Font = LabelFont;
        groupBox.ForeColor = Accent;
        groupBox.BackColor = Surface;
        groupBox.Padding = new Padding(GroupPadding, GroupPadding / 2, GroupPadding, GroupPadding / 2);
    }

    public static void StyleTabControl(TabControl tabs)
    {
        tabs.Dock = DockStyle.Fill;
        tabs.Font = ButtonFont;
        tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.ItemSize = new Size(TabHeaderWidth, TabHeaderHeight);
        tabs.Padding = new Point(0, 0);
        tabs.DrawItem += DrawTabHeader;
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = Surface;
        grid.GridColor = GridLine;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = GridHeaderHeight;
        grid.RowTemplate.Height = GridRowHeight;

        var cellPadding = new Padding(GridCellPaddingX, 0, GridCellPaddingX, 0);

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Header,
            ForeColor = Color.White,
            Font = LabelFont,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            SelectionBackColor = Header,
            SelectionForeColor = Color.White,
            Padding = cellPadding
        };

        grid.DefaultCellStyle = CreateRowStyle(Surface, cellPadding);
        grid.AlternatingRowsDefaultCellStyle = CreateRowStyle(RowAlternate, cellPadding);
        // Alternatif satır stili sütunun hizalamasını ezer; sağa hizalı sayı sütunları kaymasın diye hizalama sütundan gelir.
        grid.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.NotSet;
    }

    private static DataGridViewCellStyle CreateRowStyle(Color back, Padding padding) => new()
    {
        BackColor = back,
        ForeColor = Text,
        Font = BodyFont,
        SelectionBackColor = AccentLight,
        SelectionForeColor = Text,
        Alignment = DataGridViewContentAlignment.MiddleLeft,
        Padding = padding
    };

    private static void ApplyFlatStyle(Button button, Color back, Color hover, Color fore)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.BackColor = back;
        button.ForeColor = fore;
        button.Font = ButtonFont;
        button.Cursor = Cursors.Hand;
        button.FlatAppearance.MouseOverBackColor = hover;
        button.FlatAppearance.MouseDownBackColor = hover;
    }

    private static void DrawTabHeader(object? sender, DrawItemEventArgs e)
    {
        if (sender is not TabControl tabs)
        {
            return;
        }

        var selected = e.Index == tabs.SelectedIndex;
        using var background = new SolidBrush(selected ? Accent : Background);
        e.Graphics.FillRectangle(background, e.Bounds);

        TextRenderer.DrawText(
            e.Graphics,
            tabs.TabPages[e.Index].Text,
            ButtonFont,
            e.Bounds,
            selected ? Color.White : TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
