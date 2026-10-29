using StokTakipPro.Services;

namespace StokTakipPro.UI;

public class LoginForm : Form
{
    private const string AppTitle = "Stok Takip PRO";
    private const string FormTitle = "Stok Takip PRO - Giriş";
    private const string SubtitleText = "Devam etmek için giriş yapın";
    private const string FailedTitle = "Giriş Başarısız";
    private const string FailedMessage = "Kullanıcı adı veya şifre hatalı. Lütfen tekrar deneyin.";
    private const int FormWidth = 420;
    private const int FormHeight = 470;
    private const int HeaderHeight = 130;
    private const int BodyPadding = 40;
    private const int FieldSpacing = 16;
    private const int LabelSpacing = 4;
    private const int ButtonTopMargin = 12;

    private readonly IAuthService _authService;
    private readonly TextBox _usernameBox = new();
    private readonly TextBox _passwordBox = new();
    private readonly Button _loginButton = new() { Text = "Giriş Yap" };

    public LoginForm(IAuthService authService)
    {
        _authService = authService;
        BuildLayout();
    }

    private void BuildLayout()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = FormTitle;
        ClientSize = new Size(FormWidth, FormHeight);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        Font = Theme.BodyFont;

        ConfigureInputs();
        Controls.Add(BuildBody());
        Controls.Add(BuildHeader());

        AcceptButton = _loginButton;
        ActiveControl = _usernameBox;
        ResumeLayout(false);
    }

    private void ConfigureInputs()
    {
        Theme.StyleTextBox(_usernameBox);
        Theme.StyleTextBox(_passwordBox);
        _passwordBox.UseSystemPasswordChar = true;
        Theme.StyleButton(_loginButton, ButtonKind.Primary);
        _loginButton.Click += OnLoginClick;
    }

    private static Panel BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = HeaderHeight, BackColor = Theme.Header };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

        layout.Controls.Add(new Label
        {
            Text = AppTitle,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomCenter,
            Font = Theme.TitleFont,
            ForeColor = Color.White
        }, 0, 0);

        layout.Controls.Add(new Label
        {
            Text = SubtitleText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            Font = Theme.BodyFont,
            ForeColor = Theme.HeaderMuted
        }, 0, 1);

        header.Controls.Add(layout);
        return header;
    }

    private Panel BuildBody()
    {
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(BodyPadding, BodyPadding - 8, BodyPadding, BodyPadding)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, CreateCaption("Kullanıcı Adı"));
        AddRow(layout, WithMargin(_usernameBox, new Padding(0, 0, 0, FieldSpacing)));
        AddRow(layout, CreateCaption("Şifre"));
        AddRow(layout, WithMargin(_passwordBox, new Padding(0, 0, 0, FieldSpacing)));
        _loginButton.Dock = DockStyle.Fill;
        AddRow(layout, WithMargin(_loginButton, new Padding(0, ButtonTopMargin, 0, 0)));

        body.Controls.Add(layout);
        return body;
    }

    private static void AddRow(TableLayoutPanel layout, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.Dock = DockStyle.Fill;
        layout.Controls.Add(control);
    }

    private static Control WithMargin(Control control, Padding margin)
    {
        control.Margin = margin;
        return control;
    }

    private static Label CreateCaption(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = Theme.LabelFont,
        ForeColor = Theme.TextMuted,
        Margin = new Padding(0, 0, 0, LabelSpacing)
    };

    private void OnLoginClick(object? sender, EventArgs e)
    {
        if (_authService.Authenticate(_usernameBox.Text, _passwordBox.Text))
        {
            DialogResult = DialogResult.OK;
            return;
        }

        MessageBox.Show(FailedMessage, FailedTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _passwordBox.Clear();
        _passwordBox.Focus();
    }
}
