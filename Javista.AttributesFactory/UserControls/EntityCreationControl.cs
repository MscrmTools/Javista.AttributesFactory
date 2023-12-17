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

namespace Javista.AttributesFactory.UserControls
{
    public partial class EntityCreationControl : UserControl
    {
        private IOrganizationService _service;
        private CreateSettings _settings;
        private BackgroundWorker bw;

        public EntityCreationControl(NewEntityInfo[] entities, CreateSettings settings, IOrganizationService service)
        {
            InitializeComponent();

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

            LoadApps();
        }

        public event EventHandler OnCancel;

        public event EventHandler OnCompleted;

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (bw != null && bw.IsBusy)
            {
                bw.CancelAsync();
                toolStripStatusLabel1.Text = "Current table operation will end before cancellation. Please wait...";
            }
            else
            {
                OnCancel?.Invoke(this, new EventArgs());
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

                var addToSolutionRequests = new List<AddSolutionComponentRequest>();
                var addToAppRequests = new List<AddAppComponentsRequest>();

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
                        request.Entity.MetadataId = result.EntityId;

                        if (((BackgroundWorker)worker).CancellationPending)
                        {
                            evt.Cancel = true;
                            return;
                        }

                        addToSolutionRequests.Add(new AddSolutionComponentRequest
                        {
                            ComponentType = 1,
                            ComponentId = result.EntityId,
                            SolutionUniqueName = _settings.Solution.UniqueName,
                            AddRequiredComponents = false,
                            DoNotIncludeSubcomponents = false,
                            IncludedComponentSettingsValues = null
                        });

                        ((BackgroundWorker)worker).ReportProgress(0, $"Adding table {request.Entity.DisplayName.LocalizedLabels[0].Label} to solution. Please wait...");

                        if (app != null)
                        {
                            addToAppRequests.Add(new AddAppComponentsRequest
                            {
                                AppId = app.Id,
                                Components = new EntityReferenceCollection
                                    {
                                        new EntityReference(request.Entity.SchemaName.ToLower(), result.EntityId)
                                    }
                            });
                        }
                    }

                    if (((BackgroundWorker)worker).CancellationPending)
                    {
                        evt.Cancel = true;
                        return;
                    }
                }

                foreach (var request in addToSolutionRequests)
                {
                    var tableName = requests.First(r => r.Entity.MetadataId.Value == request.ComponentId).Entity.DisplayName.LocalizedLabels[0].Label;

                    try
                    {
                        ((BackgroundWorker)worker).ReportProgress(0, $"Adding table {tableName} to solution. Please wait...");
                        _service.Execute(request);
                    }
                    catch
                    {
                        // We don't want to fail if adding to application fails
                        sb.AppendLine($"- Adding table {tableName} to solution");
                    }

                    if (((BackgroundWorker)worker).CancellationPending)
                    {
                        evt.Cancel = true;
                        return;
                    }
                }

                foreach (var request in addToAppRequests)
                {
                    try
                    {
                        ((BackgroundWorker)worker).ReportProgress(0, $"Adding table {requests.First(r => r.Entity.MetadataId.Value == request.Components.First().Id).Entity.DisplayName.LocalizedLabels[0].Label} to application {app}. Please wait...");

                        _service.Execute(request);
                    }
                    catch
                    {
                        // We don't want to fail if adding to solution fails
                        sb.AppendLine($"- Adding table {requests.First(r => r.Entity.MetadataId.Value == request.Components.First().Id).Entity.DisplayName.LocalizedLabels[0].Label} to application {app}");
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
                    OnCancel?.Invoke(this, new EventArgs());
                    return;
                }

                if (evt.Error != null)
                {
                    MessageBox.Show(this, $"An error occured when creating tables: {evt.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (evt.Result != null && ((StringBuilder)evt.Result).Length > 0)
                    {
                        MessageBox.Show(this, $"The following action did not succeed:\n{evt.Result}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    OnCompleted?.Invoke(this, new EventArgs());
                }
            };
            bw.ProgressChanged += (worker, evt) =>
            {
                toolStripStatusLabel1.Text = evt.UserState.ToString();
            };
            bw.RunWorkerAsync();
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (btnMaximize.Text == "Maximize")
            {
                btnMaximize.Text = "Reduce";
                Width = Parent.Width;
                Height = Parent.Height;
                Location = new System.Drawing.Point(0, 0);
            }
            else
            {
                btnMaximize.Text = "Maximize";
                Width = Convert.ToInt32(Parent.Width * 0.7);
                Height = Convert.ToInt32(Parent.Height * 0.7);
                Location = new System.Drawing.Point(Parent.Width / 2 - Width / 2, Parent.Height / 2 - Height / 2);
            }
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

        private void EntityCreationControl_Load(object sender, EventArgs e)
        {
            Width = Convert.ToInt32(Parent.Width * 0.7);
            Height = Convert.ToInt32(Parent.Height * 0.7);
            Location = new System.Drawing.Point(Parent.Width / 2 - Width / 2, Parent.Height / 2 - Height / 2);
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