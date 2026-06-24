using RocheSimuLink.Him;
using RocheSimuLink.Models;

namespace RocheSimuLink;

/// <summary>
/// Edits <see cref="ConnectionSettings"/> (LIS host/port, listener port, and
/// MSH identity fields) and lets the user load the assay catalog from a Host
/// Interface Manual PDF or a portable HIMdefinitions_00x.txt file. Layout is
/// built in code to keep the dialog simple.
/// </summary>
public sealed class SettingsForm : Form
{
    private readonly SimuLinkSettings _settings;
    private readonly ConnectionSettings _connection;

    private readonly TextBox _txtHost = new();
    private readonly NumericUpDown _numLisPort = NewPort();
    private readonly NumericUpDown _numListenPort = NewPort();
    private readonly TextBox _txtSendingApp = new();
    private readonly TextBox _txtSendingFacility = new();
    private readonly TextBox _txtReceivingApp = new();
    private readonly TextBox _txtReceivingFacility = new();
    private readonly TextBox _txtVersion = new();

    private readonly Label _lblCatalog = new()
    {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(3, 8, 3, 3),
    };

    private readonly CheckBox _chkRemember = new()
    {
        Text = "Remember catalog (survives restart)",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(3, 3, 3, 3),
    };

    /// <summary>
    /// True when the user imported a new assay catalog during this dialog, so
    /// the caller knows to refresh any catalog-bound UI (test/sample dropdowns).
    /// </summary>
    public bool CatalogChanged { get; private set; }

    public SettingsForm(SimuLinkSettings settings)
    {
        _settings = settings;
        _connection = settings.Connection;
        BuildLayout();
        LoadValues();
    }

    private static NumericUpDown NewPort() => new()
    {
        Minimum = 1,
        Maximum = 65535,
        Width = 120,
    };

    private void BuildLayout()
    {
        Text = "Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(440, 470);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Padding = new Padding(12),
            AutoSize = true,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "LIS Host:", _txtHost);
        AddRow(layout, "LIS Port:", _numLisPort);
        AddRow(layout, "Listener Port:", _numListenPort);
        AddRow(layout, "Sending Application (MSH-3):", _txtSendingApp);
        AddRow(layout, "Sending Facility (MSH-4):", _txtSendingFacility);
        AddRow(layout, "Receiving Application (MSH-5):", _txtReceivingApp);
        AddRow(layout, "Receiving Facility (MSH-6):", _txtReceivingFacility);
        AddRow(layout, "HL7 Version (MSH-12):", _txtVersion);

        AddSectionHeader(layout, "Assay catalog");
        AddRow(layout, "Loaded catalog:", _lblCatalog);

        var btnLoadHim = new Button { Text = "Load HIM (PDF)\u2026", Width = 150, AutoSize = true };
        var btnLoadDefs = new Button { Text = "Load definitions\u2026", Width = 150, AutoSize = true };
        btnLoadHim.Click += (_, _) => LoadCatalog(
            "Host Interface Manual (*.pdf)|*.pdf|All files (*.*)|*.*");
        btnLoadDefs.Click += (_, _) => LoadCatalog(
            "HIM definitions (HIMdefinitions_*.txt)|HIMdefinitions_*.txt|Text files (*.txt)|*.txt|All files (*.*)|*.*");

        var catalogButtons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(3, 3, 3, 3),
            WrapContents = false,
        };
        catalogButtons.Controls.Add(btnLoadHim);
        catalogButtons.Controls.Add(btnLoadDefs);
        AddRow(layout, string.Empty, catalogButtons);
        AddRow(layout, string.Empty, _chkRemember);

        _txtHost.Width = 200;
        foreach (var tb in new[] { _txtSendingApp, _txtSendingFacility, _txtReceivingApp, _txtReceivingFacility, _txtVersion })
        {
            tb.Width = 200;
        }

        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
        btnOk.Click += (_, _) => SaveValues();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12),
            Height = 56,
        };
        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnOk);

        Controls.Add(layout);
        Controls.Add(buttons);

        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }

    private static void AddRow(TableLayoutPanel layout, string label, Control input)
    {
        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 8, 3, 3),
        });
        input.Anchor = AnchorStyles.Left;
        layout.Controls.Add(input);
    }

    private static void AddSectionHeader(TableLayoutPanel layout, string text)
    {
        var header = new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 14, 3, 3),
        };
        layout.Controls.Add(header);
        layout.SetColumnSpan(header, 2);
        // Spacer cell to keep the two-column grid aligned after a spanned row.
        layout.Controls.Add(new Label { AutoSize = true, Margin = Padding.Empty });
    }

    private void LoadCatalog(string filter)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select HIM or definitions file",
            Filter = filter,
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            var manual = HimSettingsImporter.LoadManual(dialog.FileName);
            var summary = HimSettingsImporter.Apply(_settings, manual);
            CatalogChanged = true;
            UpdateCatalogLabel(Path.GetFileName(dialog.FileName), summary);

            // Opt-in persistence: remember the catalog across restarts, or
            // clear any previously remembered one when the box is unticked.
            if (_chkRemember.Checked)
            {
                HimCatalogPersistence.Save(manual);
            }
            else
            {
                HimCatalogPersistence.Clear();
            }

            MessageBox.Show(this, summary.ToString(), "Catalog imported",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Could not load the catalog:\n\n{ex.Message}",
                "Import failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void UpdateCatalogLabel(string fileName, HimImportSummary summary) =>
        _lblCatalog.Text =
            $"{fileName} (HIM v{summary.ManualVersion}, {summary.TestTypeCount} tests)";

    private void LoadValues()
    {
        _txtHost.Text = _connection.LisHost;
        _numLisPort.Value = Clamp(_connection.LisPort);
        _numListenPort.Value = Clamp(_connection.ListenPort);
        _txtSendingApp.Text = _connection.SendingApplication;
        _txtSendingFacility.Text = _connection.SendingFacility;
        _txtReceivingApp.Text = _connection.ReceivingApplication;
        _txtReceivingFacility.Text = _connection.ReceivingFacility;
        _txtVersion.Text = _connection.Hl7Version;

        _lblCatalog.Text = _settings.TestTypes.Count > 0
            ? $"{_settings.TestTypes.Count} tests, {_settings.SampleTypes.Count} sample types"
            : "(none loaded)";

        // Reflect whether a catalog is currently remembered across restarts.
        _chkRemember.Checked = HimCatalogPersistence.Exists();
    }

    private void SaveValues()
    {
        // If the user unticked "remember" without re-importing, stop persisting
        // the previously remembered catalog so it won't reload next start.
        if (!_chkRemember.Checked && HimCatalogPersistence.Exists())
        {
            HimCatalogPersistence.Clear();
        }

        _connection.LisHost = _txtHost.Text.Trim();
        _connection.LisPort = (int)_numLisPort.Value;
        _connection.ListenPort = (int)_numListenPort.Value;
        _connection.SendingApplication = _txtSendingApp.Text.Trim();
        _connection.SendingFacility = _txtSendingFacility.Text.Trim();
        _connection.ReceivingApplication = _txtReceivingApp.Text.Trim();
        _connection.ReceivingFacility = _txtReceivingFacility.Text.Trim();
        _connection.Hl7Version = _txtVersion.Text.Trim();
    }

    private static decimal Clamp(int port) => Math.Min(65535, Math.Max(1, port));
}
