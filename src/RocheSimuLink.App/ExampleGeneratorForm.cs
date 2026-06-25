using RocheSimuLink.Models;
using RocheSimuLink.Models.Workflows;
using RocheSimuLink.Services;

namespace RocheSimuLink;

/// <summary>
/// Lets the user pick any of the LAB-27/28/29 workflows and generates the
/// corresponding HL7 messages from the values currently typed in the main UI,
/// with no LIS connection required. Messages can be viewed, copied, and saved.
/// </summary>
public sealed class ExampleGeneratorForm : Form
{
    private readonly ExampleGeneratorInput _input;
    private readonly ConnectionSettings _connection;

    private readonly CheckedListBox _workflowList = new();
    private readonly Button _selectAll = new();
    private readonly Button _generate = new();
    private readonly Button _copy = new();
    private readonly Button _save = new();
    private readonly TextBox _output = new();

    public ExampleGeneratorForm(ExampleGeneratorInput input, ConnectionSettings connection)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        BuildLayout();
        PopulateWorkflows();
    }

    private void BuildLayout()
    {
        Text = "Example Generator";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(720, 520);
        ClientSize = new Size(860, 600);
        Font = new Font("Segoe UI", 9F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // intro
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170)); // workflow list
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // buttons
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // output

        var intro = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6),
            Text = "Select workflows to generate HL7 messages from the values entered in the " +
                   "main window. No LIS connection is required.",
        };
        root.Controls.Add(intro, 0, 0);

        // Workflow checkboxes.
        var listGroup = new GroupBox
        {
            Text = "Workflows",
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
        };
        _workflowList.Dock = DockStyle.Fill;
        _workflowList.CheckOnClick = true;
        _workflowList.IntegralHeight = false;
        listGroup.Controls.Add(_workflowList);
        root.Controls.Add(listGroup, 0, 1);

        // Action buttons.
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 8),
            FlowDirection = FlowDirection.LeftToRight,
        };
        _selectAll.Text = "Select All";
        _selectAll.AutoSize = true;
        _selectAll.Click += (_, _) => SetAllChecked(!AllChecked());

        _generate.Text = "Generate";
        _generate.AutoSize = true;
        _generate.Click += (_, _) => GenerateOutput();

        _copy.Text = "Copy All";
        _copy.AutoSize = true;
        _copy.Enabled = false;
        _copy.Click += (_, _) => CopyOutput();

        _save.Text = "Save to File...";
        _save.AutoSize = true;
        _save.Enabled = false;
        _save.Click += (_, _) => SaveOutput();

        buttons.Controls.AddRange(new Control[] { _selectAll, _generate, _copy, _save });
        root.Controls.Add(buttons, 0, 2);

        // Output.
        _output.Multiline = true;
        _output.ReadOnly = true;
        _output.ScrollBars = ScrollBars.Both;
        _output.WordWrap = false;
        _output.Dock = DockStyle.Fill;
        _output.Font = new Font(FontFamily.GenericMonospace, 9F);
        root.Controls.Add(_output, 0, 3);

        Controls.Add(root);
        AcceptButton = _generate;
    }

    private void PopulateWorkflows()
    {
        foreach (var info in ExampleWorkflowInfo.All)
        {
            // All flows checked by default for a quick "generate everything".
            _workflowList.Items.Add(new WorkflowItem(info), isChecked: true);
        }
    }

    private bool AllChecked() =>
        _workflowList.CheckedItems.Count == _workflowList.Items.Count;

    private void SetAllChecked(bool value)
    {
        for (var i = 0; i < _workflowList.Items.Count; i++)
        {
            _workflowList.SetItemChecked(i, value);
        }
    }

    private void GenerateOutput()
    {
        var selected = _workflowList.CheckedItems
            .Cast<WorkflowItem>()
            .Select(i => i.Info.Workflow)
            .ToList();

        if (selected.Count == 0)
        {
            MessageBox.Show(this, "Select at least one workflow.", "Nothing selected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var generator = new ExampleGenerator(_connection);
            var messages = generator.Generate(_input, selected);
            _output.Text = ExampleGenerator.Render(messages);

            var hasOutput = _output.TextLength > 0;
            _copy.Enabled = hasOutput;
            _save.Enabled = hasOutput;
            _output.Select(0, 0);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not generate messages:\n{ex.Message}",
                "Generation failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CopyOutput()
    {
        if (_output.TextLength == 0)
        {
            return;
        }

        Clipboard.SetText(_output.Text);
    }

    private void SaveOutput()
    {
        if (_output.TextLength == 0)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Save generated HL7 messages",
            Filter = "HL7 file (*.hl7)|*.hl7|Text file (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = "example-messages.hl7",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(dialog.FileName, _output.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not save file:\n{ex.Message}",
                    "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    /// <summary>List item wrapping a workflow so the caption shows in the list.</summary>
    private sealed class WorkflowItem
    {
        public WorkflowItem(ExampleWorkflowInfo info) => Info = info;

        public ExampleWorkflowInfo Info { get; }

        public override string ToString() => Info.Caption;
    }
}
