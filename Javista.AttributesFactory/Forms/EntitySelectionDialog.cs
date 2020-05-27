using Javista.AttributesFactory.AppCode;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SolutionInfo = Javista.AttributesFactory.AppCode.SolutionInfo;

namespace Javista.AttributesFactory.Forms
{
    public partial class EntitySelectionDialog : Form
    {
        private readonly IOrganizationService _service;
        private readonly List<SolutionInfo> _solutions;

        public EntitySelectionDialog(IOrganizationService service, List<SolutionInfo> solutions)
        {
            InitializeComponent();

            lvEntities.ListViewItemSorter = new ListViewItemComparer();

            _service = service;
            _solutions = solutions;
        }

        public List<EntityMetadata> Entities { get; private set; }
        public bool LoadAllAttributes { get; private set; }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            Entities = lvEntities.CheckedItems.Cast<ListViewItem>().Select(i => (EntityMetadata)i.Tag).ToList();
            LoadAllAttributes = chkLoadAllAttributes.Checked;
        }

        private void cbbSolutions_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (cbbSolutions.SelectedIndex < 0) return;

            var emds = MetadataManager.GetEntities(((SolutionInfo)cbbSolutions.SelectedItem).Id, _service);

            lvEntities.Items.Clear();
            lvEntities.Items.AddRange(emds.Select(emd =>
                new ListViewItem(emd.DisplayName?.UserLocalizedLabel?.Label ?? "N/A") { SubItems = { emd.LogicalName }, Tag = emd }).ToArray());
        }

        private void EntitySelectionDialog_Load(object sender, System.EventArgs e)
        {
            cbbSolutions.Items.AddRange(_solutions.ToArray());
        }

        private void llClearAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvEntities.Items)
            {
                item.Checked = false;
            }
        }

        private void llInvertSelection_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvEntities.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void llSelectAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem item in lvEntities.Items)
            {
                item.Checked = true;
            }
        }
    }
}