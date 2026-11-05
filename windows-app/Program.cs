using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    // ===== Data classes =====
    class Product
    {
        public string Code, Name;
        public double Price;
        public int Stock;
    }

    class OrderItem
    {
        public Product Item;
        public string Name;
        public double Price;
        public int Qty;
        public double Total() { return Price * Qty; }
        public override string ToString()
        {
            return string.Format("{0,2} x {1,-15} {2,8:F2}", Qty, Name, Total());
        }
    }

    class Sale
    {
        public int No;
        public DateTime Time;
        public List<OrderItem> Lines = new List<OrderItem>();
        public double Total, Cash;
        public string Cashier;
        public bool Voided;

        public static string Money(double v) { return "PHP " + v.ToString("F2"); }

        public string Receipt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("====================================");
            sb.AppendLine("        SCHOOL SUPPLIES POS");
            sb.AppendLine("  Receipt #" + No + "   " + Time.ToString("MMM d, h:mm tt"));
            sb.AppendLine("====================================");
            foreach (OrderItem line in Lines) sb.AppendLine(line.ToString());
            sb.AppendLine("------------------------------------");
            sb.AppendLine("TOTAL:  " + Money(Total));
            sb.AppendLine("CASH:   " + Money(Cash));
            sb.AppendLine("CHANGE: " + Money(Cash - Total));
            sb.AppendLine("Cashier: " + Cashier);
            if (Voided) sb.AppendLine("*** VOIDED ***");
            sb.AppendLine("====================================");
            sb.AppendLine("     Thank you! Come again!");
            return sb.ToString();
        }
    }

    // ===== The one and only form =====
    class frmMain : Form
    {
        // colors (from the Figma design)
        Color Bg = Color.FromArgb(24, 24, 27);
        Color Side = Color.FromArgb(42, 45, 51);
        Color Card = Color.FromArgb(52, 55, 61);
        Color Blue = Color.FromArgb(59, 130, 246);
        Color Orange = Color.FromArgb(245, 158, 11);
        Color Green = Color.FromArgb(34, 197, 94);
        Color Red = Color.FromArgb(239, 68, 68);
        Color Navy = Color.FromArgb(30, 42, 90);
        Color Muted = Color.FromArgb(156, 163, 175);
        Color Dark = Color.FromArgb(24, 24, 27);

        // data
        List<Product> products = new List<Product>();
        List<Sale> sales = new List<Sale>();
        List<Sale> shown = new List<Sale>();     // the sales currently listed on the Records screen
        List<OrderItem> cart = new List<OrderItem>();
        int saleNo = 100;
        bool isManager = false;
        const string ManagerCode = "123";        // temporary code Staff must enter to void

        // screens
        Panel pnlLanding, pnlApp, pnlProducts, pnlSell, pnlRecords, pnlManage;

        // shell (sidebar and header)
        Label lblTitle, lblBadge, lblRoleName;
        Button navProducts, navSell, navRecords, navManage, navAccount;

        // controls we need to reach later
        DataGridView dgvView, dgvSell, dgvCart, dgvSales, dgvManage;
        TextBox txtQty, txtCash, txtCode, txtName, txtPrice, txtStock, txtAddStock, txtNewPrice, txtReceipt;
        Label lblTotal, lblChange, lblSelected, lblStatus;
        ComboBox cboFilter;

        public frmMain()
        {
            Text = "School Supplies POS";
            ClientSize = new Size(1100, 680);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            BackColor = Bg;

            LoadProducts();
            BuildLanding();
            BuildApp();
            BuildProductsPage();
            BuildSellPage();
            BuildRecordsPage();
            BuildManagePage();
            pnlLanding.Visible = true;
            pnlApp.Visible = false;
        }

        void LoadProducts()
        {
            AddProduct("P001", "Ballpen", 10, 100);
            AddProduct("P002", "Notebook", 35, 50);
            AddProduct("P003", "Pencil", 8, 80);
            AddProduct("P004", "Bond Paper", 120, 30);
            AddProduct("P005", "Yellow Pad", 45, 40);
        }

        void AddProduct(string code, string name, double price, int stock)
        {
            Product p = new Product();
            p.Code = code; p.Name = name; p.Price = price; p.Stock = stock;
            products.Add(p);
        }

        // ===== small helpers to create controls =====
        Panel MakePanel(Control parent, int x, int y, int w, int h, Color back)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y); p.Size = new Size(w, h); p.BackColor = back;
            parent.Controls.Add(p);
            return p;
        }

        Label MakeLabel(Control parent, string text, int x, int y, int w, int h, float size, bool bold, Color fore)
        {
            Label l = new Label();
            l.Text = text; l.Location = new Point(x, y); l.Size = new Size(w, h);
            l.Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
            l.ForeColor = fore; l.UseMnemonic = false;
            parent.Controls.Add(l);
            return l;
        }

        Button MakeButton(Control parent, string text, int x, int y, int w, int h, Color back, Color fore, EventHandler click)
        {
            Button b = new Button();
            b.Text = text; b.Location = new Point(x, y); b.Size = new Size(w, h);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.BackColor = back; b.ForeColor = fore;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            b.Cursor = Cursors.Hand; b.UseMnemonic = false;
            if (click != null) b.Click += click;
            parent.Controls.Add(b);
            return b;
        }

        TextBox MakeBox(Control parent, int x, int y, int w)
        {
            TextBox t = new TextBox();
            t.Location = new Point(x, y); t.Width = w;
            t.Font = new Font("Segoe UI", 10);
            t.BackColor = Bg; t.ForeColor = Color.White;
            t.BorderStyle = BorderStyle.FixedSingle;
            parent.Controls.Add(t);
            return t;
        }

        DataGridView MakeGrid(Control parent, int x, int y, int w, int h, string[] columns)
        {
            DataGridView g = new DataGridView();
            g.Location = new Point(x, y); g.Size = new Size(w, h);
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BorderStyle = BorderStyle.None;
            g.ScrollBars = ScrollBars.Vertical;     // columns always fit the table width
            g.BackgroundColor = Color.FromArgb(50, 52, 56);
            g.GridColor = Color.FromArgb(80, 83, 89);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 32;
            g.RowTemplate.Height = 28;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(66, 69, 75);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(66, 69, 75);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            g.DefaultCellStyle.BackColor = Color.FromArgb(58, 60, 64);
            g.DefaultCellStyle.ForeColor = Color.White;
            g.DefaultCellStyle.SelectionBackColor = Blue;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(50, 52, 56);
            foreach (string name in columns)
            {
                int i = g.Columns.Add(name, name);
                g.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                g.Columns[i].MinimumWidth = 20;   // keep rows in the same order as the lists
            }
            parent.Controls.Add(g);
            return g;
        }

        // index of the selected row, or -1
        int SelectedIndex(DataGridView g)
        {
            if (g.SelectedRows.Count == 0) return -1;
            return g.SelectedRows[0].Index;
        }

        // ===== Screen 1: landing =====
        void BuildLanding()
        {
            pnlLanding = MakePanel(this, 0, 0, 1100, 680, Bg);

            Panel left = MakePanel(pnlLanding, 0, 0, 528, 680, Side);
            MakeLabel(left, "SchoolPOS", 46, 270, 440, 64, 38, true, Blue);
            MakeLabel(left, "POINT OF SALES SYSTEM", 52, 336, 440, 30, 15, false, Color.White);
            MakeLabel(left, "Sales  ·  Inventory  ·  Records  ·  Reports", 46, 460, 440, 28, 11, false, Color.White);

            MakeLabel(pnlLanding, "WELCOME", 598, 190, 440, 48, 28, true, Blue);
            MakeLabel(pnlLanding, "Select your role to continue", 600, 240, 440, 34, 17, false, Color.White);

            MakeRoleCard(pnlLanding, 600, 306, "M", Navy, "Manager", "Products, prices, stock and sales records", btnManager_Click);
            MakeRoleCard(pnlLanding, 600, 412, "S", Orange, "Staff", "Process sales, view records, print receipts", btnStaff_Click);

            MakeButton(pnlLanding, "Exit", 600, 540, 80, 34, Bg, Muted, btnExit_Click);
        }

        // a clickable role card: the panel and everything on it share one click handler
        Panel MakeRoleCard(Control parent, int x, int y, string letter, Color badge, string title, string desc, EventHandler click)
        {
            Panel p = MakePanel(parent, x, y, 420, 76, Card);
            Label b = MakeLabel(p, letter, 14, 16, 44, 44, 18, true, Color.White);
            b.BackColor = badge;
            b.TextAlign = ContentAlignment.MiddleCenter;
            MakeLabel(p, title, 72, 14, 330, 26, 12, true, Color.White);
            MakeLabel(p, desc, 72, 42, 336, 22, 9, false, Muted);
            p.Cursor = Cursors.Hand;
            p.Click += click;
            foreach (Control c in p.Controls)
            {
                c.Cursor = Cursors.Hand;
                c.Click += click;
            }
            return p;
        }

        void btnManager_Click(object sender, EventArgs e) { StartSession(true); }
        void btnStaff_Click(object sender, EventArgs e) { StartSession(false); }
        void btnExit_Click(object sender, EventArgs e) { Close(); }

        void StartSession(bool manager)
        {
            isManager = manager;
            cart.Clear();
            txtCash.Text = "";
            txtQty.Text = "1";
            RefreshProducts(-1);
            RefreshCart();

            // header badge
            lblBadge.Text = manager ? "M" : "S";
            lblBadge.BackColor = manager ? Navy : Orange;
            lblRoleName.Text = manager ? "Manager" : "Staff";

            // sidebar: each role sees its own menu
            navProducts.Visible = manager;
            navManage.Visible = manager;
            navSell.Visible = !manager;
            if (manager)
            {
                navProducts.Top = 100; navRecords.Top = 156; navManage.Top = 212; navAccount.Top = 268;
            }
            else
            {
                navSell.Top = 100; navRecords.Top = 156; navAccount.Top = 212;
            }

            pnlLanding.Visible = false;
            pnlApp.Visible = true;
            if (manager) navProducts_Click(null, null);
            else navSell_Click(null, null);
        }

        // ===== The shell: sidebar + header, shared by both roles =====
        void BuildApp()
        {
            pnlApp = MakePanel(this, 0, 0, 1100, 680, Bg);

            Panel side = MakePanel(pnlApp, 0, 0, 220, 680, Side);
            MakeLabel(side, "SchoolPOS", 18, 12, 190, 30, 15, true, Blue);
            MakeLabel(side, "Supplies & Sales", 20, 40, 190, 18, 8, false, Color.White);
            navProducts = MakeNav(side, "View Products", navProducts_Click);
            navSell = MakeNav(side, "Process Sale", navSell_Click);
            navRecords = MakeNav(side, "View Record of Sales", navRecords_Click);
            navManage = MakeNav(side, "Change/Add Product", navManage_Click);
            navAccount = MakeNav(side, "Change Account", navAccount_Click);

            Panel hdr = MakePanel(pnlApp, 220, 0, 880, 64, Side);
            lblTitle = MakeLabel(hdr, "", 16, 16, 440, 32, 15, true, Color.White);
            Label lblDate = MakeLabel(hdr, DateTime.Now.ToString("MMMM d, yyyy"), 440, 20, 190, 24, 10, false, Color.White);
            lblDate.TextAlign = ContentAlignment.MiddleRight;
            MakePanel(hdr, 646, 14, 1, 36, Muted);
            lblBadge = MakeLabel(hdr, "M", 662, 17, 30, 30, 13, true, Color.White);
            lblBadge.TextAlign = ContentAlignment.MiddleCenter;
            MakeLabel(hdr, "Welcome,", 700, 20, 70, 24, 10, false, Color.White);
            lblRoleName = MakeLabel(hdr, "Manager", 768, 20, 100, 24, 10, true, Color.White);

            pnlProducts = MakePage();
            pnlSell = MakePage();
            pnlRecords = MakePage();
            pnlManage = MakePage();
        }

        Panel MakePage()
        {
            Panel p = MakePanel(pnlApp, 220, 64, 880, 616, Bg);
            p.Visible = false;
            return p;
        }

        Button MakeNav(Control parent, string text, EventHandler click)
        {
            Button b = MakeButton(parent, text, 16, 100, 188, 44, Side, Color.White, click);
            b.Font = new Font("Segoe UI", 10);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding = new Padding(14, 0, 0, 0);
            return b;
        }

        // shows one page, highlights its menu button and sets the header title
        void ShowPage(Panel page, Button active, string title)
        {
            pnlProducts.Visible = false;
            pnlSell.Visible = false;
            pnlRecords.Visible = false;
            pnlManage.Visible = false;
            page.Visible = true;

            Button[] all = { navProducts, navSell, navRecords, navManage, navAccount };
            foreach (Button b in all) b.BackColor = Side;
            if (active != null) active.BackColor = Blue;
            lblTitle.Text = title;
        }

        void navProducts_Click(object sender, EventArgs e) { RefreshProducts(-1); ShowPage(pnlProducts, navProducts, "Available school supplies"); }
        void navSell_Click(object sender, EventArgs e) { RefreshProducts(-1); ShowPage(pnlSell, navSell, "Process Sale"); }
        void navRecords_Click(object sender, EventArgs e) { RefreshRecords(null); ShowPage(pnlRecords, navRecords, "Record of Sales"); }
        void navManage_Click(object sender, EventArgs e) { RefreshProducts(-1); ShowPage(pnlManage, navManage, "Manage Products and Pricing"); }

        void navAccount_Click(object sender, EventArgs e)
        {
            cart.Clear();
            pnlApp.Visible = false;
            pnlLanding.Visible = true;
        }

        // ===== Page: View Products (Manager) =====
        void BuildProductsPage()
        {
            Panel card = MakePanel(pnlProducts, 24, 24, 832, 568, Card);
            dgvView = MakeGrid(card, 16, 16, 800, 536, new string[] { "Code", "Name", "Price", "Stock" });
        }

        // fills a products table from the list
        void FillProducts(DataGridView g)
        {
            g.Rows.Clear();
            foreach (Product p in products)
                g.Rows.Add(p.Code, p.Name, Sale.Money(p.Price), p.Stock);
            g.ClearSelection();
        }

        // refreshes all three product tables; keepRow re-selects a row on the Manage table
        void RefreshProducts(int keepRow)
        {
            FillProducts(dgvView);
            FillProducts(dgvSell);
            FillProducts(dgvManage);
            if (keepRow >= 0 && keepRow < dgvManage.Rows.Count) dgvManage.Rows[keepRow].Selected = true;
        }

        // ===== Page: Process Sale (Staff) =====
        void BuildSellPage()
        {
            Panel left = MakePanel(pnlSell, 24, 24, 500, 488, Card);
            Label t1 = MakeLabel(left, "Available Products", 0, 10, 500, 28, 13, true, Color.White);
            t1.TextAlign = ContentAlignment.MiddleCenter;
            dgvSell = MakeGrid(left, 16, 48, 468, 424, new string[] { "Code", "Name", "Price", "Stock" });

            Panel bar = MakePanel(pnlSell, 24, 524, 500, 68, Card);
            MakeLabel(bar, "Quantity:", 16, 20, 80, 26, 10, false, Color.White);
            txtQty = MakeBox(bar, 98, 20, 70);
            txtQty.Text = "1";
            MakeButton(bar, "Add to Cart", 188, 14, 296, 40, Orange, Dark, btnAddCart_Click);

            Panel cartCard = MakePanel(pnlSell, 540, 24, 316, 330, Card);
            Label t2 = MakeLabel(cartCard, "Shopping Cart", 0, 10, 316, 28, 13, true, Color.White);
            t2.TextAlign = ContentAlignment.MiddleCenter;
            dgvCart = MakeGrid(cartCard, 16, 46, 284, 230, new string[] { "Code", "Name", "Price", "Qty", "Total" });
            MakeButton(cartCard, "Remove selected", 16, 284, 138, 34, Side, Color.White, btnRemove_Click);
            MakeButton(cartCard, "Clear", 162, 284, 138, 34, Side, Color.White, btnClear_Click);

            Panel pay = MakePanel(pnlSell, 540, 366, 316, 226, Card);
            lblTotal = MakeLabel(pay, "Total: PHP 0.00", 16, 14, 284, 28, 13, true, Color.White);
            MakeLabel(pay, "Cash Payment", 16, 60, 120, 26, 10, false, Color.White);
            txtCash = MakeBox(pay, 140, 58, 160);
            txtCash.TextAlign = HorizontalAlignment.Right;
            txtCash.TextChanged += txtCash_TextChanged;
            lblChange = MakeLabel(pay, "Change: PHP 0.00", 16, 100, 284, 26, 10, false, Muted);
            MakeButton(pay, "Complete Transaction", 16, 146, 284, 52, Green, Dark, btnPay_Click);
        }

        void btnAddCart_Click(object sender, EventArgs e)
        {
            int row = SelectedIndex(dgvSell);
            if (row < 0) { MessageBox.Show("Click a product in the table first.", "No product selected"); return; }
            int qty;
            if (!int.TryParse(txtQty.Text, out qty) || qty <= 0)
            {
                MessageBox.Show("Quantity must be a whole number above 0.", "Invalid quantity");
                return;
            }

            Product p = products[row];
            OrderItem line = FindLine(p);
            int inCart = (line == null) ? 0 : line.Qty;
            if (inCart + qty > p.Stock)
            {
                MessageBox.Show("Only " + p.Stock + " " + p.Name + " in stock.", "Not enough stock");
                return;
            }
            if (line == null)
            {
                line = new OrderItem();
                line.Item = p; line.Name = p.Name; line.Price = p.Price; line.Qty = 0;
                cart.Add(line);
            }
            line.Qty += qty;
            txtQty.Text = "1";
            RefreshCart();
        }

        OrderItem FindLine(Product p)
        {
            foreach (OrderItem line in cart)
                if (line.Item == p) return line;
            return null;
        }

        double CartTotal()
        {
            double total = 0;
            foreach (OrderItem line in cart) total += line.Total();
            return total;
        }

        void RefreshCart()
        {
            dgvCart.Rows.Clear();
            foreach (OrderItem line in cart)
                dgvCart.Rows.Add(line.Item.Code, line.Name, line.Price.ToString("F2"), line.Qty, line.Total().ToString("F2"));
            dgvCart.ClearSelection();
            lblTotal.Text = "Total: " + Sale.Money(CartTotal());
            UpdateChange();
        }

        void UpdateChange()
        {
            double cash;
            if (double.TryParse(txtCash.Text, out cash) && cart.Count > 0 && cash >= CartTotal())
            {
                lblChange.Text = "Change: " + Sale.Money(cash - CartTotal());
                lblChange.ForeColor = Green;
            }
            else
            {
                lblChange.Text = "Change: PHP 0.00";
                lblChange.ForeColor = Muted;
            }
        }

        void txtCash_TextChanged(object sender, EventArgs e) { UpdateChange(); }

        void btnRemove_Click(object sender, EventArgs e)
        {
            int row = SelectedIndex(dgvCart);
            if (row < 0) { MessageBox.Show("Click a cart row first."); return; }
            cart.RemoveAt(row);
            RefreshCart();
        }

        void btnClear_Click(object sender, EventArgs e)
        {
            cart.Clear();
            txtCash.Text = "";
            RefreshCart();
        }

        // ===== completing a sale: all checks first, change data last =====
        Sale CompleteSale(double cash, out string error)
        {
            error = "";
            if (cart.Count == 0) { error = "The cart is empty."; return null; }
            foreach (OrderItem line in cart)
            {
                if (line.Qty > line.Item.Stock)
                {
                    error = "Not enough stock for " + line.Name + ".";
                    return null;
                }
            }
            if (cash < CartTotal()) { error = "Cash is less than the total."; return null; }

            Sale s = new Sale();
            saleNo++;
            s.No = saleNo; s.Time = DateTime.Now; s.Cash = cash;
            s.Total = CartTotal(); s.Cashier = lblRoleName.Text;
            foreach (OrderItem line in cart)
            {
                line.Item.Stock -= line.Qty;
                s.Lines.Add(line);
            }
            sales.Add(s);
            cart = new List<OrderItem>();
            return s;
        }

        void btnPay_Click(object sender, EventArgs e)
        {
            double cash;
            if (!double.TryParse(txtCash.Text, out cash))
            {
                MessageBox.Show("Enter the cash payment.", "Cash needed");
                return;
            }
            string error;
            Sale s = CompleteSale(cash, out error);
            if (s == null)
            {
                MessageBox.Show(error, "Cannot complete transaction");
                return;
            }
            txtCash.Text = "";
            RefreshCart();
            RefreshProducts(-1);
            ShowReceipt(s.Receipt());
        }

        // ===== Page: Record of Sales (both roles) =====
        void BuildRecordsPage()
        {
            Panel left = MakePanel(pnlRecords, 24, 24, 520, 568, Card);
            cboFilter = new ComboBox();
            cboFilter.Location = new Point(16, 14); cboFilter.Width = 220;
            cboFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFilter.FlatStyle = FlatStyle.Flat;
            cboFilter.BackColor = Side; cboFilter.ForeColor = Color.White;
            cboFilter.Items.Add("All sales");
            cboFilter.Items.Add("Completed only");
            cboFilter.Items.Add("Voided only");
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += cboFilter_SelectedIndexChanged;
            left.Controls.Add(cboFilter);
            dgvSales = MakeGrid(left, 16, 54, 488, 498, new string[] { "Receipt", "Time", "Total", "Cashier", "Status" });
            dgvSales.SelectionChanged += dgvSales_SelectionChanged;

            Panel right = MakePanel(pnlRecords, 560, 24, 296, 568, Card);
            MakeLabel(right, "Receipt", 16, 14, 120, 26, 12, true, Color.White);
            lblStatus = MakeLabel(right, "", 140, 14, 140, 26, 10, true, Muted);
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            txtReceipt = new TextBox();
            txtReceipt.Multiline = true; txtReceipt.ReadOnly = true; txtReceipt.TabStop = false;
            txtReceipt.Font = new Font("Consolas", 8.5f);
            txtReceipt.BackColor = Bg; txtReceipt.ForeColor = Color.White;
            txtReceipt.BorderStyle = BorderStyle.FixedSingle;
            txtReceipt.Location = new Point(16, 50); txtReceipt.Size = new Size(264, 420);
            right.Controls.Add(txtReceipt);
            MakeButton(right, "Void this sale", 16, 486, 126, 46, Red, Color.White, btnVoidSel_Click);
            MakeButton(right, "Reprint", 154, 486, 126, 46, Side, Color.White, btnReprint_Click);
        }

        void cboFilter_SelectedIndexChanged(object sender, EventArgs e) { RefreshRecords(null); }

        // fills the table using the filter; keeps the given sale selected if it is still shown
        void RefreshRecords(Sale keep)
        {
            shown.Clear();
            for (int i = sales.Count - 1; i >= 0; i--)      // newest first
            {
                Sale s = sales[i];
                if (cboFilter.SelectedIndex == 1 && s.Voided) continue;
                if (cboFilter.SelectedIndex == 2 && !s.Voided) continue;
                shown.Add(s);
            }

            dgvSales.Rows.Clear();
            foreach (Sale s in shown)
                dgvSales.Rows.Add("#" + s.No, s.Time.ToString("MMM d, h:mm tt"), Sale.Money(s.Total), s.Cashier, s.Voided ? "VOID" : "Completed");

            dgvSales.ClearSelection();
            if (keep != null && shown.Contains(keep)) dgvSales.Rows[shown.IndexOf(keep)].Selected = true;
            ShowSelectedSale();
        }

        void dgvSales_SelectionChanged(object sender, EventArgs e) { ShowSelectedSale(); }

        Sale SelectedSale()
        {
            int row = SelectedIndex(dgvSales);
            if (row < 0 || row >= shown.Count) return null;
            return shown[row];
        }

        void ShowSelectedSale()
        {
            Sale s = SelectedSale();
            if (s == null)
            {
                txtReceipt.Text = "";
                lblStatus.Text = (shown.Count == 0) ? "No sales yet" : "Select a sale";
                lblStatus.ForeColor = Muted;
                return;
            }
            txtReceipt.Text = s.Receipt();
            lblStatus.Text = s.Voided ? "VOIDED" : "Completed";
            lblStatus.ForeColor = s.Voided ? Red : Green;
        }

        void VoidSale(Sale s)
        {
            foreach (OrderItem line in s.Lines) line.Item.Stock += line.Qty;
            s.Voided = true;
        }

        void btnVoidSel_Click(object sender, EventArgs e)
        {
            Sale s = SelectedSale();
            if (s == null) { MessageBox.Show("Select a sale first."); return; }
            if (s.Voided) { MessageBox.Show("That sale is already voided."); return; }
            DialogResult r = MessageBox.Show("Void receipt #" + s.No + " (" + Sale.Money(s.Total) + ")?\nThe items will go back in stock.", "Void sale", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes) return;
            if (!ManagerApproved()) return;
            VoidSale(s);
            RefreshProducts(-1);
            RefreshRecords(s);
        }

        void btnReprint_Click(object sender, EventArgs e)
        {
            Sale s = SelectedSale();
            if (s == null) { MessageBox.Show("Select a sale first."); return; }
            ShowReceipt(s.Receipt());
        }

        // Managers pass straight through. Staff must type the manager code.
        bool ManagerApproved()
        {
            if (isManager) return true;

            Form f = new Form();
            f.Text = "Manager approval";
            f.ClientSize = new Size(360, 190);
            f.BackColor = Card;
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false; f.MinimizeBox = false;

            MakeLabel(f, "Manager approval needed", 20, 16, 320, 28, 13, true, Color.White);
            MakeLabel(f, "Ask your manager to enter the code.", 20, 46, 320, 24, 10, false, Muted);
            TextBox code = MakeBox(f, 20, 80, 320);
            code.UseSystemPasswordChar = true;

            Button ok = MakeButton(f, "Approve", 20, 128, 150, 42, Orange, Dark, null);
            ok.DialogResult = DialogResult.OK;
            Button cancel = MakeButton(f, "Cancel", 190, 128, 150, 42, Side, Color.White, null);
            cancel.DialogResult = DialogResult.Cancel;
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            if (f.ShowDialog() != DialogResult.OK) return false;
            if (code.Text != ManagerCode)
            {
                MessageBox.Show("Wrong code. Nothing was voided.", "Access denied");
                return false;
            }
            return true;
        }

        void ShowReceipt(string text)
        {
            Form f = new Form();
            f.Text = "Receipt";
            f.ClientSize = new Size(380, 480);
            f.BackColor = Card;
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false; f.MinimizeBox = false;

            TextBox t = new TextBox();
            t.Multiline = true; t.ReadOnly = true; t.TabStop = false;
            t.Font = new Font("Consolas", 10);
            t.BackColor = Bg; t.ForeColor = Color.White;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Text = text;
            t.Location = new Point(10, 10); t.Size = new Size(360, 410);
            f.Controls.Add(t);

            Button ok = MakeButton(f, "Close", 120, 430, 140, 38, Blue, Color.White, null);
            ok.DialogResult = DialogResult.OK;
            f.AcceptButton = ok;
            f.ShowDialog();
        }

        // ===== Page: Change/Add Product (Manager) =====
        void BuildManagePage()
        {
            Panel left = MakePanel(pnlManage, 24, 24, 500, 568, Card);
            dgvManage = MakeGrid(left, 16, 16, 468, 536, new string[] { "Code", "Name", "Price", "Stock" });
            dgvManage.SelectionChanged += dgvManage_SelectionChanged;

            Panel add = MakePanel(pnlManage, 540, 24, 316, 290, Card);
            Label t1 = MakeLabel(add, "Add New Product", 0, 10, 316, 26, 12, true, Color.White);
            t1.TextAlign = ContentAlignment.MiddleCenter;
            MakeLabel(add, "Code:", 16, 52, 64, 26, 10, false, Color.White);
            MakeLabel(add, "Name:", 16, 88, 64, 26, 10, false, Color.White);
            MakeLabel(add, "Price:", 16, 124, 64, 26, 10, false, Color.White);
            MakeLabel(add, "Stock:", 16, 160, 64, 26, 10, false, Color.White);
            txtCode = MakeBox(add, 84, 50, 216);
            txtName = MakeBox(add, 84, 86, 216);
            txtPrice = MakeBox(add, 84, 122, 216);
            txtStock = MakeBox(add, 84, 158, 216);
            MakeButton(add, "Add Product", 84, 210, 216, 40, Orange, Dark, btnAddProduct_Click);

            Panel upd = MakePanel(pnlManage, 540, 330, 316, 262, Card);
            Label t2 = MakeLabel(upd, "Update Selected Product", 0, 10, 316, 26, 12, true, Color.White);
            t2.TextAlign = ContentAlignment.MiddleCenter;
            lblSelected = MakeLabel(upd, "Click a product in the table", 16, 44, 284, 24, 10, false, Muted);
            MakeLabel(upd, "+ Stock:", 16, 90, 70, 26, 10, false, Color.White);
            txtAddStock = MakeBox(upd, 96, 88, 100);
            MakeButton(upd, "Add Stock", 206, 84, 94, 34, Orange, Dark, btnAddStock_Click);
            MakeLabel(upd, "New price:", 16, 146, 80, 26, 10, false, Color.White);
            txtNewPrice = MakeBox(upd, 96, 144, 100);
            MakeButton(upd, "Set Price", 206, 140, 94, 34, Orange, Dark, btnSetPrice_Click);
        }

        void dgvManage_SelectionChanged(object sender, EventArgs e)
        {
            int row = SelectedIndex(dgvManage);
            if (row < 0 || row >= products.Count) lblSelected.Text = "Click a product in the table";
            else lblSelected.Text = "Selected: " + products[row].Code + "  " + products[row].Name;
        }

        void btnAddProduct_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();
            double price; int stock;
            if (code == "" || name == "")
            {
                MessageBox.Show("Enter a code and a name.", "Missing information");
                return;
            }
            if (!double.TryParse(txtPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Price must be a number above 0.", "Invalid price");
                return;
            }
            if (!int.TryParse(txtStock.Text, out stock) || stock < 0)
            {
                MessageBox.Show("Stock must be a whole number, 0 or more.", "Invalid stock");
                return;
            }
            foreach (Product p in products)
            {
                if (p.Code.ToLower() == code.ToLower())
                {
                    MessageBox.Show("That code is already used.", "Duplicate code");
                    return;
                }
            }
            AddProduct(code, name, price, stock);
            txtCode.Text = ""; txtName.Text = ""; txtPrice.Text = ""; txtStock.Text = "";
            RefreshProducts(products.Count - 1);
        }

        void btnAddStock_Click(object sender, EventArgs e)
        {
            int row = SelectedIndex(dgvManage);
            if (row < 0) { MessageBox.Show("Click a product in the table first."); return; }
            int amount;
            if (!int.TryParse(txtAddStock.Text, out amount) || amount <= 0)
            {
                MessageBox.Show("Enter a whole number above 0.", "Invalid amount");
                return;
            }
            products[row].Stock += amount;
            txtAddStock.Text = "";
            RefreshProducts(row);
        }

        void btnSetPrice_Click(object sender, EventArgs e)
        {
            int row = SelectedIndex(dgvManage);
            if (row < 0) { MessageBox.Show("Click a product in the table first."); return; }
            double price;
            if (!double.TryParse(txtNewPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Price must be a number above 0.", "Invalid price");
                return;
            }
            products[row].Price = price;
            txtNewPrice.Text = "";
            RefreshProducts(row);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
