using RocheSimuLink.HL7.Law;
using RocheSimuLink.Logging;
using RocheSimuLink.Models;
using RocheSimuLink.Models.Orders;
using RocheSimuLink.Services;

namespace RocheSimuLink;

public partial class MainForm : Form
{
    private readonly SimuLinkSettings _settings;
    private readonly ActivityLog _log = new();
    private LisConnectionService? _connection;

    public MainForm()
    {
        InitializeComponent();

        _settings = SettingsLoader.Load();
        _log.EntryAdded += (_, entry) => RunOnUi(() => AppendLog(entry));

        LoadUiData();
    }

    private void LoadUiData()
    {
        BindCatalog();

        cmbResultStatus.DataSource = Enum.GetValues<ResultStatus>();
        cmbResultStatus.SelectedItem = ResultStatus.Final;

        PopulateResults();
    }

    /// <summary>
    /// (Re)binds the catalog-driven dropdowns to the current settings lists.
    /// Called on startup and after a HIM/definitions import replaces the lists.
    /// </summary>
    private void BindCatalog()
    {
        cmbTestType.DisplayMember = nameof(TestType.Name);
        cmbTestType.DataSource = null;
        cmbTestType.DataSource = _settings.TestTypes;

        cmbSampleType.DisplayMember = nameof(SampleType.DisplayName);
        cmbSampleType.DataSource = null;
        cmbSampleType.DataSource = _settings.SampleTypes;

        cmbSampleVolume.DisplayMember = nameof(SampleVolume.Volume);
        cmbSampleVolume.DataSource = null;
        cmbSampleVolume.DataSource = _settings.SampleVolumes;

        PopulateResults();
    }

    private void cmbTestType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var test = cmbTestType.SelectedItem as TestType;
        cmbSampleVolume.DataSource =
            ResultEntryPresenter.VolumesFor(test, _settings.SampleVolumes).ToList();

        PopulateResults();
    }

    private void PopulateResults()
    {
        var test = cmbTestType.SelectedItem as TestType;
        cmbResult.DataSource = ResultEntryPresenter.ResultValuesFor(test).ToList();
    }

    // --- Connection ---------------------------------------------------------

    private async void btnConnect_Click(object? sender, EventArgs e)
    {
        _connection = new LisConnectionService(_settings.Connection, _log);
        _connection.OrderReceived += (_, order) => RunOnUi(() => ShowOrder(order));
        _connection.StateChanged += (_, state) => RunOnUi(() => ApplyConnectionState(state));

        try
        {
            btnConnect.Enabled = false;
            await _connection.ConnectAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not connect to LIS:\n{ex.Message}", "Connection failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            btnConnect.Enabled = true;
        }
    }

    private async void btnDisconnect_Click(object? sender, EventArgs e)
    {
        if (_connection is not null)
        {
            await _connection.DisconnectAsync();
            _connection.Dispose();
            _connection = null;
        }
    }

    private void ApplyConnectionState(ConnectionState state)
    {
        var connected = state == ConnectionState.Connected;
        btnConnect.Enabled = state == ConnectionState.Disconnected;
        btnDisconnect.Enabled = connected;
        btnSendResult.Enabled = connected;
    }

    private void btnSettings_Click(object? sender, EventArgs e)
    {
        using var dialog = new SettingsForm(_settings);
        var result = dialog.ShowDialog(this);

        // A catalog import mutates the settings lists immediately (even on
        // Cancel), so rebind the dropdowns whenever it changed.
        if (dialog.CatalogChanged)
        {
            BindCatalog();
            _log.Info($"Assay catalog loaded: {_settings.TestTypes.Count} tests, " +
                $"{_settings.SampleTypes.Count} sample types.");
        }

        if (result == DialogResult.OK)
        {
            _log.Info("Settings updated.");
        }
    }

    // --- Sending results ----------------------------------------------------

    private async void btnSendResult_Click(object? sender, EventArgs e)
    {
        if (_connection is null || _connection.State != ConnectionState.Connected)
        {
            MessageBox.Show("Connect to a LIS first.", "Not connected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var test = cmbTestType.SelectedItem as TestType;
        var sampleType = cmbSampleType.SelectedItem as SampleType;
        if (!ResultEntryPresenter.CanSend(test, sampleType))
        {
            MessageBox.Show("Please complete the result fields.", "Missing data",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var sampleId = txtSampleId.Text.Trim();
        var target = test!.Targets[0];
        var value = ResultEntryPresenter.EffectiveResultValue(
            test, cmbResult.SelectedItem?.ToString());
        var flag = SelectedFlag();
        var status = (ResultStatus)(cmbResultStatus.SelectedItem ?? ResultStatus.Final);

        var resultMessage = LawResultMessageFactory.Create(
            sampleId, sampleType!, test, target, value, flag, status, _settings.Connection);
        var message = LawOulR22Builder.Build(resultMessage);

        try
        {
            var ack = await _connection.SendResultAsync(message.RawMessage);
            _log.Success(
                $"Test results sent to LIS: Sample ID {sampleId}, Result: {test.Name} {value}, " +
                $"{sampleType!.DisplayName}, {cmbSampleVolume.SelectedItem}");
            _ = ack; // ACK already implies success; surface details if needed later.
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to send results: {ex.Message}");
        }
    }

    private ResultFlag SelectedFlag() =>
        ResultEntryPresenter.ResolveFlag(
            chkCritical.Checked, chkHigh.Checked, chkLow.Checked);

    // --- Receiving orders ---------------------------------------------------

    private void ShowOrder(ReceivedOrder order)
    {
        txtOrderNumber.Text = order.OrderNumber;
        txtRecvSampleId.Text = order.SampleId;
        txtPatientName.Text = order.Patient.FullName;
        txtDob.Text = order.Patient.DateOfBirth?.ToString("MM/dd/yyyy") ?? string.Empty;
        txtSex.Text = order.Patient.Sex;

        gridOrders.Rows.Clear();
        foreach (var test in order.Tests)
        {
            gridOrders.Rows.Add(test.TestCode, test.TestName, test.Priority);
        }
    }

    // --- Activity log -------------------------------------------------------

    private void AppendLog(ActivityLogEntry entry)
    {
        var item = new ListViewItem(entry.ToString())
        {
            ForeColor = entry.Severity switch
            {
                LogSeverity.Success => Color.Green,
                LogSeverity.Warning => Color.DarkGoldenrod,
                LogSeverity.Error => Color.Firebrick,
                _ => Color.Black,
            },
        };
        lstLog.Items.Add(item);
        lstLog.Columns[0].Width = -2;
        item.EnsureVisible();
    }

    private void RunOnUi(Action action)
    {
        if (IsHandleCreated && InvokeRequired)
        {
            BeginInvoke(action);
        }
        else
        {
            action();
        }
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        if (_connection is not null)
        {
            await _connection.DisconnectAsync();
            _connection.Dispose();
        }

        base.OnFormClosing(e);
    }
}
