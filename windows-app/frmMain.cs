using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    public partial class frmMain : Form
    {
        public bool LoggedOut = false;

        private bool isManager;
        private List<SaleLine> cart = new List<SaleLine>();

        // the 7 bars of the "last 7 days" chart (built from Panels and Labels)
        private Panel[] bars = new Panel[7];
        private Label[] barDays = new Label[7];
        private Label[] barValues = new Label[7];

        private Color blue = Color.FromArgb(34, 52, 216);
        private Color lightBlue = Color.FromArgb(170, 182, 245);
        private Color green = Color.FromArgb(22, 163, 74);
        private Color red = Color.FromArgb(220, 38, 38);
        private Color gray = Color.FromArgb(107, 114, 128);

        // =====================================================
        //  START UP
        // =====================================================
        public frmMain(bool manager)
        {
            InitializeComponent();
            isManager = manager;

            SetupGrids();
            CreateBars();

            // Properties decide what each role can see
            if (isManager)
            {
                lblRole.Text = "Logged in as: Manager";
            }
            else
            {
                lblRole.Text = "Logged in as: Staff";
            }
            btnReports.Visible = isManager;
            pnlManage.Visible = isManager;
            btnVoidSel.Visible = isManager;

            lblDate.Text = DateTime.Now.ToString("dddd, MMMM d");
            cboFilter.SelectedIndex = 0;

            ShowPage(pnlDashboard, btnDashboard, "Dashboard");
            LoadDashboard();
        }

        private void AddColumns(DataGridView grid, string[] headers)
        {
            foreach (string h in headers)
            {
                grid.Columns.Add(h, h);
            }
        }

        private void SetupGrids()
        {
            AddColumns(dgvTop, new string[] { "Product", "Category", "Units sold", "Stock", "Unit price", "Revenue" });
            AddColumns(dgvProducts, new string[] { "Item", "Category", "Price", "Stock" });
            AddColumns(dgvCart, new string[] { "Item", "Qty", "Price", "Total" });
            AddColumns(dgvInv, new string[] { "Item", "Category", "Price", "Stock", "Status" });
            AddColumns(dgvRec, new string[] { "Receipt", "Date / Time", "Items", "Total", "Cashier", "Status" });
            // make the Date / Time column wider and the Items column narrower
            dgvRec.Columns[1].FillWeight = 170;
            dgvRec.Columns[2].FillWeight = 55;
            AddColumns(dgvHour, new string[] { "Hour", "Revenue" });
            AddColumns(dgvCat, new string[] { "Category", "Revenue", "Share" });
        }

        // =====================================================
        //  NAVIGATION (sidebar buttons show one panel at a time)
        // =====================================================
        private void ShowPage(Panel page, Button active, string title)
        {
            pnlDashboard.Visible = false;
            pnlSales.Visible = false;
            pnlInventory.Visible = false;
            pnlRecords.Visible = false;
            pnlReports.Visible = false;
            page.Visible = true;

            Button[] all = { btnDashboard, btnSales, btnInventory, btnRecords, btnReports };
            foreach (Button b in all)
            {
                b.BackColor = Color.White;
                b.ForeColor = gray;
            }
            active.BackColor = Color.FromArgb(238, 241, 255);
            active.ForeColor = blue;

            lblTitle.Text = title;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ShowPage(pnlDashboard, btnDashboard, "Dashboard");
            LoadDashboard();
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            ShowPage(pnlSales, btnSales, "Sales");
            LoadSales();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            ShowPage(pnlInventory, btnInventory, "Inventory");
            LoadInventory();
        }

        private void btnRecords_Click(object sender, EventArgs e)
        {
            ShowPage(pnlRecords, btnRecords, "Purchase Records");
            LoadRecords();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            ShowPage(pnlReports, btnReports, "Reports");
            LoadReports();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            LoggedOut = true;
            this.Close();
        }

        // =====================================================
        //  DASHBOARD
        // =====================================================
        private void CreateBars()
        {
            for (int i = 0; i < 7; i++)
            {
                bars[i] = new Panel();
                bars[i].BackColor = blue;
                bars[i].Width = 40;
                bars[i].Left = 30 + i * 70;
                pnlChart.Controls.Add(bars[i]);

                barDays[i] = new Label();
                barDays[i].AutoSize = false;
                barDays[i].Width = 60;
                barDays[i].Height = 18;
                barDays[i].Left = 20 + i * 70;
                barDays[i].Top = 196;
                barDays[i].TextAlign = ContentAlignment.MiddleCenter;
                barDays[i].ForeColor = gray;
                pnlChart.Controls.Add(barDays[i]);

                barValues[i] = new Label();
                barValues[i].AutoSize = false;
                barValues[i].Width = 60;
                barValues[i].Height = 18;
                barValues[i].Left = 20 + i * 70;
                barValues[i].TextAlign = ContentAlignment.MiddleCenter;
                barValues[i].Font = new Font("Segoe UI", 8F);
                pnlChart.Controls.Add(barValues[i]);
            }
        }

        private void UpdateBars(double[] values, string[] labels)
        {
            double max = 0;
            foreach (double v in values)
            {
                if (v > max) max = v;
            }
            if (max <= 0) max = 1;

            for (int i = 0; i < 7; i++)
            {
                int height = (int)(values[i] / max * 130);   // tallest bar = 130 pixels
                bars[i].Height = height;
                bars[i].Top = 190 - height;
                if (values[i] == max) bars[i].BackColor = blue;
                else bars[i].BackColor = lightBlue;

                barValues[i].Top = bars[i].Top - 19;
                barValues[i].Text = "P" + values[i].ToString("N0");
                barDays[i].Text = labels[i];
            }
        }

        private void ShowComparison(Label lbl, double today, double yesterday)
        {
            if (yesterday <= 0)
            {
                lbl.Text = "No data for yesterday";
                lbl.ForeColor = gray;
                return;
            }

            double percent = (today - yesterday) / yesterday * 100;
            if (percent >= 0)
            {
                lbl.Text = "Up " + percent.ToString("0.#") + "% vs yesterday";
                lbl.ForeColor = green;
            }
            else
            {
                lbl.Text = "Down " + (-percent).ToString("0.#") + "% vs yesterday";
                lbl.ForeColor = red;
            }
        }

        private void LoadDashboard()
        {
            DateTime today = DateTime.Today;
            double salesToday = Store.RevenueOn(today);
            double salesYesterday = Store.RevenueOn(today.AddDays(-1));
            int ordersToday = Store.OrdersOn(today);
            int ordersYesterday = Store.OrdersOn(today.AddDays(-1));

            double avgToday = 0;
            if (ordersToday > 0) avgToday = salesToday / ordersToday;
            double avgYesterday = 0;
            if (ordersYesterday > 0) avgYesterday = salesYesterday / ordersYesterday;

            lblK1V.Text = "P" + salesToday.ToString("N2");
            ShowComparison(lblK1S, salesToday, salesYesterday);

            lblK2V.Text = ordersToday.ToString();
            ShowComparison(lblK2S, ordersToday, ordersYesterday);

            lblK3V.Text = "P" + avgToday.ToString("N2");
            ShowComparison(lblK3S, avgToday, avgYesterday);

            int low = Store.CountLowStock();
            int outOfStock = Store.CountOutOfStock();
            lblK4V.Text = (low + outOfStock).ToString();
            lblK4S.Text = outOfStock + " out of stock";
            if (low + outOfStock > 0) lblK4S.ForeColor = red;
            else lblK4S.ForeColor = green;

            // bar chart
            string[] labels;
            double[] values = Store.RevenueLastDays(7, out labels);
            UpdateBars(values, labels);

            // recent sales list
            lstRecent.Items.Clear();
            int shown = 0;
            for (int i = Store.Sales.Count - 1; i >= 0 && shown < 8; i--)
            {
                Sale s = Store.Sales[i];
                if (s.Voided) continue;
                lstRecent.Items.Add("#" + s.No.ToString("D4") + "   " + s.Time.ToString("MMM d  h:mm tt") + "   P" + s.Total.ToString("N2"));
                shown++;
            }

            // top products table
            dgvTop.Rows.Clear();
            List<ProductStat> top = Store.TopProducts(6);
            foreach (ProductStat st in top)
            {
                dgvTop.Rows.Add(st.Item.Name, st.Item.Category, st.Units, st.Item.Stock,
                    "P" + st.Item.Price.ToString("N2"), "P" + st.Revenue.ToString("N2"));
            }
        }

        // =====================================================
        //  SALES (point of sale)
        // =====================================================
        private void LoadSales()
        {
            FillProducts();
            RefreshCart();
        }

        private void FillProducts()
        {
            string key = txtSearch.Text.Trim().ToLower();
            dgvProducts.Rows.Clear();

            foreach (Product p in Store.Products)
            {
                if (key == "" || p.Name.ToLower().Contains(key) || p.Category.ToLower().Contains(key))
                {
                    int row = dgvProducts.Rows.Add(p.Name, p.Category, "P" + p.Price.ToString("N2"), p.Stock);
                    dgvProducts.Rows[row].Tag = p;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FillProducts();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Select an item first.");
                return;
            }

            Product p = dgvProducts.CurrentRow.Tag as Product;
            if (p == null) return;

            int qty;
            if (!int.TryParse(txtQty.Text, out qty) || qty < 1)
            {
                MessageBox.Show("Enter a quantity of at least 1.");
                return;
            }

            // how many of this item are already in the order?
            int already = 0;
            SaleLine existing = null;
            foreach (SaleLine line in cart)
            {
                if (line.Item == p)
                {
                    already += line.Qty;
                    existing = line;
                }
            }

            if (qty + already > p.Stock)
            {
                MessageBox.Show("Not enough stock. Only " + (p.Stock - already) + " left for " + p.Name + ".",
                    "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existing != null)
            {
                existing.Qty += qty;
            }
            else
            {
                SaleLine line = new SaleLine();
                line.Item = p;
                line.Name = p.Name;
                line.Qty = qty;
                line.Price = p.Price;
                cart.Add(line);
            }

            txtQty.Text = "1";
            RefreshCart();
        }

        private double CartTotal()
        {
            double total = 0;
            foreach (SaleLine line in cart)
            {
                total += line.GetTotal();
            }
            return total;
        }

        private void RefreshCart()
        {
            dgvCart.Rows.Clear();
            foreach (SaleLine line in cart)
            {
                dgvCart.Rows.Add(line.Name, line.Qty, "P" + line.Price.ToString("N2"), "P" + line.GetTotal().ToString("N2"));
            }
            lblTotal.Text = "P" + CartTotal().ToString("N2");
            UpdateChange();
        }

        private void UpdateChange()
        {
            double cash;
            double total = CartTotal();

            if (double.TryParse(txtCash.Text, out cash) && total > 0 && cash >= total)
            {
                lblChange.Text = "P" + (cash - total).ToString("N2");
                lblChange.ForeColor = green;
            }
            else
            {
                lblChange.Text = "P0.00";
                lblChange.ForeColor = gray;
            }
        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            UpdateChange();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null) return;

            int index = dgvCart.CurrentRow.Index;
            if (index >= 0 && index < cart.Count)
            {
                cart.RemoveAt(index);
                RefreshCart();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cart.Clear();
            RefreshCart();
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Add at least one item first.");
                return;
            }

            double cash;
            if (!double.TryParse(txtCash.Text, out cash))
            {
                MessageBox.Show("Enter the cash received.");
                return;
            }

            string cashier = isManager ? "Manager" : "Staff";
            string error;
            Sale sale = Store.CompleteSale(cart, cash, cashier, out error);

            if (sale == null)
            {
                MessageBox.Show(error, "Cannot complete sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            frmReceipt receiptForm = new frmReceipt(sale.Receipt());
            receiptForm.ShowDialog();

            cart.Clear();
            txtCash.Text = "";
            LoadSales();
        }

        private void btnVoid_Click(object sender, EventArgs e)
        {
            Sale s = Store.LastValidSale();
            if (s == null)
            {
                MessageBox.Show("There is no sale to void.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Void receipt #" + s.No.ToString("D4") + " (P" + s.Total.ToString("N2") + ")? The stock will be restored.",
                "Void sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                Store.VoidSale(s);
                LoadSales();
                MessageBox.Show("Sale voided. Stock restored.");
            }
        }

        private void btnReprint_Click(object sender, EventArgs e)
        {
            Sale s = Store.LastValidSale();
            if (s == null)
            {
                MessageBox.Show("There is no receipt to reprint.");
                return;
            }

            frmReceipt receiptForm = new frmReceipt("*** DUPLICATE COPY ***" + Environment.NewLine + s.Receipt());
            receiptForm.ShowDialog();
        }

        // =====================================================
        //  INVENTORY
        // =====================================================
        private void LoadInventory()
        {
            // fill the category drop-down
            cboCat.Items.Clear();
            foreach (string c in Store.GetCategories())
            {
                cboCat.Items.Add(c);
            }
            FillInventory();
        }

        private void FillInventory()
        {
            string key = txtInvSearch.Text.Trim().ToLower();
            dgvInv.Rows.Clear();

            foreach (Product p in Store.Products)
            {
                bool matches = (key == "" || p.Name.ToLower().Contains(key) || p.Category.ToLower().Contains(key));
                if (chkLow.Checked && p.Stock > Store.LowStockLevel) matches = false;
                if (!matches) continue;

                string status = "In stock";
                if (p.Stock == 0) status = "Out of stock";
                else if (p.Stock <= Store.LowStockLevel) status = "Low";

                int row = dgvInv.Rows.Add(p.Name, p.Category, "P" + p.Price.ToString("N2"), p.Stock, status);
                dgvInv.Rows[row].Tag = p;

                if (p.Stock <= Store.LowStockLevel) dgvInv.Rows[row].Cells[4].Style.ForeColor = red;
                else dgvInv.Rows[row].Cells[4].Style.ForeColor = green;
            }
        }

        private void txtInvSearch_TextChanged(object sender, EventArgs e)
        {
            FillInventory();
        }

        private void chkLow_CheckedChanged(object sender, EventArgs e)
        {
            FillInventory();
        }

        // clicking a row copies it into the text boxes so it can be edited
        private void dgvInv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            Product p = dgvInv.Rows[e.RowIndex].Tag as Product;
            if (p == null) return;

            txtName.Text = p.Name;
            cboCat.Text = p.Category;
            txtPrice.Text = p.Price.ToString("0.##");
            txtStock.Text = p.Stock.ToString();
        }

        // Reads and checks the four boxes. Returns false (after showing a message) if something is wrong.
        private bool ReadProductBoxes(out string name, out string category, out double price, out int stock)
        {
            name = txtName.Text.Trim();
            category = cboCat.Text.Trim();
            price = 0;
            stock = 0;

            if (name == "") { MessageBox.Show("Enter an item name."); return false; }
            if (category == "") { MessageBox.Show("Enter a category."); return false; }
            if (!double.TryParse(txtPrice.Text, out price) || price <= 0) { MessageBox.Show("Enter a valid price."); return false; }
            if (!int.TryParse(txtStock.Text, out stock) || stock < 0) { MessageBox.Show("Enter a valid stock count."); return false; }
            return true;
        }

        private void ClearProductBoxes()
        {
            txtName.Text = "";
            cboCat.Text = "";
            txtPrice.Text = "";
            txtStock.Text = "";
        }

        private void btnInvAdd_Click(object sender, EventArgs e)
        {
            string name, category;
            double price;
            int stock;
            if (!ReadProductBoxes(out name, out category, out price, out stock)) return;

            foreach (Product p in Store.Products)
            {
                if (p.Name.ToLower() == name.ToLower())
                {
                    MessageBox.Show("That item already exists. Select it and press Update instead.");
                    return;
                }
            }

            Store.Products.Add(new Product(name, category, price, stock));
            ClearProductBoxes();
            LoadInventory();
            MessageBox.Show(name + " was added.");
        }

        private void btnInvUpdate_Click(object sender, EventArgs e)
        {
            if (dgvInv.CurrentRow == null)
            {
                MessageBox.Show("Click an item in the table first.");
                return;
            }
            Product selected = dgvInv.CurrentRow.Tag as Product;
            if (selected == null) return;

            string name, category;
            double price;
            int stock;
            if (!ReadProductBoxes(out name, out category, out price, out stock)) return;

            foreach (Product p in Store.Products)
            {
                if (p != selected && p.Name.ToLower() == name.ToLower())
                {
                    MessageBox.Show("Another item already has that name.");
                    return;
                }
            }

            selected.Name = name;
            selected.Category = category;
            selected.Price = price;
            selected.Stock = stock;
            LoadInventory();
            MessageBox.Show("Item updated.");
        }

        private void btnInvRemove_Click(object sender, EventArgs e)
        {
            if (dgvInv.CurrentRow == null)
            {
                MessageBox.Show("Click an item in the table first.");
                return;
            }
            Product selected = dgvInv.CurrentRow.Tag as Product;
            if (selected == null) return;

            DialogResult answer = MessageBox.Show("Remove " + selected.Name + " from the inventory?",
                "Remove item", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            Store.Products.Remove(selected);

            // also take it out of the current order if it is there
            for (int i = cart.Count - 1; i >= 0; i--)
            {
                if (cart[i].Item == selected) cart.RemoveAt(i);
            }

            ClearProductBoxes();
            LoadInventory();
        }

        // =====================================================
        //  PURCHASE RECORDS
        // =====================================================
        private void LoadRecords()
        {
            dgvRec.Rows.Clear();

            for (int i = Store.Sales.Count - 1; i >= 0; i--)      // newest first
            {
                Sale s = Store.Sales[i];

                if (cboFilter.SelectedIndex == 1 && s.Time.Date != DateTime.Today) continue;
                if (cboFilter.SelectedIndex == 2 && !s.Voided) continue;

                string status = "Completed";
                if (s.Voided) status = "Voided";

                int row = dgvRec.Rows.Add("#" + s.No.ToString("D4"), s.Time.ToString("MMM d, h:mm tt"),
                    s.ItemCount(), "P" + s.Total.ToString("N2"), s.Cashier, status);
                dgvRec.Rows[row].Tag = s;

                if (s.Voided) dgvRec.Rows[row].Cells[5].Style.ForeColor = red;
                else dgvRec.Rows[row].Cells[5].Style.ForeColor = green;
            }

            if (dgvRec.Rows.Count == 0) txtRecReceipt.Text = "No records.";
        }

        private void cboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void dgvRec_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRec.CurrentRow == null) return;

            Sale s = dgvRec.CurrentRow.Tag as Sale;
            if (s == null) return;

            txtRecReceipt.Text = s.Receipt();
            txtRecReceipt.SelectionStart = 0;
            txtRecReceipt.SelectionLength = 0;
        }

        private void btnVoidSel_Click(object sender, EventArgs e)
        {
            if (dgvRec.CurrentRow == null)
            {
                MessageBox.Show("Click a record first.");
                return;
            }

            Sale s = dgvRec.CurrentRow.Tag as Sale;
            if (s == null) return;

            if (s.Voided)
            {
                MessageBox.Show("That sale is already voided.");
                return;
            }

            DialogResult answer = MessageBox.Show("Void receipt #" + s.No.ToString("D4") + "? The stock will be restored.",
                "Void sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                Store.VoidSale(s);
                LoadRecords();
            }
        }

        // =====================================================
        //  REPORTS (Manager only)
        // =====================================================
        private void LoadReports()
        {
            double revenue = 0;
            int completed = 0;
            int units = 0;
            int voided = 0;

            foreach (Sale s in Store.Sales)
            {
                if (s.Voided)
                {
                    voided++;
                }
                else
                {
                    completed++;
                    revenue += s.Total;
                    units += s.ItemCount();
                }
            }

            double average = 0;
            if (completed > 0) average = revenue / completed;

            lblRep1.Text = "Total revenue:  P" + revenue.ToString("N2");
            lblRep2.Text = "Completed sales:  " + completed + "   (average P" + average.ToString("N2") + ")";
            lblRep3.Text = "Units sold:  " + units;
            lblRep4.Text = "Voided sales:  " + voided;

            // sales by hour
            dgvHour.Rows.Clear();
            string[] hourLabels;
            double[] hourValues = Store.RevenueByHour(out hourLabels);
            for (int i = 0; i < hourValues.Length; i++)
            {
                dgvHour.Rows.Add(hourLabels[i], "P" + hourValues[i].ToString("N2"));
            }

            // revenue by category
            dgvCat.Rows.Clear();
            Dictionary<string, double> categories = Store.RevenueByCategory();
            foreach (KeyValuePair<string, double> pair in categories)
            {
                double share = 0;
                if (revenue > 0) share = pair.Value / revenue * 100;
                dgvCat.Rows.Add(pair.Key, "P" + pair.Value.ToString("N2"), share.ToString("0") + "%");
            }
        }

        private string Quote(string text)
        {
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Receipt,DateTime,Cashier,Item,Qty,UnitPrice,LineTotal,Status");

            foreach (Sale s in Store.Sales)
            {
                foreach (SaleLine line in s.Lines)
                {
                    string status = "Completed";
                    if (s.Voided) status = "Voided";

                    sb.AppendLine(s.No.ToString("D4") + "," + s.Time.ToString("yyyy-MM-dd HH:mm") + "," + Quote(s.Cashier) + "," +
                        Quote(line.Name) + "," + line.Qty + "," + line.Price.ToString("F2") + "," + line.GetTotal().ToString("F2") + "," + status);
                }
            }

            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sales-report.csv");
                File.WriteAllText(path, sb.ToString());
                MessageBox.Show("Saved to:" + Environment.NewLine + path, "Export CSV");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save the file: " + ex.Message, "Export CSV");
            }
        }
    }
}
