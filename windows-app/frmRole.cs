using System;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    public partial class frmRole : Form
    {
        public bool IsManager = false;

        public frmRole()
        {
            InitializeComponent();
        }

        private void btnManager_Click(object sender, EventArgs e)
        {
            IsManager = true;
            this.DialogResult = DialogResult.OK;
        }

        private void btnStaff_Click(object sender, EventArgs e)
        {
            IsManager = false;
            this.DialogResult = DialogResult.OK;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
