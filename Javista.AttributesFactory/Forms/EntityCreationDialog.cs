using Javista.AttributesFactory.AppCode;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Javista.AttributesFactory.Forms
{
    public partial class EntityCreationDialog : Form
    {
        private IOrganizationService _service;
        private CreateSettings _settings;
        private BackgroundWorker bw;

        public EntityCreationDialog(NewEntityInfo[] entities, CreateSettings settings, IOrganizationService service)
        {
            _service = service;
            _settings = settings;

            foreach (var entity in entities)
            {
                entity.PrimaryFieldDisplayName = "Name";
                entity.PrimaryFieldSchemaName = $"{settings.Solution.Prefix}_Name";
                entity.PrimaryFieldRequired = true;
                entity.PrimaryFieldLength = 100;
                entity.OwnershipType = "User owned";
            }

            InitializeComponent();

            dgvTables.AutoGenerateColumns = false;
            dgvTables.DataSource = entities;
            dgvTables.Columns[0].DataPropertyName = "SchemaName";
            dgvTables.Columns[1].DataPropertyName = "OwnershipType";
            dgvTables.Columns[2].DataPropertyName = "DisplayName";
            dgvTables.Columns[3].DataPropertyName = "DisplayCollectionName";
            dgvTables.Columns[4].DataPropertyName = "IsActivity";
            dgvTables.Columns[5].DataPropertyName = "PrimaryFieldSchemaName";
            dgvTables.Columns[6].DataPropertyName = "PrimaryFieldDisplayName";
            dgvTables.Columns[7].DataPropertyName = "PrimaryFieldLength";
            dgvTables.Columns[8].DataPropertyName = "PrimaryFieldRequired";

            ((DataGridViewComboBoxColumn)dgvTables.Columns[1]).Items.AddRange("User owned", "Organization owned");

            dgvTables.CellValueChanged += dgvTables_CellValueChanged;

            LoadApps();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (bw != null && bw.IsBusy)
            {
                bw.CancelAsync();
                toolStripStatusLabel1.Text = "Current table operation will end before cancellation. Please wait...";
            }
            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnCreateEntities_Click(object sender, EventArgs e)
        {
            bool allSet = true;

            foreach (DataGridViewRow row in dgvTables.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value == null)
                    {
                        allSet = false;
                        break;
                    }
                }
            }

            if (!allSet)
            {
                MessageBox.Show(this, "Please fill all cells", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var requests = new List<CreateEntityRequest>();

            foreach (DataGridViewRow row in dgvTables.Rows)
            {
                if (!int.TryParse(((DataGridViewTextBoxCell)row.Cells[7]).Value.ToString(), out int length))
                {
                    MessageBox.Show(this, "Primary column length format is not an integer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var createRequest = new CreateEntityRequest
                {
                    //Define the entity
                    Entity = new EntityMetadata
                    {
                        SchemaName = ((DataGridViewTextBoxCell)row.Cells[0]).Value.ToString(),
                        OwnershipType = ((DataGridViewComboBoxCell)row.Cells[1]).Value.ToString() == "User owned" ? OwnershipTypes.UserOwned : OwnershipTypes.OrganizationOwned,
                        DisplayName = new Microsoft.Xrm.Sdk.Label(((DataGridViewTextBoxCell)row.Cells[2]).Value.ToString(), _settings.LanguageCode),
                        DisplayCollectionName = new Microsoft.Xrm.Sdk.Label(((DataGridViewTextBoxCell)row.Cells[3]).Value.ToString(), _settings.LanguageCode),
                        IsActivity = (bool)((DataGridViewCheckBoxCell)row.Cells[4]).Value,
                    },

                    // Define the primary attribute for the entity
                    PrimaryAttribute = new StringAttributeMetadata
                    {
                        SchemaName = ((DataGridViewTextBoxCell)row.Cells[5]).Value.ToString(),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty((bool)((DataGridViewCheckBoxCell)row.Cells[8]).Value ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                        MaxLength = length,
                        FormatName = StringFormatName.Text,
                        DisplayName = new Microsoft.Xrm.Sdk.Label(((DataGridViewTextBoxCell)row.Cells[6]).Value.ToString(), _settings.LanguageCode)
                    }
                };

                requests.Add(createRequest);
            }

            var app = cbbApps.SelectedItem as MdAInfo;

            btnCreateEntities.Enabled = false;
            dgvTables.Enabled = false;

            bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            bw.DoWork += (worker, evt) =>
            {
                var sb = new StringBuilder();
                evt.Result = sb;

                foreach (var request in requests)
                {
                    if (((BackgroundWorker)worker).CancellationPending)
                    {
                        evt.Cancel = true;
                        return;
                    }

                    try
                    {
                        ((BackgroundWorker)worker).ReportProgress(0, $"Checking existence of table {request.Entity.DisplayName.LocalizedLabels[0].Label}...");

                        _service.Execute(new RetrieveEntityRequest
                        {
                            LogicalName = request.Entity.SchemaName.ToLower(),
                            EntityFilters = EntityFilters.Entity
                        });
                    }
                    catch
                    {
                        ((BackgroundWorker)worker).ReportProgress(0, $"Creating table {request.Entity.DisplayName.LocalizedLabels[0].Label}. Please wait...");

                        var result = (CreateEntityResponse)_service.Execute(request);

                        if (((BackgroundWorker)worker).CancellationPending)
                        {
                            evt.Cancel = true;
                            return;
                        }

                        try
                        {
                            ((BackgroundWorker)worker).ReportProgress(0, $"Adding table {request.Entity.DisplayName.LocalizedLabels[0].Label} to solution. Please wait...");

                            _service.Execute(new AddSolutionComponentRequest
                            {
                                ComponentType = 1,
                                ComponentId = result.EntityId,
                                SolutionUniqueName = _settings.Solution.UniqueName,
                                DoNotIncludeSubcomponents = false
                            });
                        }
                        catch
                        {
                            // We don't want to fail if adding to application fails
                            sb.AppendLine($"- Adding table {request.Entity.DisplayName.LocalizedLabels[0].Label} to solution");
                        }

                        if (app != null)
                        {
                            try
                            {
                                ((BackgroundWorker)worker).ReportProgress(0, $"Adding table {request.Entity.DisplayName.LocalizedLabels[0].Label} to application {app}. Please wait...");

                                _service.Execute(new AddAppComponentsRequest
                                {
                                    AppId = app.Id,
                                    Components = new EntityReferenceCollection
                                    {
                                        new EntityReference(request.Entity.SchemaName.ToLower(), result.EntityId)
                                    }
                                });
                            }
                            catch
                            {
                                // We don't want to fail if adding to solution fails
                                sb.AppendLine($"- Adding table {request.Entity.DisplayName.LocalizedLabels[0].Label} to application {app}");
                            }
                        }
                    }

                    if (((BackgroundWorker)worker).CancellationPending)
                    {
                        evt.Cancel = true;
                        return;
                    }
                }
            };
            bw.RunWorkerCompleted += (worker, evt) =>
            {
                btnCreateEntities.Enabled = true;
                dgvTables.Enabled = true;

                if (evt.Cancelled)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                if (evt.Error != null)
                {
                    MessageBox.Show(this, $"An error occured when creating tables: {evt.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (evt.Result != null)
                    {
                        MessageBox.Show(this, $"The following action did not succeed:\n{evt.Result}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
            bw.ProgressChanged += (worker, evt) =>
            {
                toolStripStatusLabel1.Text = evt.UserState.ToString();
            };
            bw.RunWorkerAsync();
        }

        private void dgvTables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                dgvTables.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value = $"{dgvTables.Rows[e.RowIndex].Cells[e.ColumnIndex].Value}s";
            }
            else if ((new List<int> { 5, 6, 7, 8 }).Contains(e.ColumnIndex) && dgvTables.Rows.Count > 1)
            {
                if (MessageBox.Show(this, "Do you want to apply this value to other row(s)?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    dgvTables.CellValueChanged -= dgvTables_CellValueChanged;
                    foreach (DataGridViewRow row in dgvTables.Rows)
                    {
                        if (row.Index == e.RowIndex) continue;

                        row.Cells[e.ColumnIndex].Value = dgvTables.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    }
                    dgvTables.CellValueChanged += dgvTables_CellValueChanged;
                }
            }
        }

        private void LoadApps()
        {
            bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            bw.DoWork += (worker, evt) =>
            {
                evt.Result = _service.RetrieveMultiple(new QueryExpression("appmodule")
                {
                    NoLock = true,
                    ColumnSet = new ColumnSet("name"),
                    Orders =
                {
                    new OrderExpression("name", OrderType.Ascending)
                }
                }).Entities.Select(e => new MdAInfo(e)).ToList();
            };
            bw.RunWorkerCompleted += (worker, evt) =>
            {
                cbbApps.Items.AddRange(((List<MdAInfo>)evt.Result).ToArray());
                cbbApps.Enabled = cbbApps.Items.Count > 0;
                cbbApps.Width = TextRenderer.MeasureText(MdAInfo.MaxLengthValue, cbbApps.Font).Width + 20;
                cbbApps.Top = pnlOptions.Height / 2 - cbbApps.Height / 2;
                lblAddToModelDrivenApp.SetAutoWidth();
            };

            bw.RunWorkerAsync();
        }
    }
}