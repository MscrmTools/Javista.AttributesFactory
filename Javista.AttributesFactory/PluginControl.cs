using Javista.AttributesFactory.AppCode;
using Javista.AttributesFactory.Forms;
using Javista.AttributesFactory.UserControls;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
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

            var tt = new ToolTip() { IsBalloon = true, ToolTipIcon = ToolTipIcon.Info, ToolTipTitle = "Delay between operations" };
            tt.SetToolTip(lblDelay, "Time to wait (in seconds) between each attribute operation. This is to ensure the backend server is not sollicited too much");
            tt.SetToolTip(nudDelay, "Time to wait (in seconds) between each attribute operation. This is to ensure the backend server is not sollicited too much");
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
                AddOptionSetSuffix = chkAddOptionSetSuffix.Checked,
                ThrottleInSeconds = Convert.ToInt32(nudDelay.Value)
            };

            var manager = new MetadataUpsertManager(settings, Service, ConnectionDetail.OrganizationMajorVersion);

            var keysToDelete = manager.CheckKeysDependencies();
            if (keysToDelete.Count > 0)
            {
                if (DialogResult.No == MessageBox.Show(this, "The following keys will be deleted in order to delete attributes :\n- " + string.Join("\n- ", keysToDelete.Select(e => e.DisplayName.LocalizedLabels[0].Label)) + "\n\nDo you want to continue?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    return;
                }

                settings.KeysToDelete = keysToDelete;
            }

            SetWorkingState(true);

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Checking for missing tables...",
                Work = (bw, e) =>
                {
                    e.Result = MetadataManager.GetNotExistingEntities(manager.GetEntities(), Service);
                },
                PostWorkCallBack = evt =>
                {
                    SetWorkingState(false);

                    if (evt.Error != null)
                    {
                        MessageBox.Show(this, "An error occured when checking tables: " + evt.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var entities = (List<string>)evt.Result;

                    if (entities.Count > 0)
                    {
                        var ctrl = new EntityCreationControl(entities.Select(e => new NewEntityInfo { SchemaName = e }).ToArray(), settings, Service);
                        ctrl.OnCancel += (sender, e) => { Controls.Remove(ctrl); ctrl.Dispose(); toolStripMenu.Enabled = true; };
                        ctrl.OnCompleted += (sender, e) =>
                        {
                            Controls.Remove(ctrl);
                            ctrl.Dispose();
                            toolStripMenu.Enabled = true;
                            DoProcessAttributes(manager, settings);
                        };
                        Controls.Add(ctrl);
                        ctrl.BringToFront();

                        toolStripMenu.Enabled = false;
                    }
                    else
                    {
                        DoProcessAttributes(manager, settings);
                    }
                }
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
            using (var ofd = new OpenFileDialog
            {
                Filter = @"Excel spreadsheet with macros|*.xlsm|Excel spreadsheet|*.xlsx|All files|*.*"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                }
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

        private void cmsLogs_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == deleteAlternateKeyTsmi)
            {
                if (DialogResult.No == MessageBox.Show(this, "Are you sure you want to delete this alternate key?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    return;
                }

                if (lvLogs.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, "Please select an alternate key to delete", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var item = lvLogs.SelectedItems[0];
                var data = (ProcessResult)item.Tag;

                SetWorkingState(true);

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Deleting alternate key...",
                    Work = (bw, evt) =>
                    {
                        Service.Execute(new DeleteEntityKeyRequest
                        {
                            EntityLogicalName = data.Entity,
                            Name = data.Attribute
                        });
                    },
                    PostWorkCallBack = evt =>
                    {
                        SetWorkingState(false);

                        if (evt.Error != null)
                        {
                            MessageBox.Show(this, $"An error occured when deleting the alternate key: {evt.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                });
            }
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
                e.Graphics.DrawString($"{solution} {solution.Version}", e.Font, sb, r1);
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

        private void DoProcessAttributes(MetadataUpsertManager manager, CreateSettings settings)
        {
            lvLogs.Items.Clear();
            SetWorkingState(true);

            WorkAsync(new WorkAsyncInfo
            {
                Message = string.Empty,
                AsyncArgument = settings,
                Work = (w, e) =>
                {
                    manager.Process(w, ConnectionDetail, settings.KeysToDelete);
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
                            item.SubItems[5].Text = info.IsDelete ? "Deleted" : info.IsCreate ? @"Created" : @"Updated";
                            item.SubItems[6].Text = "";
                        }
                        else
                        {
                            item.ImageIndex = 2;
                            item.SubItems[3].Text = info.Attribute;
                            item.SubItems[5].Text = $@"{(info.IsDelete ? "Delete" : info.IsCreate ? "Create" : "Update")} Error";
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
                        item.SubItems.Add(info.Processing ? "Processing..." : info.Success ? info.IsDelete ? "Deleted" : info.IsCreate ? "Created" : "Updated" : info.IsDelete ? "Delete Error" : info.IsCreate ? "Create Error" : "Update Error");
                        item.SubItems.Add(info.IsDelete ? $"Deleting {(info.Type == "Many to many" ? "relationship" : info.Type == "Alternate key" ? "Alternate key" : "column")}" : "");

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

        private void exportLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = @"CSV file|*.csv"
            })
            {
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    lvLogs.ToCsv(sfd.FileName);
                }
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

        private void LvLogs_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var item = lvLogs.GetItemAt(e.X, e.Y);
                if (item != null && item.Tag is ProcessResult pr && pr.IsCreate == false && pr.IsDelete == false && pr.Type == "Alternate key")
                {
                    cmsLogs.Show(lvLogs, e.Location);
                }
            }
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

            tsbExportEntities.Visible = !working;

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
            using (var dialog = new EntitySelectionDialog(Service, _solutions))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (var sfd = new SaveFileDialog
                    {
                        Filter = @"Excel workbook with macro|*.xlsm"
                    })
                    {
                        if (sfd.ShowDialog(this) == DialogResult.OK)
                        {
                            var startDate = DateTime.Now;

                            WorkAsync(new WorkAsyncInfo
                            {
                                Message = "Exporting tables...",
                                Work = (bw, evt) =>
                                {
                                    var mdg = new MetadataDocManager(Service, this);
                                    mdg.GenerateDocumentation(dialog.Entities, sfd.FileName, dialog.LoadAllAttributes, dialog.LoadDerivedAttributes, bw);
                                },
                                ProgressChanged = evt =>
                                {
                                    SetWorkingMessage(evt.UserState.ToString());
                                },
                                PostWorkCallBack = evt =>
                                {
                                    var duration = DateTime.Now.Subtract(startDate);

                                    if (evt.Error != null)
                                    {
                                        MessageBox.Show(this, $"An error occured when generating the document: {evt.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    if (MessageBox.Show(this, $@"{dialog.Entities.Count} tables exported in {duration:hh\:mm\:ss}.

Do you want to open the document now?", @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        Process.Start(sfd.FileName);
                                    }
                                }
                            });
                        }
                    }
                }
            }
        }

        private void tsbGetTemplate_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = @"Excel workbook with macro|*.xlsm",
                FileName = "Attributes_Template.xlsm"
            })

            {
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "Javista.AttributesFactory.Template.Attributes_Template.xlsm";

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
        }

        private void tsbProcess_Click(object sender, EventArgs e)
        {
            ExecuteMethod(ProcessAttributes);
        }
    }
}