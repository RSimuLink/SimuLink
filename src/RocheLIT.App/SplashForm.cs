namespace RocheLIT;

internal sealed class SplashForm : Form
{
    private readonly PictureBox _logo;

    public SplashForm()
    {
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = AppLayout.MainWindowClientSize;
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SplashForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Roche LIT - Laboratory Interfacing Tool";

        _logo = new PictureBox
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Image = AppAssets.LoadImage("LITLogo.jpg"),
            Name = "picSplashLogo",
            Padding = new Padding(48),
            SizeMode = PictureBoxSizeMode.Zoom,
            TabStop = false,
        };

        Controls.Add(_logo);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logo.Image?.Dispose();
            _logo.Dispose();
        }

        base.Dispose(disposing);
    }
}
