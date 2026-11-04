namespace SchoolSupplyPOS
{
    partial class frmRole
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
            this.pnlRed = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSub = new System.Windows.Forms.Label();
            this.lblChoose = new System.Windows.Forms.Label();
            this.btnManager = new System.Windows.Forms.Button();
            this.btnStaff = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // pnlRed
            this.pnlRed.Name = "pnlRed";
            this.pnlRed.BackColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.pnlRed.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRed.Size = new System.Drawing.Size(700, 14);
            // lblTitle
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "SCHOOL SUPPLIES POS";
            this.lblTitle.Location = new System.Drawing.Point(0, 62);
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.UseMnemonic = false;
            this.lblTitle.AutoSize = false;
            this.lblTitle.Size = new System.Drawing.Size(700, 50);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // lblSub
            this.lblSub.Name = "lblSub";
            this.lblSub.Text = "POINT OF SALE SYSTEM";
            this.lblSub.Location = new System.Drawing.Point(0, 116);
            this.lblSub.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSub.UseMnemonic = false;
            this.lblSub.AutoSize = false;
            this.lblSub.Size = new System.Drawing.Size(700, 26);
            this.lblSub.ForeColor = System.Drawing.Color.FromArgb(200, 208, 255);
            this.lblSub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // lblChoose
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Text = "Select your role";
            this.lblChoose.Location = new System.Drawing.Point(0, 176);
            this.lblChoose.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblChoose.UseMnemonic = false;
            this.lblChoose.AutoSize = false;
            this.lblChoose.Size = new System.Drawing.Size(700, 28);
            this.lblChoose.ForeColor = System.Drawing.Color.White;
            this.lblChoose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // btnManager
            this.btnManager.Name = "btnManager";
            this.btnManager.BackColor = System.Drawing.Color.White;
            this.btnManager.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManager.ForeColor = System.Drawing.Color.FromArgb(34, 52, 216);
            this.btnManager.Location = new System.Drawing.Point(110, 220);
            this.btnManager.Size = new System.Drawing.Size(230, 100);
            this.btnManager.Text = "Manager";
            this.btnManager.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnManager.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManager.UseVisualStyleBackColor = false;
            this.btnManager.Click += new System.EventHandler(this.btnManager_Click);
            // btnStaff
            this.btnStaff.Name = "btnStaff";
            this.btnStaff.BackColor = System.Drawing.Color.White;
            this.btnStaff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStaff.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.btnStaff.Location = new System.Drawing.Point(360, 220);
            this.btnStaff.Size = new System.Drawing.Size(230, 100);
            this.btnStaff.Text = "Staff";
            this.btnStaff.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStaff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStaff.UseVisualStyleBackColor = false;
            this.btnStaff.Click += new System.EventHandler(this.btnStaff_Click);
            // btnExit
            this.btnExit.Name = "btnExit";
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(34, 52, 216);
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(300, 350);
            this.btnExit.Size = new System.Drawing.Size(100, 34);
            this.btnExit.Text = "Exit";
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            this.Controls.Add(this.pnlRed);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSub);
            this.Controls.Add(this.lblChoose);
            this.Controls.Add(this.btnManager);
            this.Controls.Add(this.btnStaff);
            this.Controls.Add(this.btnExit);
            // frmRole
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(34, 52, 216);
            this.ClientSize = new System.Drawing.Size(700, 420);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmRole";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "School Supplies POS";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlRed;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.Button btnManager;
        private System.Windows.Forms.Button btnStaff;
        private System.Windows.Forms.Button btnExit;
    }
}
