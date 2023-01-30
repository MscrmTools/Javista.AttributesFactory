
namespace Javista.AttributesFactory.Forms
{
    partial class EntityCreationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnCreateEntities = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.dgvTables = new System.Windows.Forms.DataGridView();
            this.EntitySchemaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EntityOwnershipType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.EntityDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EntityCollectionDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EntityIsActivity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.PrimaryAttributeSchemaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrimaryAttributeDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrimaryAttributeLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrimaryAttributeRequired = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.cbbApps = new System.Windows.Forms.ComboBox();
            this.lblAddToModelDrivenApp = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTables)).BeginInit();
            this.pnlOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(15);
            this.pnlHeader.Size = new System.Drawing.Size(1244, 74);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Location = new System.Drawing.Point(15, 15);
            this.lblHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(1214, 44);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "We detected tables that does not exist on the connected environment. You can crea" +
    "te them now";
            // 
            // pnlFooter
            // 
            this.pnlFooter.Controls.Add(this.btnCreateEntities);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 544);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1244, 57);
            this.pnlFooter.TabIndex = 3;
            // 
            // btnCreateEntities
            // 
            this.btnCreateEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateEntities.Location = new System.Drawing.Point(943, 8);
            this.btnCreateEntities.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCreateEntities.Name = "btnCreateEntities";
            this.btnCreateEntities.Size = new System.Drawing.Size(165, 42);
            this.btnCreateEntities.TabIndex = 1;
            this.btnCreateEntities.Text = "Create Tables";
            this.btnCreateEntities.UseVisualStyleBackColor = true;
            this.btnCreateEntities.Click += new System.EventHandler(this.btnCreateEntities_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(1117, 8);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(123, 42);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 601);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1244, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 15);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.dgvTables);
            this.pnlMain.Controls.Add(this.pnlOptions);
            this.pnlMain.Location = new System.Drawing.Point(0, 74);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
            this.pnlMain.Size = new System.Drawing.Size(1234, 473);
            this.pnlMain.TabIndex = 4;
            // 
            // dgvTables
            // 
            this.dgvTables.AllowUserToAddRows = false;
            this.dgvTables.AllowUserToDeleteRows = false;
            this.dgvTables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EntitySchemaName,
            this.EntityOwnershipType,
            this.EntityDisplayName,
            this.EntityCollectionDisplayName,
            this.EntityIsActivity,
            this.PrimaryAttributeSchemaName,
            this.PrimaryAttributeDisplayName,
            this.PrimaryAttributeLength,
            this.PrimaryAttributeRequired});
            this.dgvTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTables.Location = new System.Drawing.Point(10, 9);
            this.dgvTables.Name = "dgvTables";
            this.dgvTables.RowHeadersWidth = 62;
            this.dgvTables.RowTemplate.Height = 28;
            this.dgvTables.Size = new System.Drawing.Size(1214, 410);
            this.dgvTables.TabIndex = 0;
            // 
            // EntitySchemaName
            // 
            this.EntitySchemaName.HeaderText = "Table Schema Name";
            this.EntitySchemaName.MinimumWidth = 8;
            this.EntitySchemaName.Name = "EntitySchemaName";
            this.EntitySchemaName.ReadOnly = true;
            this.EntitySchemaName.Width = 150;
            // 
            // EntityOwnershipType
            // 
            this.EntityOwnershipType.HeaderText = "Ownership type";
            this.EntityOwnershipType.MinimumWidth = 8;
            this.EntityOwnershipType.Name = "EntityOwnershipType";
            this.EntityOwnershipType.Width = 150;
            // 
            // EntityDisplayName
            // 
            this.EntityDisplayName.HeaderText = "Display name";
            this.EntityDisplayName.MinimumWidth = 8;
            this.EntityDisplayName.Name = "EntityDisplayName";
            this.EntityDisplayName.Width = 150;
            // 
            // EntityCollectionDisplayName
            // 
            this.EntityCollectionDisplayName.HeaderText = "Display collection name";
            this.EntityCollectionDisplayName.MinimumWidth = 8;
            this.EntityCollectionDisplayName.Name = "EntityCollectionDisplayName";
            this.EntityCollectionDisplayName.Width = 150;
            // 
            // EntityIsActivity
            // 
            this.EntityIsActivity.HeaderText = "Is Activity";
            this.EntityIsActivity.MinimumWidth = 8;
            this.EntityIsActivity.Name = "EntityIsActivity";
            this.EntityIsActivity.Width = 150;
            // 
            // PrimaryAttributeSchemaName
            // 
            this.PrimaryAttributeSchemaName.HeaderText = "Primary Column Schema name";
            this.PrimaryAttributeSchemaName.MinimumWidth = 8;
            this.PrimaryAttributeSchemaName.Name = "PrimaryAttributeSchemaName";
            this.PrimaryAttributeSchemaName.Width = 150;
            // 
            // PrimaryAttributeDisplayName
            // 
            this.PrimaryAttributeDisplayName.HeaderText = "Primary Column Display name";
            this.PrimaryAttributeDisplayName.MinimumWidth = 8;
            this.PrimaryAttributeDisplayName.Name = "PrimaryAttributeDisplayName";
            this.PrimaryAttributeDisplayName.Width = 150;
            // 
            // PrimaryAttributeLength
            // 
            this.PrimaryAttributeLength.HeaderText = "Primary Column Length";
            this.PrimaryAttributeLength.MinimumWidth = 8;
            this.PrimaryAttributeLength.Name = "PrimaryAttributeLength";
            this.PrimaryAttributeLength.Width = 150;
            // 
            // PrimaryAttributeRequired
            // 
            this.PrimaryAttributeRequired.HeaderText = "Primary Column Required";
            this.PrimaryAttributeRequired.MinimumWidth = 8;
            this.PrimaryAttributeRequired.Name = "PrimaryAttributeRequired";
            this.PrimaryAttributeRequired.Width = 150;
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.cbbApps);
            this.pnlOptions.Controls.Add(this.lblAddToModelDrivenApp);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlOptions.Location = new System.Drawing.Point(10, 419);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(1214, 45);
            this.pnlOptions.TabIndex = 1;
            // 
            // cbbApps
            // 
            this.cbbApps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbApps.FormattingEnabled = true;
            this.cbbApps.Location = new System.Drawing.Point(197, 12);
            this.cbbApps.Name = "cbbApps";
            this.cbbApps.Size = new System.Drawing.Size(238, 28);
            this.cbbApps.TabIndex = 1;
            // 
            // lblAddToModelDrivenApp
            // 
            this.lblAddToModelDrivenApp.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblAddToModelDrivenApp.Location = new System.Drawing.Point(0, 0);
            this.lblAddToModelDrivenApp.Name = "lblAddToModelDrivenApp";
            this.lblAddToModelDrivenApp.Size = new System.Drawing.Size(191, 45);
            this.lblAddToModelDrivenApp.TabIndex = 0;
            this.lblAddToModelDrivenApp.Text = "Add to Model driven app : ";
            this.lblAddToModelDrivenApp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // EntityCreationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1244, 623);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip1);
            this.Name = "EntityCreationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Missing tables detected!";
            this.pnlHeader.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTables)).EndInit();
            this.pnlOptions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button btnCreateEntities;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.DataGridView dgvTables;
        private System.Windows.Forms.DataGridViewTextBoxColumn EntitySchemaName;
        private System.Windows.Forms.DataGridViewComboBoxColumn EntityOwnershipType;
        private System.Windows.Forms.DataGridViewTextBoxColumn EntityDisplayName;
        private System.Windows.Forms.DataGridViewTextBoxColumn EntityCollectionDisplayName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn EntityIsActivity;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrimaryAttributeSchemaName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrimaryAttributeDisplayName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrimaryAttributeLength;
        private System.Windows.Forms.DataGridViewCheckBoxColumn PrimaryAttributeRequired;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.Label lblAddToModelDrivenApp;
        private System.Windows.Forms.ComboBox cbbApps;
    }
}