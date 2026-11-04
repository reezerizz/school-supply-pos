using System;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    public partial class frmReceipt : Form
    {
        public frmReceipt(string receiptText)
        {
            InitializeComponent();
            txtReceipt.Text = receiptText;
            txtReceipt.SelectionStart = 0;
            txtReceipt.SelectionLength = 0;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
