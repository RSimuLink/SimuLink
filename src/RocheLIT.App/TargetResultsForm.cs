using RocheLIT.Models;
using RocheLIT.Services;

namespace RocheLIT;

internal sealed class TargetResultsForm : Form
{
    private readonly TestType _test;
    private readonly IReadOnlyDictionary<string, string> _currentResults;
    private readonly DataGridView _grid = new();
    private readonly Button _btnOk = new();

    public IReadOnlyDictionary<string, string> TargetResults { get; private set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public TargetResultsForm(TestType test, IReadOnlyDictionary<string, string> currentResults)
    {
        _test = test;
        _currentResults = currentResults;

        InitializeComponent();
        PopulateRows();
    }

    private void InitializeComponent()
    {
        Text = $"{_test.Name} Results";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(420, 250);

        _grid.Location = new Point(12, 12);
        _grid.Size = new Size(396, 180);
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.EditMode = DataGridViewEditMode.EditOnEnter;
        _grid.DataError += (_, e) => e.ThrowException = false;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn
            {
                HeaderText = "Target",
                Name = "colTarget",
                ReadOnly = true,
            },
            new DataGridViewComboBoxColumn
            {
                HeaderText = "Result",
                Name = "colResult",
            });

        _btnOk.Location = new Point(333, 207);
        _btnOk.Size = new Size(75, 30);
        _btnOk.Text = "OK";
        _btnOk.DialogResult = DialogResult.None;
        _btnOk.Click += btnOk_Click;

        AcceptButton = _btnOk;
        Controls.Add(_grid);
        Controls.Add(_btnOk);
    }

    private void PopulateRows()
    {
        foreach (var target in _test.Targets)
        {
            var values = ResultEntryPresenter.ResultValuesForTarget(target).ToList();
            if (values.Count == 0)
            {
                continue;
            }

            var rowIndex = _grid.Rows.Add();
            var row = _grid.Rows[rowIndex];
            row.Tag = target;
            row.Cells[0].Value = target.Name;
            row.Cells[1] = new DataGridViewComboBoxCell
            {
                DataSource = values,
                Value = CurrentValueFor(target, values),
            };
        }
    }

    private string CurrentValueFor(Target target, IReadOnlyList<string> values)
    {
        if (_currentResults.TryGetValue(target.ObservationIdentifier, out var value) &&
            values.Any(v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase)))
        {
            return value;
        }

        return values[0];
    }

    private void btnOk_Click(object? sender, EventArgs e)
    {
        if (_grid.IsCurrentCellDirty)
        {
            _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        _grid.EndEdit();

        var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Tag is not Target target)
            {
                continue;
            }

            var value = row.Cells[1].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Please select a result for each target.", "Missing data",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            results[target.ObservationIdentifier] = value;
        }

        TargetResults = results;
        DialogResult = DialogResult.OK;
        Close();
    }
}
