using RocheLIT.HL7.Law;
using RocheLIT.Logging;
using RocheLIT.Models;
using RocheLIT.Models.Law;
using RocheLIT.Models.Orders;
using RocheLIT.Models.Workflows;
using RocheLIT.Services;

namespace RocheLIT;

public partial class MainForm : Form
{
    private readonly LitSettings _settings;
    private readonly ActivityLog _log = new();
    private LisConnectionService? _connection;
    private ReceivedOrder? _lastReceivedOrder;

    public MainForm()
    {
        InitializeComponent();

        _settings = SettingsLoader.Load();
        _log.EntryAdded += (_, entry) => RunOnUi(() => AppendLog(entry));

        LoadUiData();
    }

    /// <summary>
    /// Loads the Roche brand logo (Assets/roche-logo.png) shown on the right of
    /// the toolbar.
    /// </summary>
    private static Image? LoadBrandLogo() => AppAssets.LoadImage("roche-logo.png");

    /// <summary>
    /// Loads the LIT product logo (Assets/LITLogo.jpg) shown on the
    /// left of the toolbar.
    /// </summary>
    private static Image? LoadAppLogo() => AppAssets.LoadImage("LITLogo.jpg");

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
        var status = (ResultStatus)(cmbResultStatus.SelectedItem ?? ResultStatus.Final);

        var resultMessage = LawResultMessageFactory.Create(
            sampleId,
            sampleType!,
            test,
            target,
            value,
            ResultFlag.Normal,
            status,
            _settings.Connection,
            sampleVolume: cmbSampleVolume.SelectedItem?.ToString() ?? string.Empty);
        ApplyReceivedOrderContext(resultMessage, sampleId);
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

    // --- Example Generator --------------------------------------------------

    private void exampleGeneratorMenuItem_Click(object? sender, EventArgs e)
    {
        // No LIS connection needed — the generator only formats messages from
        // whatever is currently typed into the result-entry fields.
        var input = BuildExampleInput();
        using var dialog = new ExampleGeneratorForm(input, _settings.Connection);
        dialog.ShowDialog(this);
    }

    /// <summary>
    /// Snapshots the current result-entry fields into the generator input. Uses
    /// the same selections as <see cref="btnSendResult_Click"/> so generated
    /// examples match what would be sent on the wire.
    /// </summary>
    private ExampleGeneratorInput BuildExampleInput()
    {
        var test = cmbTestType.SelectedItem as TestType ?? new TestType();
        var sampleType = cmbSampleType.SelectedItem as SampleType ?? new SampleType();
        var value = ResultEntryPresenter.EffectiveResultValue(
            test, cmbResult.SelectedItem?.ToString());

        return new ExampleGeneratorInput
        {
            SampleId = txtSampleId.Text.Trim(),
            Test = test,
            Target = test.Targets.Count > 0 ? test.Targets[0] : null,
            SampleType = sampleType,
            SampleVolume = cmbSampleVolume.SelectedItem?.ToString() ?? string.Empty,
            ResultValue = value,
            ResultStatus = (ResultStatus)(cmbResultStatus.SelectedItem ?? ResultStatus.Final),
            ResultFlag = ResultFlag.Normal,
        };
    }

    // --- Receiving orders ---------------------------------------------------

    private void ShowOrder(ReceivedOrder order)
    {
        _lastReceivedOrder = order;
        txtOrderNumber.Text = order.OrderNumber;
        txtRecvSampleId.Text = order.SampleId;
        txtReceivedTestType.Text = order.TestType;
        txtReceivedSampleType.Text = order.SampleType;
        txtReceivedSampleVolume.Text = order.SampleVolume;

        gridOrders.Rows.Clear();
        foreach (var test in order.Tests)
        {
            gridOrders.Rows.Add(test.TestCode, test.TestName, test.Priority);
        }
    }

    private void ApplyReceivedOrderContext(LawResultMessage resultMessage, string sampleId)
    {
        if (_lastReceivedOrder is null || !SameSample(_lastReceivedOrder.SampleId, sampleId))
        {
            return;
        }

        resultMessage.Specimen.CarrierId = _lastReceivedOrder.CarrierId;
        resultMessage.Specimen.CarrierPosition = _lastReceivedOrder.CarrierPosition;
    }

    private static bool SameSample(string receivedSampleId, string sentSampleId)
    {
        var received = receivedSampleId.Split('&', 2)[0].Trim();
        return string.Equals(received, sentSampleId.Trim(), StringComparison.OrdinalIgnoreCase);
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
