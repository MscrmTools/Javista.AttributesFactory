using Javista.AttributesFactory.AppCode;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            ((DataGridViewComboBoxColumn)dgvTables.Columns[1]).Items.AddRange("User owned", "Organization owned");
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
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
                        MaxLength = 100,
                        FormatName = StringFormatName.Text,
                        DisplayName = new Microsoft.Xrm.Sdk.Label(((DataGridViewTextBoxCell)row.Cells[6]).Value.ToString(), _settings.LanguageCode)
                    }
                };

                requests.Add(createRequest);
            }

            btnCreateEntities.Enabled = false;
            dgvTables.Enabled = false;

            bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            bw.DoWork += (worker, evt) =>
            {
                foreach (var request in requests)
                {
                    if (((BackgroundWorker)worker).CancellationPending)
                    {
                        evt.Cancel = true;
                        return;
                    }

                    ((BackgroundWorker)worker).ReportProgress(0, $"Creating table {request.Entity.DisplayName.LocalizedLabels[0].Label}. Please wait...");

                    var result = (CreateEntityResponse)_service.Execute(request);

                    _service.Execute(new AddSolutionComponentRequest
                    {
                        ComponentType = 1,
                        ComponentId = result.EntityId,
                        SolutionUniqueName = _settings.Solution.UniqueName
                    });

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
    }
}