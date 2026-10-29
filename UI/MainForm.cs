using StokTakipPro.Data;

namespace StokTakipPro.UI;

public class MainForm : Form
{
    private const string ClockFormat = "d MMMM yyyy dddd - HH:mm:ss";
    private const int ClockIntervalMs = 1000;
    private const int ClockWidth = 500;
    private const int HeaderHeight = 72;
    private const int PagePadding = 24;
    private const int MinWindowWidth = 1000;
    private const int MinWindowHeight = 680;
    private const int InitialClientWidth = 1200;
    private const int InitialClientHeight = 760;

    private readonly System.Windows.Forms.Timer _clockTimer = new() { Interval = ClockIntervalMs };
    private readonly Label _clockLabel = new();
    private readonly TabControl _tabs = new();

    public MainForm(IProductRepository repository)
    {
        BuildLayout();

        var entryPanel = new ProductEntryPanel(repository);
        var listPanel = new ProductListPanel(repository);
        var entryTab = AddTab("Ürün Ekle", entryPanel);
        var listTab = AddTab("Ürün Listesi", listPanel);

        listPanel.EditRequested += (_, product) =>
        {
            entryPanel.LoadForEdit(product);
            _tabs.SelectedTab = entryTab;
        };
        entryPanel.EditCompleted += (_, _) => _tabs.SelectedTab = listTab;

        _clockTimer.Tick += (_, _) => UpdateClock();
        UpdateClock();
        _clockTimer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clockTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = Dialogs.AppTitle;
        MinimumSize = new Size(MinWindowWidth, MinWindowHeight);
        ClientSize = new Size(InitialClientWidth, InitialClientHeight);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        Font = Theme.BodyFont;

        Theme.StyleTabControl(_tabs);

        Controls.Add(_tabs);
        Controls.Add(BuildHeader());
        ResumeLayout(false);
    }

    private Panel BuildHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = HeaderHeight,
            BackColor = Theme.Header,
            Padding = new Padding(PagePadding, 0, PagePadding, 0)
        };

        var title = new Label
        {
            Text = Dialogs.AppTitle,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = Theme.TitleFont,
            ForeColor = Color.White
        };

        _clockLabel.Dock = DockStyle.Right;
        _clockLabel.Width = ClockWidth;
        _clockLabel.TextAlign = ContentAlignment.MiddleRight;
        _clockLabel.Font = Theme.HeadingFont;
        _clockLabel.ForeColor = Theme.HeaderMuted;

        header.Controls.Add(title);
        header.Controls.Add(_clockLabel);
        return header;
    }

    private TabPage AddTab(string title, Control content)
    {
        var page = new TabPage(title) { BackColor = Theme.Background, Padding = Padding.Empty };
        content.Dock = DockStyle.Fill;
        page.Controls.Add(content);
        _tabs.TabPages.Add(page);
        return page;
    }

    private void UpdateClock() =>
        _clockLabel.Text = DateTime.Now.ToString(ClockFormat, Program.Turkish);
}
