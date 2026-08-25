namespace RocheLIT;

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
        // Menu
        menuStrip = new MenuStrip();
        toolsMenu = new ToolStripMenuItem();
        exampleGeneratorMenuItem = new ToolStripMenuItem();

        // Toolbar
        pnlToolbar = new Panel();
        btnConnect = new Button();
        btnDisconnect = new Button();
        btnSettings = new Button();
        picBrand = new PictureBox();
        picAppLogo = new PictureBox();

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
        btnSendResult = new Button();

        // Right: Received order
        grpReceived = new GroupBox();
        lblOrderNumber = new Label();
        txtOrderNumber = new TextBox();
        lblRecvSampleId = new Label();
        txtRecvSampleId = new TextBox();
        lblReceivedTestType = new Label();
        txtReceivedTestType = new TextBox();
        lblReceivedSampleType = new Label();
        txtReceivedSampleType = new TextBox();
        lblReceivedSampleVolume = new Label();
        txtReceivedSampleVolume = new TextBox();
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
        // menuStrip
        //
        menuStrip.Items.Add(toolsMenu);
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        //
        // toolsMenu
        //
        toolsMenu.DropDownItems.Add(exampleGeneratorMenuItem);
        toolsMenu.Text = "&Tools";
        toolsMenu.Name = "toolsMenu";
        //
        // exampleGeneratorMenuItem
        //
        exampleGeneratorMenuItem.Text = "&Example Generator...";
        exampleGeneratorMenuItem.Name = "exampleGeneratorMenuItem";
        exampleGeneratorMenuItem.Click += exampleGeneratorMenuItem_Click;

        //
        // pnlToolbar
        //
        pnlToolbar.BackColor = Color.White;
        pnlToolbar.Controls.Add(picAppLogo);
        pnlToolbar.Controls.Add(btnConnect);
        pnlToolbar.Controls.Add(btnDisconnect);
        pnlToolbar.Controls.Add(btnSettings);
        pnlToolbar.Controls.Add(picBrand);
        pnlToolbar.Dock = DockStyle.Top;
        pnlToolbar.Height = 56;
        pnlToolbar.Name = "pnlToolbar";

        //
        // picAppLogo (LIT logo)
        //
        picAppLogo.BackColor = Color.Transparent;
        picAppLogo.Location = new Point(12, 6);
        picAppLogo.Size = new Size(240, 44);
        picAppLogo.Name = "picAppLogo";
        picAppLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picAppLogo.TabStop = false;
        picAppLogo.Image = LoadAppLogo();

        //
        // btnConnect
        //
        btnConnect.BackColor = Color.FromArgb(0, 102, 204);
        btnConnect.ForeColor = Color.White;
        btnConnect.FlatStyle = FlatStyle.Flat;
        btnConnect.Location = new Point(270, 10);
        btnConnect.Size = new Size(160, 36);
        btnConnect.Name = "btnConnect";
        btnConnect.Text = "Connect to LIS";
        btnConnect.UseVisualStyleBackColor = false;
        btnConnect.Click += btnConnect_Click;

        //
        // btnDisconnect
        //
        btnDisconnect.Location = new Point(438, 10);
        btnDisconnect.Size = new Size(140, 36);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Text = "Disconnect";
        btnDisconnect.Enabled = false;
        btnDisconnect.Click += btnDisconnect_Click;

        //
        // btnSettings
        //
        btnSettings.Location = new Point(586, 10);
        btnSettings.Size = new Size(140, 36);
        btnSettings.Name = "btnSettings";
        btnSettings.Text = "⚙  Settings";
        btnSettings.Click += btnSettings_Click;

        //
        // picBrand (Roche logo)
        //
        picBrand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        picBrand.BackColor = Color.Transparent;
        picBrand.Location = new Point(880, 8);
        picBrand.Size = new Size(80, 40);
        picBrand.Name = "picBrand";
        picBrand.SizeMode = PictureBoxSizeMode.Zoom;
        picBrand.TabStop = false;
        picBrand.Image = LoadBrandLogo();

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
        grpSend.Controls.Add(btnSendResult);
        grpSend.Location = new Point(12, 92);
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

        btnSendResult.BackColor = Color.FromArgb(0, 102, 204);
        btnSendResult.ForeColor = Color.White;
        btnSendResult.FlatStyle = FlatStyle.Flat;
        btnSendResult.Location = new Point(150, 264);
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
        grpReceived.Controls.Add(lblReceivedTestType);
        grpReceived.Controls.Add(txtReceivedTestType);
        grpReceived.Controls.Add(lblReceivedSampleType);
        grpReceived.Controls.Add(txtReceivedSampleType);
        grpReceived.Controls.Add(lblReceivedSampleVolume);
        grpReceived.Controls.Add(txtReceivedSampleVolume);
        grpReceived.Controls.Add(gridOrders);
        grpReceived.Location = new Point(504, 92);
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

        lblReceivedTestType.AutoSize = true;
        lblReceivedTestType.Location = new Point(16, 108);
        lblReceivedTestType.Text = "Test Type:";
        txtReceivedTestType.Location = new Point(150, 105);
        txtReceivedTestType.Size = new Size(390, 27);
        txtReceivedTestType.ReadOnly = true;
        txtReceivedTestType.Name = "txtReceivedTestType";

        lblReceivedSampleType.AutoSize = true;
        lblReceivedSampleType.Location = new Point(16, 144);
        lblReceivedSampleType.Text = "Sample Type:";
        txtReceivedSampleType.Location = new Point(150, 141);
        txtReceivedSampleType.Size = new Size(220, 27);
        txtReceivedSampleType.ReadOnly = true;
        txtReceivedSampleType.Name = "txtReceivedSampleType";

        lblReceivedSampleVolume.AutoSize = true;
        lblReceivedSampleVolume.Location = new Point(16, 180);
        lblReceivedSampleVolume.Text = "Sample Volume:";
        txtReceivedSampleVolume.Location = new Point(150, 177);
        txtReceivedSampleVolume.Size = new Size(220, 27);
        txtReceivedSampleVolume.ReadOnly = true;
        txtReceivedSampleVolume.Name = "txtReceivedSampleVolume";

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
        grpLog.Location = new Point(12, 464);
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
        ClientSize = new Size(1076, 656);
        Controls.Add(grpLog);
        Controls.Add(grpReceived);
        Controls.Add(grpSend);
        Controls.Add(pnlToolbar);
        // Added last so it docks above the toolbar; set as the form's menu.
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        MinimumSize = new Size(1092, 695);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Roche LIT";

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

    private MenuStrip menuStrip;
    private ToolStripMenuItem toolsMenu;
    private ToolStripMenuItem exampleGeneratorMenuItem;

    private Panel pnlToolbar;
    private Button btnConnect;
    private Button btnDisconnect;
    private Button btnSettings;
    private PictureBox picBrand;
    private PictureBox picAppLogo;

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
    private Button btnSendResult;

    private GroupBox grpReceived;
    private Label lblOrderNumber;
    private TextBox txtOrderNumber;
    private Label lblRecvSampleId;
    private TextBox txtRecvSampleId;
    private Label lblReceivedTestType;
    private TextBox txtReceivedTestType;
    private Label lblReceivedSampleType;
    private TextBox txtReceivedSampleType;
    private Label lblReceivedSampleVolume;
    private TextBox txtReceivedSampleVolume;
    private DataGridView gridOrders;
    private DataGridViewTextBoxColumn colTestCode;
    private DataGridViewTextBoxColumn colTestName;
    private DataGridViewTextBoxColumn colPriority;

    private GroupBox grpLog;
    private ListView lstLog;
}
