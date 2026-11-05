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
        public string Name, Category;
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

        public override string ToString()
        {
            string s = string.Format("#{0}   {1:h:mm tt}   P{2:F2}", No, Time, Total);
            if (Voided) s += "   VOID";
            return s;
        }

        public string Receipt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("====================================");
            sb.AppendLine("        SCHOOL SUPPLIES POS");
            sb.AppendLine("  Receipt #" + No + "   " + Time.ToString("MMM d, h:mm tt"));
            sb.AppendLine("====================================");
            foreach (OrderItem line in Lines) sb.AppendLine(line.ToString());
            sb.AppendLine("------------------------------------");
            sb.AppendLine("TOTAL:  P" + Total.ToString("F2"));
            sb.AppendLine("CASH:   P" + Cash.ToString("F2"));
            sb.AppendLine("CHANGE: P" + (Cash - Total).ToString("F2"));
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
        // colors
        Color Blue = Color.FromArgb(34, 52, 216);
        Color Red = Color.FromArgb(220, 38, 38);
        Color Green = Color.FromArgb(22, 163, 74);
        Color Dark = Color.FromArgb(17, 24, 39);
        Color Grey = Color.FromArgb(107, 114, 128);
        Color Light = Color.FromArgb(243, 244, 246);

        // data
        List<Product> products = new List<Product>();
        List<Sale> sales = new List<Sale>();
        List<OrderItem> cart = new List<OrderItem>();
        int saleNo = 100;
        string category = "All";
        int editRow = -1;

        // screens
        Panel pnlLanding, pnlPos, pnlManage, pnlHistory;

        // POS screen controls
        Label lblRole, lblTotal, lblChange;
        Panel pnlCats;
        FlowLayoutPanel pnlTiles;
        ListBox lstCart;
        TextBox txtCash;
        Button btnManage, btnHistory;

        // Manage screen controls
        DataGridView dgvProducts;
        TextBox txtName, txtPrice, txtStock, txtReceipt;
        ComboBox cboCat;
        // Transactions screen controls
        ListBox lstSales;
        ComboBox cboFilter;
        Label lblStatus;

        public frmMain()
        {
            Text = "School Supplies POS";
            ClientSize = new Size(1100, 680);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            LoadProducts();
            BuildLanding();
            BuildPos();
            BuildManage();
            BuildHistory();
            ShowScreen(pnlLanding);
        }

        void LoadProducts()
        {
            AddProduct("Notebook", "Paper", 35, 50);
            AddProduct("Bond Paper (ream)", "Paper", 280, 15);
            AddProduct("Folder", "Paper", 7, 70);
            AddProduct("Ballpen", "Writing", 12, 100);
            AddProduct("Pencil", "Writing", 8, 80);
            AddProduct("Eraser", "Writing", 10, 60);
            AddProduct("Crayons", "Art", 60, 20);
            AddProduct("Ruler", "Tools", 15, 40);
            AddProduct("Scissors", "Tools", 45, 8);
            AddProduct("Glue", "Tools", 25, 30);
        }

        void AddProduct(string name, string cat, double price, int stock)
        {
            Product p = new Product();
            p.Name = name; p.Category = cat; p.Price = price; p.Stock = stock;
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

        TextBox MakeBox(Control parent, int x, int y, int w, float size)
        {
            TextBox t = new TextBox();
            t.Location = new Point(x, y); t.Width = w;
            t.Font = new Font("Segoe UI", size);
            parent.Controls.Add(t);
            return t;
        }

        void ShowScreen(Panel screen)
        {
            pnlLanding.Visible = false;
            pnlPos.Visible = false;
            pnlManage.Visible = false;
            pnlHistory.Visible = false;
            screen.Visible = true;
        }

        // ===== Screen 1: landing =====
        void BuildLanding()
        {
            pnlLanding = MakePanel(this, 0, 0, 1100, 680, Color.White);

            Panel left = MakePanel(pnlLanding, 0, 0, 520, 680, Blue);
            MakePanel(left, 60, 205, 70, 8, Red);
            MakeLabel(left, "SchoolPOS", 54, 220, 440, 80, 44, true, Color.White);
            MakeLabel(left, "Point of sale for school supplies", 60, 305, 440, 32, 15, false, Color.FromArgb(200, 210, 255));
            MakeLabel(left, "Fast, simple checkout.", 60, 600, 440, 28, 11, false, Color.FromArgb(160, 175, 255));

            MakeLabel(pnlLanding, "Welcome back", 600, 160, 440, 56, 30, true, Dark);
            MakeLabel(pnlLanding, "Choose your role to open the register.", 602, 220, 440, 28, 12, false, Grey);

            Button m = MakeButton(pnlLanding, "MANAGER\nSell, manage items, void sales", 602, 290, 400, 90, Blue, Color.White, btnManager_Click);
            m.Font = new Font("Segoe UI", 13, FontStyle.Bold);

            Button s = MakeButton(pnlLanding, "STAFF\nRun the register", 602, 396, 400, 90, Color.White, Blue, btnStaff_Click);
            s.FlatAppearance.BorderSize = 2;
            s.FlatAppearance.BorderColor = Blue;
            s.Font = new Font("Segoe UI", 13, FontStyle.Bold);

            MakeButton(pnlLanding, "Exit", 602, 520, 100, 36, Color.White, Grey, btnExit_Click);
        }

        void btnManager_Click(object sender, EventArgs e) { StartSession(true); }
        void btnStaff_Click(object sender, EventArgs e) { StartSession(false); }
        void btnExit_Click(object sender, EventArgs e) { Close(); }

        void StartSession(bool manager)
        {
            lblRole.Text = manager ? "Manager" : "Staff";
            btnManage.Visible = manager;
            btnHistory.Left = manager ? 670 : 810;   // Staff has no Manage button, so Transactions takes its place
            lblRole.Left = manager ? 420 : 560;
            cart.Clear();
            category = "All";
            txtCash.Text = "";
            RefreshCats();
            RefreshTiles();
            RefreshCart();
            ShowScreen(pnlPos);
        }

        // ===== Screen 2: the register =====
        void BuildPos()
        {
            pnlPos = MakePanel(this, 0, 0, 1100, 680, Light);

            Panel bar = MakePanel(pnlPos, 0, 0, 1100, 64, Red);
            MakeLabel(bar, "SchoolPOS", 20, 12, 220, 40, 20, true, Color.White);
            lblRole = MakeLabel(bar, "", 560, 18, 240, 28, 12, true, Color.White);
            lblRole.TextAlign = ContentAlignment.MiddleRight;
            btnHistory = MakeButton(bar, "Transactions", 810, 14, 130, 36, Color.White, Red, btnHistory_Click);
            btnManage = MakeButton(bar, "Manage store", 810, 14, 130, 36, Color.White, Red, btnManage_Click);
            MakeButton(bar, "Logout", 950, 14, 130, 36, Color.FromArgb(153, 27, 27), Color.White, btnLogout_Click);

            pnlCats = MakePanel(pnlPos, 16, 76, 700, 44, Light);
            pnlTiles = new FlowLayoutPanel();
            pnlTiles.Location = new Point(16, 130);
            pnlTiles.Size = new Size(700, 480);
            pnlTiles.AutoScroll = true;
            pnlPos.Controls.Add(pnlTiles);

            MakeButton(pnlPos, "Void last sale", 16, 622, 200, 42, Dark, Color.White, btnVoidLast_Click);
            MakeButton(pnlPos, "Reprint receipt", 226, 622, 200, 42, Dark, Color.White, btnReprint_Click);

            Panel order = MakePanel(pnlPos, 730, 64, 370, 616, Color.White);
            MakeLabel(order, "Current order", 16, 10, 338, 28, 14, true, Dark);
            lstCart = new ListBox();
            lstCart.Location = new Point(16, 44); lstCart.Size = new Size(338, 240);
            lstCart.Font = new Font("Consolas", 11);
            lstCart.BorderStyle = BorderStyle.FixedSingle;
            order.Controls.Add(lstCart);

            MakeButton(order, "- 1", 16, 292, 106, 36, Light, Dark, btnMinus_Click);
            MakeButton(order, "Remove", 130, 292, 106, 36, Light, Dark, btnRemove_Click);
            MakeButton(order, "Clear", 244, 292, 106, 36, Light, Dark, btnClear_Click);

            MakeLabel(order, "TOTAL", 16, 346, 120, 36, 12, true, Grey);
            lblTotal = MakeLabel(order, "P0.00", 136, 336, 218, 48, 26, true, Dark);
            lblTotal.TextAlign = ContentAlignment.MiddleRight;

            MakeLabel(order, "Cash received", 16, 396, 150, 28, 11, false, Grey);
            txtCash = MakeBox(order, 176, 392, 178, 14);
            txtCash.TextAlign = HorizontalAlignment.Right;
            txtCash.TextChanged += txtCash_TextChanged;

            MakeButton(order, "Exact", 16, 432, 78, 34, Light, Dark, quickCash_Click);
            MakeButton(order, "100", 102, 432, 78, 34, Light, Dark, quickCash_Click);
            MakeButton(order, "500", 188, 432, 78, 34, Light, Dark, quickCash_Click);
            MakeButton(order, "1000", 274, 432, 78, 34, Light, Dark, quickCash_Click);

            MakeLabel(order, "Change", 16, 480, 120, 30, 11, false, Grey);
            lblChange = MakeLabel(order, "P0.00", 136, 474, 218, 38, 18, true, Green);
            lblChange.TextAlign = ContentAlignment.MiddleRight;

            MakeButton(order, "Complete sale", 16, 524, 338, 64, Green, Color.White, btnPay_Click).Font = new Font("Segoe UI", 16, FontStyle.Bold);
        }

        void RefreshCats()
        {
            pnlCats.Controls.Clear();
            List<string> cats = new List<string>();
            cats.Add("All");
            foreach (Product p in products)
                if (!cats.Contains(p.Category)) cats.Add(p.Category);

            int x = 0;
            foreach (string c in cats)
            {
                bool active = (c == category);
                MakeButton(pnlCats, c, x, 0, 104, 40, active ? Blue : Color.White, active ? Color.White : Dark, cat_Click);
                x += 112;
            }
        }

        void RefreshTiles()
        {
            pnlTiles.Controls.Clear();
            foreach (Product p in products)
            {
                if (category != "All" && p.Category != category) continue;
                string text = p.Name + "\nP" + p.Price.ToString("F2") + "\n" + p.Stock + " left";
                Button b = MakeButton(pnlTiles, text, 0, 0, 160, 96, Color.White, Dark, tile_Click);
                b.Margin = new Padding(0, 0, 10, 10);
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                b.Tag = p;
                if (p.Stock == 0) { b.Text = p.Name + "\nSold out"; b.Enabled = false; }
                else if (p.Stock <= 10) b.ForeColor = Red;
            }
        }

        void cat_Click(object sender, EventArgs e)
        {
            category = ((Button)sender).Text;
            RefreshCats();
            RefreshTiles();
        }

        void tile_Click(object sender, EventArgs e)
        {
            Product p = (Product)((Button)sender).Tag;
            OrderItem line = FindLine(p);
            int inCart = (line == null) ? 0 : line.Qty;
            if (inCart + 1 > p.Stock)
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
            line.Qty++;
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
            lstCart.Items.Clear();
            foreach (OrderItem line in cart) lstCart.Items.Add(line);
            lblTotal.Text = "P" + CartTotal().ToString("F2");
            UpdateChange();
        }

        void UpdateChange()
        {
            double cash;
            if (double.TryParse(txtCash.Text, out cash) && cash >= CartTotal() && cart.Count > 0)
            {
                lblChange.Text = "P" + (cash - CartTotal()).ToString("F2");
                lblChange.ForeColor = Green;
            }
            else
            {
                lblChange.Text = "P0.00";
                lblChange.ForeColor = Grey;
            }
        }

        void txtCash_TextChanged(object sender, EventArgs e) { UpdateChange(); }

        void quickCash_Click(object sender, EventArgs e)
        {
            string t = ((Button)sender).Text;
            txtCash.Text = (t == "Exact") ? CartTotal().ToString("F2") : t;
        }

        void btnMinus_Click(object sender, EventArgs e)
        {
            OrderItem line = lstCart.SelectedItem as OrderItem;
            if (line == null) return;
            line.Qty--;
            if (line.Qty == 0) cart.Remove(line);
            RefreshCart();
        }

        void btnRemove_Click(object sender, EventArgs e)
        {
            OrderItem line = lstCart.SelectedItem as OrderItem;
            if (line == null) return;
            cart.Remove(line);
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
            if (cart.Count == 0) { error = "The order is empty."; return null; }
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
            s.Total = CartTotal(); s.Cashier = lblRole.Text;
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
                MessageBox.Show("Enter the cash received.", "Cash needed");
                return;
            }
            string error;
            Sale s = CompleteSale(cash, out error);
            if (s == null)
            {
                MessageBox.Show(error, "Cannot complete sale");
                return;
            }
            txtCash.Text = "";
            RefreshCart();
            RefreshTiles();
            ShowReceipt(s.Receipt());
        }

        Sale LastValidSale()
        {
            for (int i = sales.Count - 1; i >= 0; i--)
                if (!sales[i].Voided) return sales[i];
            return null;
        }

        void VoidSale(Sale s)
        {
            foreach (OrderItem line in s.Lines) line.Item.Stock += line.Qty;
            s.Voided = true;
        }

        void btnVoidLast_Click(object sender, EventArgs e)
        {
            Sale s = LastValidSale();
            if (s == null) { MessageBox.Show("There is no sale to void."); return; }
            DialogResult r = MessageBox.Show("Void receipt #" + s.No + " (P" + s.Total.ToString("F2") + ")?", "Void last sale", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes) return;
            VoidSale(s);
            RefreshTiles();
        }

        void btnReprint_Click(object sender, EventArgs e)
        {
            Sale s = LastValidSale();
            if (s == null) { MessageBox.Show("There is no receipt to print."); return; }
            ShowReceipt(s.Receipt());
        }

        void ShowReceipt(string text)
        {
            Form f = new Form();
            f.Text = "Receipt";
            f.ClientSize = new Size(380, 480);
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false; f.MinimizeBox = false;

            TextBox t = new TextBox();
            t.Multiline = true; t.ReadOnly = true;
            t.Font = new Font("Consolas", 10);
            t.Text = text;
            t.TabStop = false;
            t.Location = new Point(10, 10); t.Size = new Size(360, 410);
            f.Controls.Add(t);

            Button ok = MakeButton(f, "Close", 120, 430, 140, 38, Blue, Color.White, null);
            ok.DialogResult = DialogResult.OK;
            f.AcceptButton = ok;
            f.ShowDialog();
        }

        void btnLogout_Click(object sender, EventArgs e) { ShowScreen(pnlLanding); }

        void btnManage_Click(object sender, EventArgs e)
        {
            RefreshManage();
            ShowScreen(pnlManage);
        }

        // ===== Screen 3: manager only =====
        void BuildManage()
        {
            pnlManage = MakePanel(this, 0, 0, 1100, 680, Light);

            Panel bar = MakePanel(pnlManage, 0, 0, 1100, 64, Blue);
            MakeLabel(bar, "Manage store", 20, 12, 300, 40, 20, true, Color.White);
            MakeButton(bar, "Back to register", 920, 14, 160, 36, Color.White, Blue, btnBack_Click);

            MakeLabel(pnlManage, "Products", 20, 76, 300, 28, 13, true, Dark);
            dgvProducts = new DataGridView();
            dgvProducts.Location = new Point(20, 108); dgvProducts.Size = new Size(1060, 320);
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.BackgroundColor = Color.White;
            dgvProducts.Columns.Add("colName", "Item");
            dgvProducts.Columns.Add("colCat", "Category");
            dgvProducts.Columns.Add("colPrice", "Price");
            dgvProducts.Columns.Add("colStock", "Stock");
            dgvProducts.CellClick += dgvProducts_CellClick;
            pnlManage.Controls.Add(dgvProducts);

            MakeLabel(pnlManage, "Name", 20, 446, 250, 22, 10, false, Grey);
            MakeLabel(pnlManage, "Category", 300, 446, 120, 22, 10, false, Grey);
            MakeLabel(pnlManage, "Price", 500, 446, 80, 22, 10, false, Grey);
            MakeLabel(pnlManage, "Stock", 620, 446, 80, 22, 10, false, Grey);
            txtName = MakeBox(pnlManage, 20, 470, 270, 11);
            cboCat = new ComboBox();
            cboCat.Location = new Point(300, 470); cboCat.Width = 190;
            cboCat.Font = new Font("Segoe UI", 11);
            pnlManage.Controls.Add(cboCat);
            txtPrice = MakeBox(pnlManage, 500, 470, 110, 11);
            txtStock = MakeBox(pnlManage, 620, 470, 110, 11);

            MakeButton(pnlManage, "Add item", 20, 520, 200, 42, Blue, Color.White, btnAddProduct_Click);
            MakeButton(pnlManage, "Update selected", 230, 520, 200, 42, Dark, Color.White, btnUpdateProduct_Click);
            MakeButton(pnlManage, "Remove selected", 440, 520, 200, 42, Red, Color.White, btnRemoveProduct_Click);

        }

        void btnBack_Click(object sender, EventArgs e)
        {
            category = "All";
            RefreshCats();
            RefreshTiles();
            ShowScreen(pnlPos);
        }

        void RefreshManage()
        {
            dgvProducts.Rows.Clear();
            foreach (Product p in products)
                dgvProducts.Rows.Add(p.Name, p.Category, "P" + p.Price.ToString("F2"), p.Stock);

            cboCat.Items.Clear();
            foreach (Product p in products)
                if (!cboCat.Items.Contains(p.Category)) cboCat.Items.Add(p.Category);

            ClearFields();
        }

        void ClearFields()
        {
            editRow = -1;
            txtName.Text = ""; cboCat.Text = ""; txtPrice.Text = ""; txtStock.Text = "";
        }

        void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            editRow = e.RowIndex;
            Product p = products[editRow];
            txtName.Text = p.Name; cboCat.Text = p.Category;
            txtPrice.Text = p.Price.ToString("F2"); txtStock.Text = p.Stock.ToString();
        }

        // reads the text boxes; returns false (and shows a message) if something is wrong
        bool ReadFields(out double price, out int stock)
        {
            price = 0; stock = 0;
            if (txtName.Text.Trim() == "" || cboCat.Text.Trim() == "")
            {
                MessageBox.Show("Enter a name and a category.", "Missing information");
                return false;
            }
            if (!double.TryParse(txtPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Price must be a number above 0.", "Invalid price");
                return false;
            }
            if (!int.TryParse(txtStock.Text, out stock) || stock < 0)
            {
                MessageBox.Show("Stock must be a whole number, 0 or more.", "Invalid stock");
                return false;
            }
            return true;
        }

        void btnAddProduct_Click(object sender, EventArgs e)
        {
            double price; int stock;
            if (!ReadFields(out price, out stock)) return;
            foreach (Product p in products)
            {
                if (p.Name.ToLower() == txtName.Text.Trim().ToLower())
                {
                    MessageBox.Show("That item already exists. Use Update instead.", "Duplicate item");
                    return;
                }
            }
            AddProduct(txtName.Text.Trim(), cboCat.Text.Trim(), price, stock);
            RefreshManage();
        }

        void btnUpdateProduct_Click(object sender, EventArgs e)
        {
            if (editRow < 0) { MessageBox.Show("Click an item in the table first."); return; }
            double price; int stock;
            if (!ReadFields(out price, out stock)) return;
            Product p = products[editRow];
            p.Name = txtName.Text.Trim(); p.Category = cboCat.Text.Trim();
            p.Price = price; p.Stock = stock;
            RefreshManage();
        }

        void btnRemoveProduct_Click(object sender, EventArgs e)
        {
            if (editRow < 0) { MessageBox.Show("Click an item in the table first."); return; }
            Product p = products[editRow];
            DialogResult r = MessageBox.Show("Remove " + p.Name + "?", "Remove item", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes) return;
            products.Remove(p);
            RefreshManage();
        }

        // ===== Screen 4: transactions (Manager and Staff) =====
        void BuildHistory()
        {
            pnlHistory = MakePanel(this, 0, 0, 1100, 680, Light);

            Panel bar = MakePanel(pnlHistory, 0, 0, 1100, 64, Red);
            MakeLabel(bar, "Transactions", 20, 12, 300, 40, 20, true, Color.White);
            MakeButton(bar, "Back to register", 920, 14, 160, 36, Color.White, Red, btnHistBack_Click);

            MakeLabel(pnlHistory, "Past sales (newest first)", 20, 76, 300, 28, 13, true, Dark);
            cboFilter = new ComboBox();
            cboFilter.Location = new Point(400, 76); cboFilter.Width = 180;
            cboFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFilter.Font = new Font("Segoe UI", 11);
            cboFilter.Items.Add("All sales");
            cboFilter.Items.Add("Completed only");
            cboFilter.Items.Add("Voided only");
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += cboFilter_SelectedIndexChanged;
            pnlHistory.Controls.Add(cboFilter);

            lstSales = new ListBox();
            lstSales.Location = new Point(20, 112); lstSales.Size = new Size(560, 548);
            lstSales.Font = new Font("Consolas", 12);
            lstSales.ItemHeight = 24;
            lstSales.SelectedIndexChanged += lstSales_SelectedIndexChanged;
            pnlHistory.Controls.Add(lstSales);

            MakeLabel(pnlHistory, "Receipt", 610, 76, 200, 28, 13, true, Dark);
            lblStatus = MakeLabel(pnlHistory, "", 840, 76, 240, 28, 12, true, Grey);
            lblStatus.TextAlign = ContentAlignment.MiddleRight;

            txtReceipt = new TextBox();
            txtReceipt.Multiline = true; txtReceipt.ReadOnly = true; txtReceipt.TabStop = false;
            txtReceipt.Font = new Font("Consolas", 10);
            txtReceipt.Location = new Point(610, 112); txtReceipt.Size = new Size(470, 420);
            pnlHistory.Controls.Add(txtReceipt);

            MakeButton(pnlHistory, "Void this sale", 610, 548, 230, 52, Red, Color.White, btnVoidSel_Click);
            MakeButton(pnlHistory, "Reprint", 850, 548, 230, 52, Dark, Color.White, btnHistReprint_Click);
            MakeLabel(pnlHistory, "Voiding puts the items back in stock.", 610, 612, 470, 26, 10, false, Grey);
        }

        void btnHistory_Click(object sender, EventArgs e)
        {
            RefreshHistory(null);
            ShowScreen(pnlHistory);
        }

        void btnHistBack_Click(object sender, EventArgs e)
        {
            RefreshTiles();
            ShowScreen(pnlPos);
        }

        void cboFilter_SelectedIndexChanged(object sender, EventArgs e) { RefreshHistory(null); }

        // fills the list using the filter; keeps the given sale selected if it is still shown
        void RefreshHistory(Sale keep)
        {
            lstSales.Items.Clear();
            for (int i = sales.Count - 1; i >= 0; i--)
            {
                Sale s = sales[i];
                if (cboFilter.SelectedIndex == 1 && s.Voided) continue;
                if (cboFilter.SelectedIndex == 2 && !s.Voided) continue;
                lstSales.Items.Add(s);
            }
            if (keep != null && lstSales.Items.Contains(keep)) lstSales.SelectedItem = keep;
            else ShowSelectedSale();
        }

        void lstSales_SelectedIndexChanged(object sender, EventArgs e) { ShowSelectedSale(); }

        void ShowSelectedSale()
        {
            Sale s = lstSales.SelectedItem as Sale;
            if (s == null)
            {
                txtReceipt.Text = "";
                lblStatus.Text = lstSales.Items.Count == 0 ? "No sales to show" : "Select a sale";
                lblStatus.ForeColor = Grey;
                return;
            }
            txtReceipt.Text = s.Receipt();
            lblStatus.Text = s.Voided ? "VOIDED" : "Completed";
            lblStatus.ForeColor = s.Voided ? Red : Green;
        }

        void btnVoidSel_Click(object sender, EventArgs e)
        {
            Sale s = lstSales.SelectedItem as Sale;
            if (s == null) { MessageBox.Show("Select a sale first."); return; }
            if (s.Voided) { MessageBox.Show("That sale is already voided."); return; }
            DialogResult r = MessageBox.Show("Void receipt #" + s.No + " (P" + s.Total.ToString("F2") + ")?\nThe items will go back in stock.", "Void sale", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes) return;
            VoidSale(s);
            RefreshHistory(s);
        }

        void btnHistReprint_Click(object sender, EventArgs e)
        {
            Sale s = lstSales.SelectedItem as Sale;
            if (s == null) { MessageBox.Show("Select a sale first."); return; }
            ShowReceipt(s.Receipt());
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
