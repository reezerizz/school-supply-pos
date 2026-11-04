namespace SchoolSupplyPOS
{
    partial class frmReceipt
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtReceipt = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // txtReceipt
            this.txtReceipt.Name = "txtReceipt";
            this.txtReceipt.BackColor = System.Drawing.Color.White;
            this.txtReceipt.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtReceipt.Location = new System.Drawing.Point(16, 16);
            this.txtReceipt.Multiline = true;
            this.txtReceipt.ReadOnly = true;
            this.txtReceipt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtReceipt.Size = new System.Drawing.Size(348, 390);
            // btnClose
            this.btnClose.Name = "btnClose";
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(34, 52, 216);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(264, 418);
            this.btnClose.Size = new System.Drawing.Size(100, 34);
            this.btnClose.Text = "Close";
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.Controls.Add(this.txtReceipt);
            this.Controls.Add(this.btnClose);
            // frmReceipt
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AcceptButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(380, 470);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmReceipt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Receipt";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtReceipt;
        private System.Windows.Forms.Button btnClose;
    }
}
