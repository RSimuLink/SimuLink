using RocheLIT.Models.Ucap;

namespace RocheLIT;

internal sealed class UcapTargetResultsForm : Form
{
    private const int IncludeColumn = 0;
    private const int TargetColumn = 1;
    private const int ResultColumn = 2;

    private static readonly string[] ResultValues = { "Reactive", "Non-Reactive" };

    private readonly DataGridView _grid = new();
    private readonly Button _btnOk = new();

    public IReadOnlyList<UcapTargetResult> TargetResults { get; private set; } =
        Array.Empty<UcapTargetResult>();

    public UcapTargetResultsForm(IReadOnlyList<UcapTargetResult> currentResults)
    {
        InitializeComponent();
        PopulateRows(currentResults);
    }

    private void InitializeComponent()
    {
        Text = "UCAP Results";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(500, 260);

        _grid.Location = new Point(12, 12);
        _grid.Size = new Size(476, 190);
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;
        _grid.DataError += (_, e) => e.ThrowException = false;
        _grid.CurrentCellDirtyStateChanged += grid_CurrentCellDirtyStateChanged;
        _grid.CellValueChanged += grid_CellValueChanged;
        _grid.CellEndEdit += grid_CellEndEdit;
        _grid.Columns.AddRange(
            new DataGridViewCheckBoxColumn
            {
                HeaderText = "",
                Name = "colInclude",
                FillWeight = 32,
            },
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Target",
                Name = "colTarget",
                FillWeight = 130,
            },
            new DataGridViewComboBoxColumn
            {
                HeaderText = "Observation value",
                Name = "colResult",
                DataSource = ResultValues,
                FillWeight = 130,
            });

        _btnOk.Location = new Point(413, 218);
        _btnOk.Size = new Size(75, 30);
        _btnOk.Text = "OK";
        _btnOk.DialogResult = DialogResult.None;
        _btnOk.Click += btnOk_Click;

        AcceptButton = _btnOk;
        Controls.Add(_grid);
        Controls.Add(_btnOk);
    }

    private void PopulateRows(IReadOnlyList<UcapTargetResult> currentResults)
    {
        for (var i = 0; i < 4; i++)
        {
            var current = i < currentResults.Count ? currentResults[i] : null;
            var include = i == 0 || current is not null;
            var rowIndex = _grid.Rows.Add(include, current?.TargetName ?? $"Target {i + 1}",
                current?.ResultValue ?? ResultValues[0]);
            _grid.Rows[rowIndex].Cells[TargetColumn].Value = current?.TargetName ?? $"Target {i + 1}";
            _grid.Rows[rowIndex].Cells[ResultColumn].Value = current?.ResultValue ?? ResultValues[0];
        }

        ApplyRowAvailability();
    }

    private void grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (_grid.IsCurrentCellDirty)
        {
            _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            ApplyRowAvailability();
        }
    }

    private void grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != TargetColumn)
        {
            return;
        }

        var cell = _grid.Rows[e.RowIndex].Cells[TargetColumn];
        var cleaned = CleanAlphanumeric(cell.Value?.ToString() ?? string.Empty, 15);
        cell.Value = cleaned.Length == 0 ? $"Target {e.RowIndex + 1}" : cleaned;
        ApplyRowAvailability();
    }

    private void ApplyRowAvailability()
    {
        for (var rowIndex = 0; rowIndex < _grid.Rows.Count; rowIndex++)
        {
            var row = _grid.Rows[rowIndex];
            var previousComplete = rowIndex == 0 || RowIncluded(rowIndex - 1) && TargetName(rowIndex - 1).Length > 0;
            if (!previousComplete)
            {
                row.Cells[IncludeColumn].Value = false;
            }

            var included = rowIndex == 0 || RowIncluded(rowIndex);
            row.Cells[IncludeColumn].ReadOnly = rowIndex == 0 || !previousComplete;
            row.Cells[TargetColumn].ReadOnly = !included || !previousComplete;
            row.Cells[ResultColumn].ReadOnly = !included || !previousComplete;
            row.DefaultCellStyle.ForeColor = included && previousComplete ? SystemColors.ControlText : SystemColors.GrayText;
        }
    }

    private bool RowIncluded(int rowIndex) =>
        Convert.ToBoolean(_grid.Rows[rowIndex].Cells[IncludeColumn].Value ?? false);

    private string TargetName(int rowIndex) =>
        (_grid.Rows[rowIndex].Cells[TargetColumn].Value?.ToString() ?? string.Empty).Trim();

    private void btnOk_Click(object? sender, EventArgs e)
    {
        if (_grid.IsCurrentCellDirty)
        {
            _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        _grid.EndEdit();
        ApplyRowAvailability();

        var results = new List<UcapTargetResult>();
        for (var i = 0; i < _grid.Rows.Count; i++)
        {
            if (i > 0 && !RowIncluded(i))
            {
                continue;
            }

            if (i > 0 && (!RowIncluded(i - 1) || TargetName(i - 1).Length == 0))
            {
                MessageBox.Show(this, "Enable UCAP targets in sequence.", "Missing target",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var name = CleanAlphanumeric(TargetName(i), 15);
            if (name.Length == 0)
            {
                MessageBox.Show(this, "Each enabled target needs a name.", "Missing target",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var value = _grid.Rows[i].Cells[ResultColumn].Value?.ToString() ?? string.Empty;
            if (!ResultValues.Contains(value, StringComparer.Ordinal))
            {
                MessageBox.Show(this, "Each enabled target needs a result.", "Missing result",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            results.Add(new UcapTargetResult
            {
                TargetName = name,
                ResultValue = value,
            });
        }

        TargetResults = results;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static string CleanAlphanumeric(string value, int maxLength)
    {
        var cleaned = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
    }
}
