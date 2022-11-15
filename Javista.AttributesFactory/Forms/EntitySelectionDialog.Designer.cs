namespace Javista.AttributesFactory.Forms
{
    partial class EntitySelectionDialog
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
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.chkExportDerivedAttributes = new System.Windows.Forms.CheckBox();
            this.chkLoadAllAttributes = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.lvEntities = new System.Windows.Forms.ListView();
            this.chDisplayName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.llInvertSelection = new System.Windows.Forms.LinkLabel();
            this.llClearAll = new System.Windows.Forms.LinkLabel();
            this.llSelectAll = new System.Windows.Forms.LinkLabel();
            this.pnlSolution = new System.Windows.Forms.Panel();
            this.cbbSolutions = new System.Windows.Forms.ComboBox();
            this.lblSolutionSelection = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlSolution.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(2);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(920, 80);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeaderTitle.Location = new System.Drawing.Point(7, 7);
            this.lblHeaderTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(253, 32);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "Select tables to export";
            // 
            // pnlFooter
            // 
            this.pnlFooter.Controls.Add(this.chkExportDerivedAttributes);
            this.pnlFooter.Controls.Add(this.chkLoadAllAttributes);
            this.pnlFooter.Controls.Add(this.btnOK);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 407);
            this.pnlFooter.Margin = new System.Windows.Forms.Padding(2);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(920, 60);
            this.pnlFooter.TabIndex = 1;
            // 
            // chkExportDerivedAttributes
            // 
            this.chkExportDerivedAttributes.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkExportDerivedAttributes.Location = new System.Drawing.Point(217, 0);
            this.chkExportDerivedAttributes.Margin = new System.Windows.Forms.Padding(2);
            this.chkExportDerivedAttributes.Name = "chkExportDerivedAttributes";
            this.chkExportDerivedAttributes.Size = new System.Drawing.Size(420, 60);
            this.chkExportDerivedAttributes.TabIndex = 30;
            this.chkExportDerivedAttributes.Text = "Export derived columns (rollup, currency, virutal)";
            this.chkExportDerivedAttributes.UseVisualStyleBackColor = true;
            // 
            // chkLoadAllAttributes
            // 
            this.chkLoadAllAttributes.Dock = System.Windows.Forms.DockStyle.Left;
            this.chkLoadAllAttributes.Location = new System.Drawing.Point(0, 0);
            this.chkLoadAllAttributes.Margin = new System.Windows.Forms.Padding(2);
            this.chkLoadAllAttributes.Name = "chkLoadAllAttributes";
            this.chkLoadAllAttributes.Size = new System.Drawing.Size(217, 60);
            this.chkLoadAllAttributes.TabIndex = 29;
            this.chkLoadAllAttributes.Text = "Export system columns";
            this.chkLoadAllAttributes.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(679, 13);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 35);
            this.btnOK.TabIndex = 28;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(798, 13);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 35);
            this.btnCancel.TabIndex = 27;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lvEntities);
            this.pnlMain.Controls.Add(this.panel1);
            this.pnlMain.Controls.Add(this.pnlSolution);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 80);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(2);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(920, 327);
            this.pnlMain.TabIndex = 2;
            // 
            // lvEntities
            // 
            this.lvEntities.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvEntities.CheckBoxes = true;
            this.lvEntities.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chDisplayName,
            this.chName});
            this.lvEntities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvEntities.HideSelection = false;
            this.lvEntities.HoverSelection = true;
            this.lvEntities.Location = new System.Drawing.Point(0, 72);
            this.lvEntities.Margin = new System.Windows.Forms.Padding(2);
            this.lvEntities.Name = "lvEntities";
            this.lvEntities.Size = new System.Drawing.Size(920, 255);
            this.lvEntities.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvEntities.TabIndex = 3;
            this.lvEntities.UseCompatibleStateImageBehavior = false;
            this.lvEntities.View = System.Windows.Forms.View.Details;
            this.lvEntities.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvEntities_ColumnClick);
            // 
            // chDisplayName
            // 
            this.chDisplayName.Text = "Display name";
            this.chDisplayName.Width = 300;
            // 
            // chName
            // 
            this.chName.Text = "Logical name";
            this.chName.Width = 300;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.llInvertSelection);
            this.panel1.Controls.Add(this.llClearAll);
            this.panel1.Controls.Add(this.llSelectAll);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 32);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(920, 40);
            this.panel1.TabIndex = 2;
            // 
            // llInvertSelection
            // 
            this.llInvertSelection.Dock = System.Windows.Forms.DockStyle.Right;
            this.llInvertSelection.Location = new System.Drawing.Point(635, 0);
            this.llInvertSelection.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.llInvertSelection.Name = "llInvertSelection";
            this.llInvertSelection.Size = new System.Drawing.Size(142, 40);
            this.llInvertSelection.TabIndex = 2;
            this.llInvertSelection.TabStop = true;
            this.llInvertSelection.Text = "Invert selection";
            this.llInvertSelection.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.llInvertSelection.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llInvertSelection_LinkClicked);
            // 
            // llClearAll
            // 
            this.llClearAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.llClearAll.Location = new System.Drawing.Point(777, 0);
            this.llClearAll.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.llClearAll.Name = "llClearAll";
            this.llClearAll.Size = new System.Drawing.Size(68, 40);
            this.llClearAll.TabIndex = 1;
            this.llClearAll.TabStop = true;
            this.llClearAll.Text = "Clear all";
            this.llClearAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.llClearAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llClearAll_LinkClicked);
            // 
            // llSelectAll
            // 
            this.llSelectAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.llSelectAll.Location = new System.Drawing.Point(845, 0);
            this.llSelectAll.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.llSelectAll.Name = "llSelectAll";
            this.llSelectAll.Size = new System.Drawing.Size(75, 40);
            this.llSelectAll.TabIndex = 0;
            this.llSelectAll.TabStop = true;
            this.llSelectAll.Text = "Select all";
            this.llSelectAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.llSelectAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llSelectAll_LinkClicked);
            // 
            // pnlSolution
            // 
            this.pnlSolution.Controls.Add(this.cbbSolutions);
            this.pnlSolution.Controls.Add(this.lblSolutionSelection);
            this.pnlSolution.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSolution.Location = new System.Drawing.Point(0, 0);
            this.pnlSolution.Margin = new System.Windows.Forms.Padding(2);
            this.pnlSolution.Name = "pnlSolution";
            this.pnlSolution.Size = new System.Drawing.Size(920, 32);
            this.pnlSolution.TabIndex = 0;
            // 
            // cbbSolutions
            // 
            this.cbbSolutions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbbSolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbSolutions.FormattingEnabled = true;
            this.cbbSolutions.Location = new System.Drawing.Point(217, 0);
            this.cbbSolutions.Margin = new System.Windows.Forms.Padding(2);
            this.cbbSolutions.Name = "cbbSolutions";
            this.cbbSolutions.Size = new System.Drawing.Size(703, 28);
            this.cbbSolutions.Sorted = true;
            this.cbbSolutions.TabIndex = 1;
            this.cbbSolutions.SelectedIndexChanged += new System.EventHandler(this.cbbSolutions_SelectedIndexChanged);
            // 
            // lblSolutionSelection
            // 
            this.lblSolutionSelection.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSolutionSelection.Location = new System.Drawing.Point(0, 0);
            this.lblSolutionSelection.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSolutionSelection.Name = "lblSolutionSelection";
            this.lblSolutionSelection.Size = new System.Drawing.Size(217, 32);
            this.lblSolutionSelection.TabIndex = 0;
            this.lblSolutionSelection.Text = "Tables from solution";
            this.lblSolutionSelection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // EntitySelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 467);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EntitySelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.EntitySelectionDialog_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlSolution.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlSolution;
        private System.Windows.Forms.ComboBox cbbSolutions;
        private System.Windows.Forms.Label lblSolutionSelection;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.ListView lvEntities;
        private System.Windows.Forms.ColumnHeader chDisplayName;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel llInvertSelection;
        private System.Windows.Forms.LinkLabel llClearAll;
        private System.Windows.Forms.LinkLabel llSelectAll;
        private System.Windows.Forms.CheckBox chkLoadAllAttributes;
        private System.Windows.Forms.CheckBox chkExportDerivedAttributes;
    }
}