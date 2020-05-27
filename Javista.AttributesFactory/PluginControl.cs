using Javista.AttributesFactory.AppCode;
using Javista.AttributesFactory.Forms;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;

namespace Javista.AttributesFactory
{
    public partial class PluginControl : PluginControlBase, IStatusBarMessenger
    {
        private List<AppCode.SolutionInfo> _solutions;
        private bool isRequestFromSolutionsList;
        private int languageCode;

        public PluginControl()
        {
            InitializeComponent();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

        public void ProcessAttributes()
        {
            if (cbbSolutions.SelectedItem == null)
            {
                MessageBox.Show(this, @"Please select a solution", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtFilePath.Text.Length == 0)
            {
                MessageBox.Show(this, @"Please define a file", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(txtFilePath.Text))
            {
                MessageBox.Show(this, @"The file specified does not exist", @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settings = new CreateSettings
            {
                FilePath = txtFilePath.Text,
                LanguageCode = languageCode,
                Solution = (AppCode.SolutionInfo)cbbSolutions.SelectedItem,
                AddLookupSuffix = chkAddLookupSuffix.Checked,
                AddOptionSetSuffix = chkAddOptionSetSuffix.Checked
            };

            lvLogs.Items.Clear();
            SetWorkingState(true);

            WorkAsync(new WorkAsyncInfo
            {
                Message = string.Empty,
                AsyncArgument = settings,
                Work = (w, e) =>
                {
                    var manager = new MetadataUpsertManager((CreateSettings)e.Argument, Service, ConnectionDetail.OrganizationMajorVersion);
                    manager.Process(w, ConnectionDetail);
                },
                ProgressChanged = e =>
                {
                    var info = (ProcessResult)e.UserState;
                    SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(e.ProgressPercentage, "Processing..."));

                    var item = lvLogs.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Tag == info);
                    if (item != null)
                    {
                        if (info.Success)
                        {
                            item.ImageIndex = 1;
                            item.SubItems[3].Text = info.Attribute;
                            item.SubItems[5].Text = info.IsCreate ? @"Created" : @"Updated";
                        }
                        else
                        {
                            item.ImageIndex = 2;
                            item.SubItems[3].Text = info.Attribute;
                            item.SubItems[5].Text = $@"{(info.IsCreate ? "Create" : "Update")} Error";
                            item.SubItems[6].Text = info.Message;
                        }
                    }
                    else
                    {
                        item = new ListViewItem
                        {
                            Tag = info,
                            ImageIndex = info.Processing ? 0 : info.Success ? 1 : 2,
                            Text = string.Empty
                        };
                        item.SubItems.Add(info.DisplayName);
                        item.SubItems.Add(info.Type);
                        item.SubItems.Add(info.Attribute);
                        item.SubItems.Add(info.Entity);
                        item.SubItems.Add(info.Processing ? "Processing..." : info.Success ? info.IsCreate ? "Created" : "Updated" : info.IsCreate ? "Create Error" : "Update Error");
                        item.SubItems.Add(string.Empty);

                        lvLogs.Items.Add(item);
                    }
                },
                PostWorkCallBack = e =>
                {
                    SetWorkingState(false);

                    SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(null, null));
                    if (e.Error != null)
                    {
                        MessageBox.Show(this, e.Error.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                },
                IsCancelable = true
            });
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            RetrieveBaseLanguage();

            if (!isRequestFromSolutionsList)
            {
                FillSolutions();
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = @"Excel spreadsheet|*.xlsx"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                txtFilePath.Text = ofd.FileName;
            }
        }

        private void btnRefreshSolutions_Click(object sender, EventArgs e)
        {
            isRequestFromSolutionsList = true;
            ExecuteMethod(FillSolutions);
        }

        private void cbbSolutions_Click(object sender, EventArgs e)
        {
            if (cbbSolutions.Items.Count == 0)
            {
                isRequestFromSolutionsList = true;
                ExecuteMethod(FillSolutions);
            }
        }

        private void clearLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvLogs.Items.Clear();
        }

        private void combobox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index == -1) return;

            var solution = (AppCode.SolutionInfo)((ComboBox)sender).Items[e.Index];

            Rectangle r1 = e.Bounds;
            r1.Width /= 2;

            // First column
            using (SolidBrush sb = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(solution.ToString(), e.Font, sb, r1);
            }

            // Second column
            Rectangle r2 = e.Bounds;
            r2.X = e.Bounds.Width / 2;
            r2.Width /= 4;

            using (SolidBrush sb = new SolidBrush(Color.DarkGray))
            {
                e.Graphics.DrawString($"Prefix: {solution.Prefix}", e.Font, sb, r2);
            }

            // Third column
            Rectangle r3 = e.Bounds;
            r3.X = e.Bounds.Width / 2 + e.Bounds.Width / 4;
            r3.Width /= 4;

            using (SolidBrush sb = new SolidBrush(Color.DarkGray))
            {
                e.Graphics.DrawString($"OptionSet Prefix: {solution.OptionSetPrefix}", e.Font, sb, r3);
            }
        }

        private void exportLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = @"CSV file|*.csv"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                lvLogs.ToCsv(sfd.FileName);
            }
        }

        private void FillSolutions()
        {
            cbbSolutions.Items.Clear();
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading solutions...",
                Work = (w, e) => { _solutions = SolutionManager.GetSolutions(Service); },
                PostWorkCallBack = e => { cbbSolutions.Items.AddRange(_solutions.OrderBy(s => s.ToString()).ToArray<object>()); }
            });
        }

        private void RetrieveBaseLanguage()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading base language...",
                Work = (w, e) => { e.Result = OrganizationManager.GetLanguageCode(Service); },
                PostWorkCallBack = e => { languageCode = (int)e.Result; }
            });
        }

        private void SetWorkingState(bool working)
        {
            tsbProcess.Visible = !working;
            tsbCancel.Visible = working;
            gbOptions.Enabled = !working;
            pnlOptions.Enabled = !working;

            tsbCancel.Text = @"Cancel";
            tsbCancel.Enabled = true;
        }

        private void tsbCancel_Click(object sender, EventArgs e)
        {
            tsbCancel.Text = @"Cancelling...";
            tsbCancel.Enabled = false;

            CancelWorker();
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void tsbExportEntities_Click(object sender, EventArgs e)
        {
            var dialog = new EntitySelectionDialog(Service, _solutions);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = @"Excel workbook|*.xlsx"
                };
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    var mdg = new MetadataDocManager(Service, this);
                    mdg.GenerateDocumentation(dialog.Entities, sfd.FileName, dialog.LoadAllAttributes, null);

                    if (MessageBox.Show(this, @"Do you want to open the document now?", @"Question",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Process.Start(sfd.FileName);
                    }
                }
            }
        }

        private void tsbGetTemplate_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = @"Excel workbook|*.xlsx",
                FileName = "Attributes_Template.xlsx"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Javista.AttributesFactory.Template.Attributes_Template.xlsx";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        MessageBox.Show(this, @"There was an error trying to retrieve the Excel template", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    using (FileStream fileStream = File.Create(sfd.FileName, (int)stream.Length))
                    {
                        byte[] bytesInStream = new byte[stream.Length];
                        stream.Read(bytesInStream, 0, bytesInStream.Length);
                        fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                    }
                }

                if (MessageBox.Show(this, $@"File saved to {sfd.FileName}! Would you like to open it now? (requires Excel)", @"Success", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start("EXCEL.EXE", sfd.FileName);
                }
            }
        }

        private void tsbProcess_Click(object sender, EventArgs e)
        {
            ExecuteMethod(ProcessAttributes);
        }
    }
}