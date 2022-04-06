namespace Javista.AttributesFactory
{
    partial class PluginControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbProcess = new System.Windows.Forms.ToolStripButton();
            this.tsbCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbGetTemplate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsddbLogs = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExportEntities = new System.Windows.Forms.ToolStripButton();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.cbbSolutions = new System.Windows.Forms.ComboBox();
            this.btnRefreshSolutions = new System.Windows.Forms.Button();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblSolutionSelection = new System.Windows.Forms.Label();
            this.ilStatus = new System.Windows.Forms.ImageList(this.components);
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.chkAddOptionSetSuffix = new System.Windows.Forms.CheckBox();
            this.chkAddLookupSuffix = new System.Windows.Forms.CheckBox();
            this.gbLogs = new System.Windows.Forms.GroupBox();
            this.lvLogs = new System.Windows.Forms.ListView();
            this.chImage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDisplayName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLogicalName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chEntity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblDelay = new System.Windows.Forms.Label();
            this.nudDelay = new System.Windows.Forms.NumericUpDown();
            this.toolStripMenu.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.gbLogs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbProcess,
            this.tsbCancel,
            this.toolStripSeparator1,
            this.tsbGetTemplate,
            this.toolStripSeparator2,
            this.tsddbLogs,
            this.toolStripSeparator3,
            this.tsbExportEntities});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1628, 34);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "tsMain";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbClose.Image = ((System.Drawing.Image)(resources.GetObject("tsbClose.Image")));
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(34, 29);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 34);
            // 
            // tsbProcess
            // 
            this.tsbProcess.Image = ((System.Drawing.Image)(resources.GetObject("tsbProcess.Image")));
            this.tsbProcess.Name = "tsbProcess";
            this.tsbProcess.Size = new System.Drawing.Size(172, 29);
            this.tsbProcess.Text = "Process attributes";
            this.tsbProcess.Click += new System.EventHandler(this.tsbProcess_Click);
            // 
            // tsbCancel
            // 
            this.tsbCancel.Image = ((System.Drawing.Image)(resources.GetObject("tsbCancel.Image")));
            this.tsbCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCancel.Name = "tsbCancel";
            this.tsbCancel.Size = new System.Drawing.Size(83, 29);
            this.tsbCancel.Text = "Cancel";
            this.tsbCancel.Visible = false;
            this.tsbCancel.Click += new System.EventHandler(this.tsbCancel_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 34);
            // 
            // tsbGetTemplate
            // 
            this.tsbGetTemplate.Image = ((System.Drawing.Image)(resources.GetObject("tsbGetTemplate.Image")));
            this.tsbGetTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbGetTemplate.Name = "tsbGetTemplate";
            this.tsbGetTemplate.Size = new System.Drawing.Size(134, 29);
            this.tsbGetTemplate.Text = "Get template";
            this.tsbGetTemplate.ToolTipText = "Get the Excel template file to list attributes to create";
            this.tsbGetTemplate.Click += new System.EventHandler(this.tsbGetTemplate_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 34);
            // 
            // tsddbLogs
            // 
            this.tsddbLogs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportLogsToolStripMenuItem,
            this.clearLogsToolStripMenuItem});
            this.tsddbLogs.Image = ((System.Drawing.Image)(resources.GetObject("tsddbLogs.Image")));
            this.tsddbLogs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbLogs.Name = "tsddbLogs";
            this.tsddbLogs.Size = new System.Drawing.Size(84, 29);
            this.tsddbLogs.Text = "Logs";
            // 
            // exportLogsToolStripMenuItem
            // 
            this.exportLogsToolStripMenuItem.Name = "exportLogsToolStripMenuItem";
            this.exportLogsToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.exportLogsToolStripMenuItem.Text = "Export logs";
            this.exportLogsToolStripMenuItem.Click += new System.EventHandler(this.exportLogsToolStripMenuItem_Click);
            // 
            // clearLogsToolStripMenuItem
            // 
            this.clearLogsToolStripMenuItem.Name = "clearLogsToolStripMenuItem";
            this.clearLogsToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.clearLogsToolStripMenuItem.Text = "Clear logs";
            this.clearLogsToolStripMenuItem.Click += new System.EventHandler(this.clearLogsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 34);
            // 
            // tsbExportEntities
            // 
            this.tsbExportEntities.Image = global::Javista.AttributesFactory.Properties.Resources.ico_16_9507_Excel;
            this.tsbExportEntities.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExportEntities.Name = "tsbExportEntities";
            this.tsbExportEntities.Size = new System.Drawing.Size(144, 29);
            this.tsbExportEntities.Text = "Export tables";
            this.tsbExportEntities.Click += new System.EventHandler(this.tsbExportEntities_Click);
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.btnOpenFile);
            this.pnlOptions.Controls.Add(this.txtFilePath);
            this.pnlOptions.Controls.Add(this.cbbSolutions);
            this.pnlOptions.Controls.Add(this.btnRefreshSolutions);
            this.pnlOptions.Controls.Add(this.lblFilePath);
            this.pnlOptions.Controls.Add(this.lblSolutionSelection);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOptions.Location = new System.Drawing.Point(0, 34);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(1628, 78);
            this.pnlOptions.TabIndex = 5;
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFile.Location = new System.Drawing.Point(1578, 37);
            this.btnOpenFile.Margin = new System.Windows.Forms.Padding(0);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(50, 32);
            this.btnOpenFile.TabIndex = 4;
            this.btnOpenFile.Text = "...";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilePath.Location = new System.Drawing.Point(300, 40);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(1276, 26);
            this.txtFilePath.TabIndex = 3;
            // 
            // cbbSolutions
            // 
            this.cbbSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbbSolutions.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbbSolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbSolutions.FormattingEnabled = true;
            this.cbbSolutions.Location = new System.Drawing.Point(300, 0);
            this.cbbSolutions.Name = "cbbSolutions";
            this.cbbSolutions.Size = new System.Drawing.Size(1276, 27);
            this.cbbSolutions.TabIndex = 1;
            this.cbbSolutions.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.combobox_DrawItem);
            this.cbbSolutions.Click += new System.EventHandler(this.cbbSolutions_Click);
            // 
            // btnRefreshSolutions
            // 
            this.btnRefreshSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshSolutions.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshSolutions.Image")));
            this.btnRefreshSolutions.Location = new System.Drawing.Point(1578, 0);
            this.btnRefreshSolutions.Margin = new System.Windows.Forms.Padding(0);
            this.btnRefreshSolutions.Name = "btnRefreshSolutions";
            this.btnRefreshSolutions.Size = new System.Drawing.Size(50, 32);
            this.btnRefreshSolutions.TabIndex = 2;
            this.btnRefreshSolutions.UseVisualStyleBackColor = true;
            this.btnRefreshSolutions.Click += new System.EventHandler(this.btnRefreshSolutions_Click);
            // 
            // lblFilePath
            // 
            this.lblFilePath.Location = new System.Drawing.Point(3, 38);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(300, 28);
            this.lblFilePath.TabIndex = 5;
            this.lblFilePath.Text = "Excel file with attributes list";
            this.lblFilePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSolutionSelection
            // 
            this.lblSolutionSelection.Location = new System.Drawing.Point(3, 6);
            this.lblSolutionSelection.Name = "lblSolutionSelection";
            this.lblSolutionSelection.Size = new System.Drawing.Size(300, 20);
            this.lblSolutionSelection.TabIndex = 1;
            this.lblSolutionSelection.Text = "Solution where to create attributes";
            // 
            // ilStatus
            // 
            this.ilStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilStatus.ImageStream")));
            this.ilStatus.TransparentColor = System.Drawing.Color.Transparent;
            this.ilStatus.Images.SetKeyName(0, "control_play_blue.png");
            this.ilStatus.Images.SetKeyName(1, "timer_success_16.png");
            this.ilStatus.Images.SetKeyName(2, "notif_icn_crit16.png");
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.nudDelay);
            this.gbOptions.Controls.Add(this.lblDelay);
            this.gbOptions.Controls.Add(this.chkAddOptionSetSuffix);
            this.gbOptions.Controls.Add(this.chkAddLookupSuffix);
            this.gbOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbOptions.Location = new System.Drawing.Point(0, 112);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(1628, 59);
            this.gbOptions.TabIndex = 14;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options";
            // 
            // chkAddOptionSetSuffix
            // 
            this.chkAddOptionSetSuffix.AutoSize = true;
            this.chkAddOptionSetSuffix.Checked = true;
            this.chkAddOptionSetSuffix.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAddOptionSetSuffix.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkAddOptionSetSuffix.Location = new System.Drawing.Point(317, 22);
            this.chkAddOptionSetSuffix.Name = "chkAddOptionSetSuffix";
            this.chkAddOptionSetSuffix.Size = new System.Drawing.Size(357, 34);
            this.chkAddOptionSetSuffix.TabIndex = 6;
            this.chkAddOptionSetSuffix.Text = "Add \"Code\" suffix to OptionSet schema name";
            this.chkAddOptionSetSuffix.UseVisualStyleBackColor = true;
            // 
            // chkAddLookupSuffix
            // 
            this.chkAddLookupSuffix.AutoSize = true;
            this.chkAddLookupSuffix.Checked = true;
            this.chkAddLookupSuffix.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAddLookupSuffix.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkAddLookupSuffix.Location = new System.Drawing.Point(3, 22);
            this.chkAddLookupSuffix.Name = "chkAddLookupSuffix";
            this.chkAddLookupSuffix.Size = new System.Drawing.Size(314, 34);
            this.chkAddLookupSuffix.TabIndex = 5;
            this.chkAddLookupSuffix.Text = "Add \"Id\" suffix to Lookup schema name";
            this.chkAddLookupSuffix.UseVisualStyleBackColor = true;
            // 
            // gbLogs
            // 
            this.gbLogs.Controls.Add(this.lvLogs);
            this.gbLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbLogs.Location = new System.Drawing.Point(0, 171);
            this.gbLogs.Name = "gbLogs";
            this.gbLogs.Size = new System.Drawing.Size(1628, 637);
            this.gbLogs.TabIndex = 15;
            this.gbLogs.TabStop = false;
            this.gbLogs.Text = "Logs";
            // 
            // lvLogs
            // 
            this.lvLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chImage,
            this.chDisplayName,
            this.chType,
            this.chLogicalName,
            this.chEntity,
            this.chStatus,
            this.chInfo});
            this.lvLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLogs.FullRowSelect = true;
            this.lvLogs.HideSelection = false;
            this.lvLogs.Location = new System.Drawing.Point(3, 22);
            this.lvLogs.Name = "lvLogs";
            this.lvLogs.Size = new System.Drawing.Size(1622, 612);
            this.lvLogs.SmallImageList = this.ilStatus;
            this.lvLogs.TabIndex = 0;
            this.lvLogs.UseCompatibleStateImageBehavior = false;
            this.lvLogs.View = System.Windows.Forms.View.Details;
            // 
            // chImage
            // 
            this.chImage.Text = "";
            this.chImage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.chImage.Width = 32;
            // 
            // chDisplayName
            // 
            this.chDisplayName.Text = "Display name";
            this.chDisplayName.Width = 150;
            // 
            // chType
            // 
            this.chType.Text = "Type";
            this.chType.Width = 150;
            // 
            // chLogicalName
            // 
            this.chLogicalName.Text = "Logical name";
            this.chLogicalName.Width = 150;
            // 
            // chEntity
            // 
            this.chEntity.Text = "Entity";
            this.chEntity.Width = 150;
            // 
            // chStatus
            // 
            this.chStatus.Text = "Status";
            this.chStatus.Width = 150;
            // 
            // chInfo
            // 
            this.chInfo.Text = "Additional info";
            this.chInfo.Width = 300;
            // 
            // lblDelay
            // 
            this.lblDelay.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblDelay.Location = new System.Drawing.Point(674, 22);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDelay.Size = new System.Drawing.Size(216, 34);
            this.lblDelay.TabIndex = 7;
            this.lblDelay.Text = "Delay between operations";
            this.lblDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nudDelay
            // 
            this.nudDelay.Dock = System.Windows.Forms.DockStyle.Left;
            this.nudDelay.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudDelay.Location = new System.Drawing.Point(890, 22);
            this.nudDelay.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.nudDelay.Name = "nudDelay";
            this.nudDelay.Size = new System.Drawing.Size(85, 26);
            this.nudDelay.TabIndex = 8;
            // 
            // PluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbLogs);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.pnlOptions);
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "PluginControl";
            this.Size = new System.Drawing.Size(1628, 808);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.gbLogs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsbProcess;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.Label lblSolutionSelection;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbGetTemplate;
        private System.Windows.Forms.ImageList ilStatus;
        private System.Windows.Forms.ToolStripButton tsbCancel;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.CheckBox chkAddOptionSetSuffix;
        private System.Windows.Forms.CheckBox chkAddLookupSuffix;
        private System.Windows.Forms.ComboBox cbbSolutions;
        private System.Windows.Forms.Button btnRefreshSolutions;
        private System.Windows.Forms.GroupBox gbLogs;
        private System.Windows.Forms.ListView lvLogs;
        private System.Windows.Forms.ColumnHeader chImage;
        private System.Windows.Forms.ColumnHeader chDisplayName;
        private System.Windows.Forms.ColumnHeader chType;
        private System.Windows.Forms.ColumnHeader chLogicalName;
        private System.Windows.Forms.ColumnHeader chEntity;
        private System.Windows.Forms.ColumnHeader chStatus;
        private System.Windows.Forms.ColumnHeader chInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton tsddbLogs;
        private System.Windows.Forms.ToolStripMenuItem exportLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsbExportEntities;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.NumericUpDown nudDelay;
        private System.Windows.Forms.Label lblDelay;
    }
}
