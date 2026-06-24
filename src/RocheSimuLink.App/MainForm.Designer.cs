namespace RocheSimuLink;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        // Toolbar
        pnlToolbar = new Panel();
        btnConnect = new Button();
        btnDisconnect = new Button();
        btnSettings = new Button();
        lblBrand = new Label();

        // Left: Send results
        grpSend = new GroupBox();
        lblSampleId = new Label();
        txtSampleId = new TextBox();
        lblTestType = new Label();
        cmbTestType = new ComboBox();
        lblResult = new Label();
        cmbResult = new ComboBox();
        lblSampleType = new Label();
        cmbSampleType = new ComboBox();
        lblSampleVolume = new Label();
        cmbSampleVolume = new ComboBox();
        lblResultStatus = new Label();
        cmbResultStatus = new ComboBox();
        lblFlags = new Label();
        chkNormal = new CheckBox();
        chkHigh = new CheckBox();
        chkLow = new CheckBox();
        chkCritical = new CheckBox();
        btnSendResult = new Button();

        // Right: Received order
        grpReceived = new GroupBox();
        lblOrderNumber = new Label();
        txtOrderNumber = new TextBox();
        lblRecvSampleId = new Label();
        txtRecvSampleId = new TextBox();
        lblPatientName = new Label();
        txtPatientName = new TextBox();
        lblDob = new Label();
        txtDob = new TextBox();
        lblSex = new Label();
        txtSex = new TextBox();
        gridOrders = new DataGridView();
        colTestCode = new DataGridViewTextBoxColumn();
        colTestName = new DataGridViewTextBoxColumn();
        colPriority = new DataGridViewTextBoxColumn();

        // Bottom: Activity log
        grpLog = new GroupBox();
        lstLog = new ListView();

        pnlToolbar.SuspendLayout();
        grpSend.SuspendLayout();
        grpReceived.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridOrders).BeginInit();
        grpLog.SuspendLayout();
        SuspendLayout();

        // 
        // pnlToolbar
        // 
        pnlToolbar.BackColor = Color.White;
        pnlToolbar.Controls.Add(btnConnect);
        pnlToolbar.Controls.Add(btnDisconnect);
        pnlToolbar.Controls.Add(btnSettings);
        pnlToolbar.Controls.Add(lblBrand);
        pnlToolbar.Dock = DockStyle.Top;
        pnlToolbar.Height = 56;
        pnlToolbar.Name = "pnlToolbar";

        // 
        // btnConnect
        // 
        btnConnect.BackColor = Color.FromArgb(0, 102, 204);
        btnConnect.ForeColor = Color.White;
        btnConnect.FlatStyle = FlatStyle.Flat;
        btnConnect.Location = new Point(12, 10);
        btnConnect.Size = new Size(160, 36);
        btnConnect.Name = "btnConnect";
        btnConnect.Text = "Connect to LIS";
        btnConnect.UseVisualStyleBackColor = false;
        btnConnect.Click += btnConnect_Click;

        // 
        // btnDisconnect
        // 
        btnDisconnect.Location = new Point(180, 10);
        btnDisconnect.Size = new Size(140, 36);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Text = "Disconnect";
        btnDisconnect.Enabled = false;
        btnDisconnect.Click += btnDisconnect_Click;

        // 
        // btnSettings
        // 
        btnSettings.Location = new Point(330, 10);
        btnSettings.Size = new Size(140, 36);
        btnSettings.Name = "btnSettings";
        btnSettings.Text = "⚙  Settings";
        btnSettings.Click += btnSettings_Click;

        // 
        // lblBrand
        // 
        lblBrand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblBrand.AutoSize = true;
        lblBrand.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblBrand.ForeColor = Color.FromArgb(0, 102, 204);
        lblBrand.Location = new Point(960, 14);
        lblBrand.Name = "lblBrand";
        lblBrand.Text = "Roche";

        // 
        // grpSend
        // 
        grpSend.Controls.Add(lblSampleId);
        grpSend.Controls.Add(txtSampleId);
        grpSend.Controls.Add(lblTestType);
        grpSend.Controls.Add(cmbTestType);
        grpSend.Controls.Add(lblResult);
        grpSend.Controls.Add(cmbResult);
        grpSend.Controls.Add(lblSampleType);
        grpSend.Controls.Add(cmbSampleType);
        grpSend.Controls.Add(lblSampleVolume);
        grpSend.Controls.Add(cmbSampleVolume);
        grpSend.Controls.Add(lblResultStatus);
        grpSend.Controls.Add(cmbResultStatus);
        grpSend.Controls.Add(lblFlags);
        grpSend.Controls.Add(chkNormal);
        grpSend.Controls.Add(chkHigh);
        grpSend.Controls.Add(chkLow);
        grpSend.Controls.Add(chkCritical);
        grpSend.Controls.Add(btnSendResult);
        grpSend.Location = new Point(12, 68);
        grpSend.Size = new Size(480, 360);
        grpSend.Name = "grpSend";
        grpSend.Text = "Send Test Results to LIS";

        lblSampleId.AutoSize = true;
        lblSampleId.Location = new Point(16, 36);
        lblSampleId.Text = "Sample ID:";
        txtSampleId.Location = new Point(150, 33);
        txtSampleId.Size = new Size(300, 27);
        txtSampleId.Name = "txtSampleId";

        lblTestType.AutoSize = true;
        lblTestType.Location = new Point(16, 72);
        lblTestType.Text = "Test Type:";
        cmbTestType.Location = new Point(150, 69);
        cmbTestType.Size = new Size(300, 27);
        cmbTestType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTestType.Name = "cmbTestType";
        cmbTestType.SelectedIndexChanged += cmbTestType_SelectedIndexChanged;

        lblResult.AutoSize = true;
        lblResult.Location = new Point(16, 108);
        lblResult.Text = "Result:";
        cmbResult.Location = new Point(150, 105);
        cmbResult.Size = new Size(300, 27);
        cmbResult.Name = "cmbResult";

        lblSampleType.AutoSize = true;
        lblSampleType.Location = new Point(16, 144);
        lblSampleType.Text = "Sample Type:";
        cmbSampleType.Location = new Point(150, 141);
        cmbSampleType.Size = new Size(300, 27);
        cmbSampleType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSampleType.Name = "cmbSampleType";

        lblSampleVolume.AutoSize = true;
        lblSampleVolume.Location = new Point(16, 180);
        lblSampleVolume.Text = "Sample Volume:";
        cmbSampleVolume.Location = new Point(150, 177);
        cmbSampleVolume.Size = new Size(300, 27);
        cmbSampleVolume.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSampleVolume.Name = "cmbSampleVolume";

        lblResultStatus.AutoSize = true;
        lblResultStatus.Location = new Point(16, 216);
        lblResultStatus.Text = "Result Status:";
        cmbResultStatus.Location = new Point(150, 213);
        cmbResultStatus.Size = new Size(150, 27);
        cmbResultStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbResultStatus.Name = "cmbResultStatus";

        lblFlags.AutoSize = true;
        lblFlags.Location = new Point(16, 256);
        lblFlags.Text = "Flags:";
        chkNormal.AutoSize = true;
        chkNormal.Location = new Point(150, 255);
        chkNormal.Text = "Normal";
        chkNormal.Checked = true;
        chkNormal.Name = "chkNormal";
        chkHigh.AutoSize = true;
        chkHigh.Location = new Point(235, 255);
        chkHigh.Text = "High";
        chkHigh.Name = "chkHigh";
        chkLow.AutoSize = true;
        chkLow.Location = new Point(305, 255);
        chkLow.Text = "Low";
        chkLow.Name = "chkLow";
        chkCritical.AutoSize = true;
        chkCritical.Location = new Point(370, 255);
        chkCritical.Text = "Critical";
        chkCritical.Name = "chkCritical";

        btnSendResult.BackColor = Color.FromArgb(0, 102, 204);
        btnSendResult.ForeColor = Color.White;
        btnSendResult.FlatStyle = FlatStyle.Flat;
        btnSendResult.Location = new Point(150, 300);
        btnSendResult.Size = new Size(300, 40);
        btnSendResult.Name = "btnSendResult";
        btnSendResult.Text = "Send Results to LIS";
        btnSendResult.UseVisualStyleBackColor = false;
        btnSendResult.Click += btnSendResult_Click;

        // 
        // grpReceived
        // 
        grpReceived.Controls.Add(lblOrderNumber);
        grpReceived.Controls.Add(txtOrderNumber);
        grpReceived.Controls.Add(lblRecvSampleId);
        grpReceived.Controls.Add(txtRecvSampleId);
        grpReceived.Controls.Add(lblPatientName);
        grpReceived.Controls.Add(txtPatientName);
        grpReceived.Controls.Add(lblDob);
        grpReceived.Controls.Add(txtDob);
        grpReceived.Controls.Add(lblSex);
        grpReceived.Controls.Add(txtSex);
        grpReceived.Controls.Add(gridOrders);
        grpReceived.Location = new Point(504, 68);
        grpReceived.Size = new Size(560, 360);
        grpReceived.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpReceived.Name = "grpReceived";
        grpReceived.Text = "Received LIS Order Details";

        lblOrderNumber.AutoSize = true;
        lblOrderNumber.Location = new Point(16, 36);
        lblOrderNumber.Text = "Order Number:";
        txtOrderNumber.Location = new Point(150, 33);
        txtOrderNumber.Size = new Size(220, 27);
        txtOrderNumber.ReadOnly = true;
        txtOrderNumber.Name = "txtOrderNumber";

        lblRecvSampleId.AutoSize = true;
        lblRecvSampleId.Location = new Point(16, 72);
        lblRecvSampleId.Text = "Sample ID:";
        txtRecvSampleId.Location = new Point(150, 69);
        txtRecvSampleId.Size = new Size(220, 27);
        txtRecvSampleId.ReadOnly = true;
        txtRecvSampleId.Name = "txtRecvSampleId";

        lblPatientName.AutoSize = true;
        lblPatientName.Location = new Point(16, 108);
        lblPatientName.Text = "Patient Name:";
        txtPatientName.Location = new Point(150, 105);
        txtPatientName.Size = new Size(390, 27);
        txtPatientName.ReadOnly = true;
        txtPatientName.Name = "txtPatientName";

        lblDob.AutoSize = true;
        lblDob.Location = new Point(16, 144);
        lblDob.Text = "Date of Birth:";
        txtDob.Location = new Point(150, 141);
        txtDob.Size = new Size(150, 27);
        txtDob.ReadOnly = true;
        txtDob.Name = "txtDob";

        lblSex.AutoSize = true;
        lblSex.Location = new Point(16, 180);
        lblSex.Text = "Sex:";
        txtSex.Location = new Point(150, 177);
        txtSex.Size = new Size(80, 27);
        txtSex.ReadOnly = true;
        txtSex.Name = "txtSex";

        gridOrders.Columns.AddRange(new DataGridViewColumn[] { colTestCode, colTestName, colPriority });
        gridOrders.Location = new Point(16, 216);
        gridOrders.Size = new Size(524, 128);
        gridOrders.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        gridOrders.AllowUserToAddRows = false;
        gridOrders.ReadOnly = true;
        gridOrders.RowHeadersVisible = false;
        gridOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridOrders.Name = "gridOrders";
        colTestCode.HeaderText = "Test Code";
        colTestCode.Name = "colTestCode";
        colTestName.HeaderText = "Test Name";
        colTestName.Name = "colTestName";
        colPriority.HeaderText = "Priority";
        colPriority.Name = "colPriority";

        // 
        // grpLog
        // 
        grpLog.Controls.Add(lstLog);
        grpLog.Location = new Point(12, 440);
        grpLog.Size = new Size(1052, 180);
        grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpLog.Name = "grpLog";
        grpLog.Text = "Activity Log";

        lstLog.Dock = DockStyle.Fill;
        lstLog.View = View.Details;
        lstLog.FullRowSelect = true;
        lstLog.HeaderStyle = ColumnHeaderStyle.None;
        lstLog.Name = "lstLog";
        lstLog.Columns.Add("Entry", -2);

        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1076, 632);
        Controls.Add(grpLog);
        Controls.Add(grpReceived);
        Controls.Add(grpSend);
        Controls.Add(pnlToolbar);
        MinimumSize = new Size(1092, 671);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Roche SimuLink";

        pnlToolbar.ResumeLayout(false);
        pnlToolbar.PerformLayout();
        grpSend.ResumeLayout(false);
        grpSend.PerformLayout();
        grpReceived.ResumeLayout(false);
        grpReceived.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridOrders).EndInit();
        grpLog.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlToolbar;
    private Button btnConnect;
    private Button btnDisconnect;
    private Button btnSettings;
    private Label lblBrand;

    private GroupBox grpSend;
    private Label lblSampleId;
    private TextBox txtSampleId;
    private Label lblTestType;
    private ComboBox cmbTestType;
    private Label lblResult;
    private ComboBox cmbResult;
    private Label lblSampleType;
    private ComboBox cmbSampleType;
    private Label lblSampleVolume;
    private ComboBox cmbSampleVolume;
    private Label lblResultStatus;
    private ComboBox cmbResultStatus;
    private Label lblFlags;
    private CheckBox chkNormal;
    private CheckBox chkHigh;
    private CheckBox chkLow;
    private CheckBox chkCritical;
    private Button btnSendResult;

    private GroupBox grpReceived;
    private Label lblOrderNumber;
    private TextBox txtOrderNumber;
    private Label lblRecvSampleId;
    private TextBox txtRecvSampleId;
    private Label lblPatientName;
    private TextBox txtPatientName;
    private Label lblDob;
    private TextBox txtDob;
    private Label lblSex;
    private TextBox txtSex;
    private DataGridView gridOrders;
    private DataGridViewTextBoxColumn colTestCode;
    private DataGridViewTextBoxColumn colTestName;
    private DataGridViewTextBoxColumn colPriority;

    private GroupBox grpLog;
    private ListView lstLog;
}
