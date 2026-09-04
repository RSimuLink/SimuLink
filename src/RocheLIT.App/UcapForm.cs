using RocheLIT.HL7.Law;
using RocheLIT.Logging;
using RocheLIT.Models;
using RocheLIT.Models.Orders;
using RocheLIT.Models.Ucap;
using RocheLIT.Services;

namespace RocheLIT;

internal sealed class UcapForm : Form
{
    private readonly ConnectionSettings _settings;
    private readonly Func<ConnectionState> _connectionState;
    private readonly Func<string, Task<string>> _sendResultAsync;
    private readonly ActivityLog _log;
    private readonly List<UcapTargetResult> _targetResults = new()
    {
        new UcapTargetResult { TargetName = "Target1", ResultValue = "Reactive" },
    };

    private ReceivedOrder? _lastReceivedOrder;

    private readonly TextBox _txtSampleId = new();
    private readonly TextBox _txtTestTypeSuffix = new();
    private readonly TextBox _txtUsid = new();
    private readonly TextBox _txtResultsSummary = new();
    private readonly Button _btnSelectResults = new();
    private readonly ComboBox _cmbSampleType = new();
    private readonly ComboBox _cmbSampleVolume = new();
    private readonly Button _btnSend = new();

    private readonly TextBox _txtRecvSampleId = new();
    private readonly TextBox _txtReceivedTestType = new();
    private readonly TextBox _txtReceivedSampleType = new();
    private readonly TextBox _txtReceivedSampleVolume = new();
    private readonly TextBox _txtReceivedRackId = new();
    private readonly TextBox _txtReceivedCarrierPosition = new();
    private readonly DataGridView _gridOrders = new();

    public UcapForm(
        ConnectionSettings settings,
        Func<ConnectionState> connectionState,
        Func<string, Task<string>> sendResultAsync,
        ActivityLog log,
        ReceivedOrder? currentOrder)
    {
        _settings = settings;
        _connectionState = connectionState;
        _sendResultAsync = sendResultAsync;
        _log = log;
        _lastReceivedOrder = currentOrder;

        BuildLayout();
        BindSampleTypes();
        UpdateResultsSummary();
        ShowOrder(currentOrder);
        SetConnectionState(connectionState());
    }

    public void SetConnectionState(ConnectionState state) =>
        _btnSend.Enabled = state == ConnectionState.Connected;

    public void ShowOrder(ReceivedOrder? order)
    {
        _lastReceivedOrder = order;
        _txtRecvSampleId.Text = order?.SampleId ?? string.Empty;
        _txtReceivedTestType.Text = order?.TestType ?? string.Empty;
        _txtReceivedSampleType.Text = order?.SampleType ?? string.Empty;
        _txtReceivedSampleVolume.Text = order?.SampleVolume ?? string.Empty;
        _txtReceivedRackId.Text = order?.CarrierId ?? string.Empty;
        _txtReceivedCarrierPosition.Text = order?.CarrierPosition ?? string.Empty;

        _gridOrders.Rows.Clear();
        if (order is null)
        {
            return;
        }

        foreach (var test in order.Tests)
        {
            _gridOrders.Rows.Add(test.TestCode, test.TestName, test.Priority);
        }
    }

    private void BuildLayout()
    {
        Text = "UCAP";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(1092, 640);
        ClientSize = new Size(1080, 600);
        Font = new Font("Segoe UI", 9F);

        var grpSend = new GroupBox
        {
            Text = "Send Test Results to LIS",
            Location = new Point(12, 12),
            Size = new Size(480, 420),
        };

        AddLabel(grpSend, "Sample ID:", 16, 36);
        _txtSampleId.Location = new Point(150, 33);
        _txtSampleId.Size = new Size(300, 27);
        grpSend.Controls.Add(_txtSampleId);

        AddLabel(grpSend, "Test Type:", 16, 72);
        AddLabel(grpSend, "U_", 150, 72);
        _txtTestTypeSuffix.Location = new Point(176, 69);
        _txtTestTypeSuffix.Size = new Size(86, 27);
        _txtTestTypeSuffix.MaxLength = 14;
        _txtTestTypeSuffix.TextChanged += (_, _) => KeepAllowedCharacters(_txtTestTypeSuffix, char.IsLetterOrDigit);
        grpSend.Controls.Add(_txtTestTypeSuffix);

        AddLabel(grpSend, "USID:", 272, 72);
        _txtUsid.Location = new Point(315, 69);
        _txtUsid.Size = new Size(64, 27);
        _txtUsid.MaxLength = 7;
        _txtUsid.TextChanged += (_, _) => KeepAllowedCharacters(_txtUsid, char.IsDigit);
        grpSend.Controls.Add(_txtUsid);
        AddLabel(grpSend, "^UCAP^99ROC", 384, 72);

        AddLabel(grpSend, "Results:", 16, 108);
        _txtResultsSummary.Location = new Point(150, 105);
        _txtResultsSummary.Size = new Size(214, 27);
        _txtResultsSummary.ReadOnly = true;
        grpSend.Controls.Add(_txtResultsSummary);

        _btnSelectResults.Location = new Point(372, 105);
        _btnSelectResults.Size = new Size(78, 27);
        _btnSelectResults.Text = "Select...";
        _btnSelectResults.Click += btnSelectResults_Click;
        grpSend.Controls.Add(_btnSelectResults);

        AddLabel(grpSend, "Sample Type:", 16, 144);
        _cmbSampleType.Location = new Point(150, 141);
        _cmbSampleType.Size = new Size(300, 27);
        _cmbSampleType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbSampleType.DisplayMember = nameof(SampleType.DisplayName);
        _cmbSampleType.SelectedIndexChanged += cmbSampleType_SelectedIndexChanged;
        grpSend.Controls.Add(_cmbSampleType);

        AddLabel(grpSend, "Sample Volume:", 16, 180);
        _cmbSampleVolume.Location = new Point(150, 177);
        _cmbSampleVolume.Size = new Size(300, 27);
        _cmbSampleVolume.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbSampleVolume.DisplayMember = nameof(SampleVolume.Volume);
        grpSend.Controls.Add(_cmbSampleVolume);

        _btnSend.BackColor = Color.FromArgb(0, 102, 204);
        _btnSend.ForeColor = Color.White;
        _btnSend.FlatStyle = FlatStyle.Flat;
        _btnSend.Location = new Point(150, 240);
        _btnSend.Size = new Size(300, 40);
        _btnSend.Text = "Send Results to LIS";
        _btnSend.UseVisualStyleBackColor = false;
        _btnSend.Click += btnSend_Click;
        grpSend.Controls.Add(_btnSend);

        var grpReceived = BuildReceivedOrderGroup();

        Controls.Add(grpSend);
        Controls.Add(grpReceived);
    }

    private GroupBox BuildReceivedOrderGroup()
    {
        var grpReceived = new GroupBox
        {
            Text = "Received LIS Order Details",
            Location = new Point(504, 12),
            Size = new Size(560, 420),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        AddLabel(grpReceived, "Sample ID:", 16, 36);
        ConfigureReadonlyText(_txtRecvSampleId, 150, 33, 220);
        grpReceived.Controls.Add(_txtRecvSampleId);

        AddLabel(grpReceived, "Test Type:", 16, 72);
        ConfigureReadonlyText(_txtReceivedTestType, 150, 69, 390);
        grpReceived.Controls.Add(_txtReceivedTestType);

        AddLabel(grpReceived, "Sample Type:", 16, 108);
        ConfigureReadonlyText(_txtReceivedSampleType, 150, 105, 220);
        grpReceived.Controls.Add(_txtReceivedSampleType);

        AddLabel(grpReceived, "Sample Volume:", 16, 144);
        ConfigureReadonlyText(_txtReceivedSampleVolume, 150, 141, 220);
        grpReceived.Controls.Add(_txtReceivedSampleVolume);

        AddLabel(grpReceived, "Rack ID:", 16, 180);
        ConfigureReadonlyText(_txtReceivedRackId, 150, 177, 220);
        grpReceived.Controls.Add(_txtReceivedRackId);

        AddLabel(grpReceived, "Position in carrier:", 16, 216);
        ConfigureReadonlyText(_txtReceivedCarrierPosition, 150, 213, 220);
        grpReceived.Controls.Add(_txtReceivedCarrierPosition);

        _gridOrders.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Test Code", Name = "colTestCode" },
            new DataGridViewTextBoxColumn { HeaderText = "Test Name", Name = "colTestName" },
            new DataGridViewTextBoxColumn { HeaderText = "Priority", Name = "colPriority" });
        _gridOrders.Location = new Point(16, 252);
        _gridOrders.Size = new Size(524, 152);
        _gridOrders.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _gridOrders.AllowUserToAddRows = false;
        _gridOrders.ReadOnly = true;
        _gridOrders.RowHeadersVisible = false;
        _gridOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grpReceived.Controls.Add(_gridOrders);

        return grpReceived;
    }

    private void BindSampleTypes()
    {
        _cmbSampleType.DataSource = UcapCatalog.SampleTypes();
        _cmbSampleType.SelectedIndex = -1;
        _cmbSampleVolume.DataSource = null;
        _cmbSampleVolume.Enabled = false;
    }

    private void cmbSampleType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var sampleType = _cmbSampleType.SelectedItem as SampleType;
        _cmbSampleVolume.DataSource = null;
        _cmbSampleVolume.DataSource = sampleType?.AllowedVolumes.ToList() ?? new List<SampleVolume>();
        _cmbSampleVolume.Enabled = sampleType is not null && sampleType.AllowedVolumes.Count > 0;
    }

    private void btnSelectResults_Click(object? sender, EventArgs e)
    {
        using var dialog = new UcapTargetResultsForm(_targetResults);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _targetResults.Clear();
        _targetResults.AddRange(dialog.TargetResults);
        UpdateResultsSummary();
    }

    private async void btnSend_Click(object? sender, EventArgs e)
    {
        if (_connectionState() != ConnectionState.Connected)
        {
            MessageBox.Show(this, "Connect to a LIS first.", "Not connected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var sampleType = _cmbSampleType.SelectedItem as SampleType;
        if (sampleType is null)
        {
            MessageBox.Show(this, "Please complete the UCAP result fields.", "Missing data",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var request = new UcapResultRequest
        {
            SampleId = _txtSampleId.Text.Trim(),
            TestNameSuffix = _txtTestTypeSuffix.Text.Trim(),
            UniversalServiceId = _txtUsid.Text.Trim(),
            SampleType = sampleType,
            SampleVolume = _cmbSampleVolume.SelectedItem?.ToString() ?? string.Empty,
            Targets = _targetResults.ToList(),
        };
        ApplyReceivedOrderContext(request);

        try
        {
            var result = LawUcapResultMessageFactory.Create(request, _settings);
            var message = LawOulR22Builder.Build(result);
            await _sendResultAsync(message.RawMessage);
            _log.Success(
                $"UCAP results sent to LIS: Sample ID {request.SampleId}, Test Type U_{request.TestNameSuffix}, USID {request.UniversalServiceId}");
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to send UCAP results: {ex.Message}");
            MessageBox.Show(this, $"Could not send UCAP results:\n{ex.Message}",
                "UCAP send failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplyReceivedOrderContext(UcapResultRequest request)
    {
        if (_lastReceivedOrder is null || !SameSample(_lastReceivedOrder.SampleId, request.SampleId))
        {
            return;
        }

        request.RackId = _lastReceivedOrder.CarrierId;
        request.CarrierPosition = _lastReceivedOrder.CarrierPosition;
    }

    private void UpdateResultsSummary()
    {
        _txtResultsSummary.Text = string.Join("; ", _targetResults
            .Select(t => $"{t.TargetName}: {t.ResultValue}"));
    }

    private static bool SameSample(string receivedSampleId, string sentSampleId)
    {
        var received = receivedSampleId.Split('&', 2)[0].Trim();
        return string.Equals(received, sentSampleId.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static void AddLabel(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(x, y),
            Text = text,
        });
    }

    private static void ConfigureReadonlyText(TextBox textBox, int x, int y, int width)
    {
        textBox.Location = new Point(x, y);
        textBox.Size = new Size(width, 27);
        textBox.ReadOnly = true;
    }

    private static void KeepAllowedCharacters(TextBox textBox, Func<char, bool> allowed)
    {
        var original = textBox.Text;
        var cleaned = new string(original.Where(allowed).ToArray());
        if (cleaned == original)
        {
            return;
        }

        var selectionStart = Math.Min(textBox.SelectionStart, cleaned.Length);
        textBox.Text = cleaned;
        textBox.SelectionStart = selectionStart;
    }
}
