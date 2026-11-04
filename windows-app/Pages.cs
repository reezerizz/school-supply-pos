using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    // =====================================================================
    //  DASHBOARD  (KPI cards, revenue chart, live sales, top products)
    // =====================================================================
    class DashboardPage : Panel, IPage
    {
        readonly KpiCard kSales, kOrders, kAvg, kLow;
        readonly LineChart chart;
        readonly Label chartTitle;
        readonly Panel live;
        readonly DataGridView top;
        readonly Button bWeek, bToday;
        bool hourly;

        public DashboardPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Bg;
            Padding = new Padding(16);

            var root = UI.Grid(2, 3);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 48));

            // ----- KPI row -----
            var kp = UI.Grid(4, 1);
            for (int i = 0; i < 4; i++) kp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kSales = new KpiCard("Today's Sales", Theme.Blue);
            kOrders = new KpiCard("Orders Today", Theme.Amber);
            kAvg = new KpiCard("Avg Order Value", Theme.Green);
            kLow = new KpiCard("Low Stock Items", Theme.Red);
            kp.Controls.Add(kSales, 0, 0);
            kp.Controls.Add(kOrders, 1, 0);
            kp.Controls.Add(kAvg, 2, 0);
            kp.Controls.Add(kLow, 3, 0);
            root.Controls.Add(kp, 0, 0);
            root.SetColumnSpan(kp, 2);

            // ----- revenue chart -----
            chart = new LineChart();
            var toggle = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Color.Transparent, WrapContents = false };
            bWeek = Theme.Btn("7 Days"); bWeek.Width = 78; bWeek.Height = 28;
            bToday = Theme.Btn("Today"); bToday.Width = 78; bToday.Height = 28;
            bWeek.Click += (s, e) => { hourly = false; ReloadChart(); };
            bToday.Click += (s, e) => { hourly = true; ReloadChart(); };
            toggle.Controls.Add(bWeek);
            toggle.Controls.Add(bToday);
            root.Controls.Add(UI.Card("Revenue", chart, toggle, out chartTitle), 0, 1);

            // ----- live sales -----
            live = new Panel { AutoScroll = true, BackColor = Color.White };
            root.Controls.Add(UI.Card("Live Sales", live), 1, 1);

            // ----- top products -----
            top = new DataGridView();
            Theme.StyleGrid(top);
            Theme.AddCol(top, "PRODUCT", 28);
            Theme.AddCol(top, "CATEGORY", 16);
            Theme.AddCol(top, "UNITS SOLD", 14, true);
            Theme.AddCol(top, "STOCK", 22);
            Theme.AddCol(top, "UNIT PRICE", 14, true);
            Theme.AddCol(top, "REVENUE", 16, true);
            top.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 3) return;
                var p = top.Rows[e.RowIndex].Tag as Product;
                if (p != null) Theme.PaintStock(e, p.Stock);
            };
            var topCard = UI.Card("Top Selling Products", top);
            root.Controls.Add(topCard, 0, 2);
            root.SetColumnSpan(topCard, 2);

            Controls.Add(root);
        }

        public void Reload()
        {
            DateTime today = DateTime.Today;
            double rt = Store.RevenueOn(today), ry = Store.RevenueOn(today.AddDays(-1));
            int ot = Store.OrdersOn(today), oy = Store.OrdersOn(today.AddDays(-1));
            double at = ot > 0 ? rt / ot : 0, ay = oy > 0 ? ry / oy : 0;
            Color c;

            string d = UI.Delta(rt, ry, "vs yesterday", out c);
            kSales.Set("P" + rt.ToString("N2"), d, c);
            d = UI.Delta(ot, oy, "vs yesterday", out c);
            kOrders.Set(ot.ToString(), d, c);
            d = UI.Delta(at, ay, "vs yesterday", out c);
            kAvg.Set("P" + at.ToString("N2"), d, c);

            int outCount = Store.Products.Count(p => p.Stock == 0);
            int lowCount = Store.Products.Count(p => p.Stock > 0 && p.Stock <= Store.LowStockLevel);
            kLow.Set((lowCount + outCount).ToString(), outCount + " out of stock",
                (lowCount + outCount) > 0 ? Theme.Red : Theme.Green);

            ReloadChart();
            ReloadLive();
            ReloadTop();
        }

        void ReloadChart()
        {
            string[] labels;
            double[] values;
            if (hourly)
            {
                values = Store.RevenueByHour(DateTime.Today, out labels);
                chartTitle.Text = "Revenue - Today by hour";
            }
            else
            {
                values = Store.RevenueByDay(7, out labels);
                chartTitle.Text = "Revenue - Last 7 days";
            }
            chart.SetData(values, labels);

            bWeek.BackColor = hourly ? Color.White : Theme.Blue; bWeek.ForeColor = hourly ? Theme.Ink : Color.White;
            bToday.BackColor = hourly ? Theme.Blue : Color.White; bToday.ForeColor = hourly ? Color.White : Theme.Ink;
        }

        void ReloadLive()
        {
            live.SuspendLayout();
            foreach (Control c in live.Controls.Cast<Control>().ToList()) { live.Controls.Remove(c); c.Dispose(); }

            var recent = Store.Valid.Reverse().Take(8).ToList();
            if (recent.Count == 0)
            {
                live.Controls.Add(new Label { Text = "No sales yet.", ForeColor = Theme.Muted, AutoSize = true, Location = new Point(4, 8) });
            }

            // Dock=Top rows: the last one added appears on top, so add oldest first
            for (int i = recent.Count - 1; i >= 0; i--)
                live.Controls.Add(MakeLiveRow(recent[i]));

            live.ResumeLayout();
        }

        static Panel MakeLiveRow(Sale s)
        {
            var row = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            var amount = new Label
            {
                Text = "P" + s.Total.ToString("N2"),
                Dock = DockStyle.Right,
                Width = 96,
                TextAlign = ContentAlignment.MiddleRight,
                Font = Theme.F(10, FontStyle.Bold),
                ForeColor = Theme.Ink
            };
            var t = new Label
            {
                Text = "Order #" + s.No.ToString("D4"),
                Location = new Point(0, 8),
                AutoSize = true,
                Font = Theme.F(10, FontStyle.Bold),
                ForeColor = Theme.Ink
            };
            var sub = new Label
            {
                Text = s.Items + " items  ·  " + s.Time.ToString("MMM d, h:mm tt"),
                Location = new Point(0, 30),
                AutoSize = true,
                Font = Theme.F(8.5f),
                ForeColor = Theme.Muted
            };
            row.Controls.Add(amount);
            row.Controls.Add(t);
            row.Controls.Add(sub);
            row.Paint += (o, e) =>
            {
                using (var p = new Pen(Theme.GridLine))
                    e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1);
            };
            return row;
        }

        void ReloadTop()
        {
            top.Rows.Clear();
            foreach (var st in Store.TopProducts(6))
            {
                int i = top.Rows.Add(st.Name, st.P.Category, st.Units, st.P.Stock, "P" + st.P.Price.ToString("N2"), "P" + st.Revenue.ToString("N2"));
                top.Rows[i].Tag = st.P;
            }
            top.ClearSelection();
        }
    }

    // =====================================================================
    //  SALES  (point of sale)
    // =====================================================================
    class SalesPage : Panel, IPage
    {
        readonly string cashier;
        readonly List<SaleLine> lines = new List<SaleLine>();

        readonly DataGridView products, cart;
        readonly TextBox search, cash;
        readonly NumericUpDown qty;
        readonly Label total, change;

        public SalesPage(string cashierName)
        {
            cashier = cashierName;
            Dock = DockStyle.Fill;
            BackColor = Theme.Bg;
            Padding = new Padding(16);

            var root = UI.Grid(2, 1);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ----- left: product list -----
            products = new DataGridView();
            Theme.StyleGrid(products);
            Theme.AddCol(products, "ITEM", 34);
            Theme.AddCol(products, "CATEGORY", 20);
            Theme.AddCol(products, "PRICE", 16, true);
            Theme.AddCol(products, "STOCK", 30);
            products.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 3) return;
                var p = products.Rows[e.RowIndex].Tag as Product;
                if (p != null) Theme.PaintStock(e, p.Stock);
            };
            products.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) AddToOrder(); };

            search = new TextBox { Dock = DockStyle.Top, Font = Theme.F(10.5f), PlaceholderText = "Search supplies..." };
            search.TextChanged += (s, e) => FillProducts();
            var searchWrap = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.Transparent };
            searchWrap.Controls.Add(search);

            qty = new NumericUpDown { Minimum = 1, Maximum = 999, Value = 1, Width = 70, Location = new Point(40, 10), Font = Theme.F(10.5f) };
            var qtyLabel = new Label { Text = "Qty", Location = new Point(0, 13), AutoSize = true, ForeColor = Theme.Muted };
            var addBtn = Theme.Btn("Add to order", true);
            addBtn.Dock = DockStyle.Right; addBtn.Width = 140;
            addBtn.Click += (s, e) => AddToOrder();
            var addBar = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.Transparent };
            addBar.Controls.Add(qtyLabel);
            addBar.Controls.Add(qty);
            addBar.Controls.Add(addBtn);

            var leftIn = new Panel { BackColor = Color.Transparent };
            leftIn.Controls.Add(products);
            leftIn.Controls.Add(addBar);
            leftIn.Controls.Add(searchWrap);
            root.Controls.Add(UI.Card("Supplies", leftIn), 0, 0);

            // ----- right: current order -----
            cart = new DataGridView();
            Theme.StyleGrid(cart);
            Theme.AddCol(cart, "ITEM", 38);
            Theme.AddCol(cart, "QTY", 14, true);
            Theme.AddCol(cart, "PRICE", 22, true);
            Theme.AddCol(cart, "TOTAL", 26, true);

            total = new Label { Text = "P0.00", Font = Theme.F(18, FontStyle.Bold), ForeColor = Theme.Ink, TextAlign = ContentAlignment.MiddleRight };
            cash = new TextBox { Font = Theme.F(12), TextAlign = HorizontalAlignment.Right };
            cash.TextChanged += (s, e) => UpdateChange();
            change = new Label { Text = "P0.00", Font = Theme.F(12, FontStyle.Bold), ForeColor = Theme.Green, TextAlign = ContentAlignment.MiddleRight };

            var remove = Theme.Btn("Remove item"); remove.Click += (s, e) => RemoveSelected();
            var clear = Theme.Btn("Clear"); clear.Click += (s, e) => { lines.Clear(); RefreshCart(); };
            var rowBtns = Pair(remove, clear);

            var pay = Theme.Btn("Complete Sale", true);
            pay.Dock = DockStyle.Top; pay.Height = 46; pay.Font = Theme.F(11, FontStyle.Bold);
            pay.Click += (s, e) => Pay();

            var voidBtn = Theme.Btn("Void last sale", false, true); voidBtn.Click += (s, e) => VoidLast();
            var reprint = Theme.Btn("Reprint receipt"); reprint.Click += (s, e) => Reprint();
            var rowBtns2 = Pair(voidBtn, reprint);

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 286, BackColor = Color.Transparent };
            // added bottom-to-top: the last one added ends up on top
            bottom.Controls.Add(rowBtns2);
            bottom.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent });
            bottom.Controls.Add(pay);
            bottom.Controls.Add(Row("Change", change, 34));
            bottom.Controls.Add(Row("Cash received", cash, 40));
            bottom.Controls.Add(Row("TOTAL", total, 46));
            bottom.Controls.Add(rowBtns);

            var rightIn = new Panel { BackColor = Color.Transparent };
            rightIn.Controls.Add(cart);
            rightIn.Controls.Add(bottom);
            root.Controls.Add(UI.Card("Current Order", rightIn), 1, 0);

            Controls.Add(root);
        }

        static Panel Row(string text, Control right, int h)
        {
            var p = new Panel { Dock = DockStyle.Top, Height = h, BackColor = Color.Transparent };
            var l = new Label { Text = text, Dock = DockStyle.Left, Width = 120, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Theme.Muted, Font = Theme.F(10) };
            right.Dock = DockStyle.Fill;
            p.Controls.Add(right);
            p.Controls.Add(l);
            return p;
        }

        static Panel Pair(Button a, Button b)
        {
            var t = new TableLayoutPanel { Dock = DockStyle.Top, Height = 44, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            a.Dock = DockStyle.Fill; a.Margin = new Padding(0, 4, 4, 4);
            b.Dock = DockStyle.Fill; b.Margin = new Padding(4, 4, 0, 4);
            t.Controls.Add(a, 0, 0);
            t.Controls.Add(b, 1, 0);
            return t;
        }

        public void Reload()
        {
            FillProducts();
            RefreshCart();
        }

        void FillProducts()
        {
            string key = search.Text.Trim().ToLower();
            products.Rows.Clear();
            foreach (var p in Store.Products.Where(x => key == "" || x.Name.ToLower().Contains(key) || x.Category.ToLower().Contains(key)).OrderBy(x => x.Name))
            {
                int i = products.Rows.Add(p.Name, p.Category, "P" + p.Price.ToString("N2"), p.Stock);
                products.Rows[i].Tag = p;
            }
        }

        void AddToOrder()
        {
            if (products.CurrentRow == null) return;
            var p = products.CurrentRow.Tag as Product;
            if (p == null) return;

            int want = (int)qty.Value;
            int already = lines.Where(l => l.P == p).Sum(l => l.Qty);
            if (want + already > p.Stock)
            {
                MessageBox.Show("Not enough stock. Only " + (p.Stock - already) + " left for " + p.Name + ".", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var line = lines.FirstOrDefault(l => l.P == p);
            if (line != null) line.Qty += want;
            else lines.Add(new SaleLine { P = p, Name = p.Name, Qty = want, Price = p.Price });

            qty.Value = 1;
            RefreshCart();
        }

        void RemoveSelected()
        {
            if (cart.CurrentRow == null) return;
            int i = cart.CurrentRow.Index;
            if (i >= 0 && i < lines.Count)
            {
                lines.RemoveAt(i);
                RefreshCart();
            }
        }

        void RefreshCart()
        {
            cart.Rows.Clear();
            foreach (var l in lines)
                cart.Rows.Add(l.Name, l.Qty, "P" + l.Price.ToString("N2"), "P" + l.Total.ToString("N2"));
            total.Text = "P" + lines.Sum(l => l.Total).ToString("N2");
            UpdateChange();
        }

        void UpdateChange()
        {
            double c;
            double due = lines.Sum(l => l.Total);
            if (double.TryParse(cash.Text, out c) && c >= due && due > 0)
            {
                change.Text = "P" + (c - due).ToString("N2");
                change.ForeColor = Theme.Green;
            }
            else
            {
                change.Text = "P0.00";
                change.ForeColor = Theme.Muted;
            }
        }

        void Pay()
        {
            double c;
            if (lines.Count == 0) { MessageBox.Show("Add at least one item first.", "Order", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (!double.TryParse(cash.Text, out c)) { MessageBox.Show("Enter the cash received.", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            string err;
            var sale = Store.CompleteSale(lines, c, cashier, out err);
            if (sale == null) { MessageBox.Show(err, "Cannot complete sale", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            Dialogs.ShowReceipt(sale.Receipt());
            lines.Clear();
            cash.Text = "";
            Reload();
        }

        void VoidLast()
        {
            var s = Store.LastValidSale();
            if (s == null) { MessageBox.Show("There is no sale to void."); return; }
            if (MessageBox.Show("Void Order #" + s.No.ToString("D4") + " (P" + s.Total.ToString("N2") + ")? Stock will be restored.",
                "Void sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Store.Void(s);
            Reload();
        }

        void Reprint()
        {
            var s = Store.LastValidSale();
            if (s == null) { MessageBox.Show("There is no receipt to reprint."); return; }
            Dialogs.ShowReceipt("*** DUPLICATE COPY ***\r\n" + s.Receipt());
        }
    }

    // =====================================================================
    //  INVENTORY
    // =====================================================================
    class InventoryPage : Panel, IPage
    {
        readonly bool manager;
        readonly DataGridView grid;
        readonly TextBox search;
        readonly CheckBox lowOnly;

        public InventoryPage(bool isManager)
        {
            manager = isManager;
            Dock = DockStyle.Fill;
            BackColor = Theme.Bg;
            Padding = new Padding(16);

            grid = new DataGridView();
            Theme.StyleGrid(grid);
            Theme.AddCol(grid, "ITEM", 26);
            Theme.AddCol(grid, "CATEGORY", 16);
            Theme.AddCol(grid, "PRICE", 14, true);
            Theme.AddCol(grid, "STOCK", 24);
            Theme.AddCol(grid, "STATUS", 14);
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 3) return;
                var p = grid.Rows[e.RowIndex].Tag as Product;
                if (p != null) Theme.PaintStock(e, p.Stock);
            };

            var bar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };
            search = new TextBox { Location = new Point(0, 4), Width = 240, Font = Theme.F(10.5f), PlaceholderText = "Search supplies..." };
            search.TextChanged += (s, e) => Reload();
            lowOnly = new CheckBox { Text = "Low stock only", Location = new Point(256, 6), AutoSize = true, ForeColor = Theme.Muted };
            lowOnly.CheckedChanged += (s, e) => Reload();
            bar.Controls.Add(search);
            bar.Controls.Add(lowOnly);

            if (manager)
            {
                var flow = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, BackColor = Color.Transparent };
                var add = Theme.Btn("Add item", true);
                var stock = Theme.Btn("Update stock");
                var price = Theme.Btn("Change price");
                var rem = Theme.Btn("Remove", false, true);
                add.Click += (s, e) => AddItem();
                stock.Click += (s, e) => UpdateStock();
                price.Click += (s, e) => ChangePrice();
                rem.Click += (s, e) => RemoveItem();
                flow.Controls.AddRange(new Control[] { add, stock, price, rem });
                bar.Controls.Add(flow);
            }

            var inner = new Panel { BackColor = Color.Transparent };
            inner.Controls.Add(grid);
            inner.Controls.Add(bar);

            var card = UI.Card("Inventory", inner);
            card.Margin = new Padding(8);
            var wrap = UI.Grid(1, 1);
            wrap.Controls.Add(card, 0, 0);
            Controls.Add(wrap);
        }

        public void Reload()
        {
            string key = search.Text.Trim().ToLower();
            grid.Rows.Clear();
            foreach (var p in Store.Products
                .Where(x => (key == "" || x.Name.ToLower().Contains(key) || x.Category.ToLower().Contains(key)) &&
                            (!lowOnly.Checked || x.Stock <= Store.LowStockLevel))
                .OrderBy(x => x.Name))
            {
                string status = p.Stock == 0 ? "Out of stock" : p.Stock <= Store.LowStockLevel ? "Low" : "In stock";
                int i = grid.Rows.Add(p.Name, p.Category, "P" + p.Price.ToString("N2"), p.Stock, status);
                grid.Rows[i].Tag = p;
                grid.Rows[i].Cells[4].Style.ForeColor = p.Stock == 0 ? Theme.Red : p.Stock <= Store.LowStockLevel ? Theme.Amber : Theme.Green;
            }
        }

        Product Selected()
        {
            if (grid.CurrentRow == null) { MessageBox.Show("Select an item first."); return null; }
            return grid.CurrentRow.Tag as Product;
        }

        void AddItem()
        {
            var p = Dialogs.AskProduct();
            if (p == null) return;
            Store.Products.Add(p);
            Reload();
        }

        void UpdateStock()
        {
            var p = Selected();
            if (p == null) return;
            string s = Dialogs.Ask("Update stock", "Quantity to add to \"" + p.Name + "\" (use a negative number to remove). Current stock: " + p.Stock, "0");
            if (s == null) return;
            int delta;
            if (!int.TryParse(s, out delta)) { MessageBox.Show("Enter a whole number."); return; }
            if (p.Stock + delta < 0) { MessageBox.Show("Stock cannot go below zero."); return; }
            p.Stock += delta;
            Reload();
        }

        void ChangePrice()
        {
            var p = Selected();
            if (p == null) return;
            string s = Dialogs.Ask("Change price", "New price for \"" + p.Name + "\" (current: P" + p.Price.ToString("N2") + ")", p.Price.ToString("0.##"));
            if (s == null) return;
            double price;
            if (!double.TryParse(s, out price) || price <= 0) { MessageBox.Show("Enter a valid price."); return; }
            p.Price = price;
            Reload();
        }

        void RemoveItem()
        {
            var p = Selected();
            if (p == null) return;
            if (MessageBox.Show("Remove \"" + p.Name + "\" from the inventory?", "Remove item", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Store.Products.Remove(p);
            Reload();
        }
    }

    // =====================================================================
    //  RECORDS  (purchase records + receipt viewer)
    // =====================================================================
    class RecordsPage : Panel, IPage
    {
        readonly bool manager;
        readonly DataGridView grid;
        readonly TextBox receipt;
        readonly ComboBox filter;

        public RecordsPage(bool isManager)
        {
            manager = isManager;
            Dock = DockStyle.Fill;
            BackColor = Theme.Bg;
            Padding = new Padding(16);

            var root = UI.Grid(2, 1);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            grid = new DataGridView();
            Theme.StyleGrid(grid);
            Theme.AddCol(grid, "RECEIPT", 12);
            Theme.AddCol(grid, "DATE / TIME", 26);
            Theme.AddCol(grid, "ITEMS", 10, true);
            Theme.AddCol(grid, "TOTAL", 18, true);
            Theme.AddCol(grid, "CASHIER", 16);
            Theme.AddCol(grid, "STATUS", 14);
            grid.SelectionChanged += (s, e) => ShowSelected();

            filter = new ComboBox { Dock = DockStyle.Left, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.F(10) };
            filter.Items.AddRange(new object[] { "All sales", "Today", "Voided only" });
            filter.SelectedIndex = 0;
            filter.SelectedIndexChanged += (s, e) => Reload();

            var bar = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.Transparent };
            bar.Controls.Add(filter);
            if (manager)
            {
                var voidBtn = Theme.Btn("Void selected", false, true);
                voidBtn.Dock = DockStyle.Right; voidBtn.Width = 140;
                voidBtn.Click += (s, e) => VoidSelected();
                bar.Controls.Add(voidBtn);
            }

            var inner = new Panel { BackColor = Color.Transparent };
            inner.Controls.Add(grid);
            inner.Controls.Add(bar);
            root.Controls.Add(UI.Card("Purchase Records", inner), 0, 0);

            receipt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical
            };
            root.Controls.Add(UI.Card("Receipt", receipt), 1, 0);

            Controls.Add(root);
        }

        public void Reload()
        {
            IEnumerable<Sale> src = Store.Sales;
            if (filter.SelectedIndex == 1) src = src.Where(s => s.Time.Date == DateTime.Today);
            else if (filter.SelectedIndex == 2) src = src.Where(s => s.Voided);

            grid.Rows.Clear();
            foreach (var s in src.OrderByDescending(x => x.Time))
            {
                int i = grid.Rows.Add("#" + s.No.ToString("D4"), s.Time.ToString("yyyy-MM-dd  h:mm tt"), s.Items, "P" + s.Total.ToString("N2"), s.Cashier, s.Voided ? "Voided" : "Completed");
                grid.Rows[i].Tag = s;
                grid.Rows[i].Cells[5].Style.ForeColor = s.Voided ? Theme.Red : Theme.Green;
            }
            ShowSelected();
        }

        void ShowSelected()
        {
            var s = grid.CurrentRow == null ? null : grid.CurrentRow.Tag as Sale;
            receipt.Text = s == null ? "Select a record to view its receipt." : s.Receipt().Replace("\r\n", "\n").Replace("\n", "\r\n");
            receipt.Select(0, 0);
        }

        void VoidSelected()
        {
            var s = grid.CurrentRow == null ? null : grid.CurrentRow.Tag as Sale;
            if (s == null) { MessageBox.Show("Select a record first."); return; }
            if (s.Voided) { MessageBox.Show("That sale is already voided."); return; }
            if (MessageBox.Show("Void Order #" + s.No.ToString("D4") + "? Stock will be restored.", "Void sale",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Store.Void(s);
            Reload();
        }
    }

    // =====================================================================
    //  REPORTS  (manager only)
    // =====================================================================
    class ReportsPage : Panel, IPage
    {
        readonly KpiCard k1, k2, k3, k4;
        readonly BarChart bars;
        readonly DonutChart donut;

        public ReportsPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Bg;
            Padding = new Padding(16);

            var root = UI.Grid(2, 2);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var kp = UI.Grid(4, 1);
            for (int i = 0; i < 4; i++) kp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            k1 = new KpiCard("Total Revenue", Theme.Blue);
            k2 = new KpiCard("Completed Sales", Theme.Green);
            k3 = new KpiCard("Units Sold", Theme.Amber);
            k4 = new KpiCard("Voided Sales", Theme.Red);
            kp.Controls.Add(k1, 0, 0);
            kp.Controls.Add(k2, 1, 0);
            kp.Controls.Add(k3, 2, 0);
            kp.Controls.Add(k4, 3, 0);
            root.Controls.Add(kp, 0, 0);
            root.SetColumnSpan(kp, 2);

            bars = new BarChart();
            var export = Theme.Btn("Export CSV"); export.Height = 28; export.Width = 100;
            export.Click += (s, e) => ExportCsv();
            root.Controls.Add(UI.Card("Sales by hour of day", bars, export), 0, 1);

            donut = new DonutChart();
            root.Controls.Add(UI.Card("Revenue by category", donut), 1, 1);

            Controls.Add(root);
        }

        public void Reload()
        {
            var valid = Store.Valid.ToList();
            double rev = valid.Sum(s => s.Total);
            int units = valid.Sum(s => s.Items);
            int voided = Store.Sales.Count(s => s.Voided);
            double avg = valid.Count > 0 ? rev / valid.Count : 0;

            k1.Set("P" + rev.ToString("N2"), "Average sale P" + avg.ToString("N2"), Theme.Muted);
            k2.Set(valid.Count.ToString(), "All recorded sales", Theme.Muted);
            k3.Set(units.ToString(), "Across all items", Theme.Muted);
            k4.Set(voided.ToString(), voided > 0 ? "Stock was restored" : "None so far", voided > 0 ? Theme.Red : Theme.Green);

            string[] labels;
            double[] v = Store.RevenueByHourAll(out labels);
            bars.SetData(v, labels);

            var cats = Store.CategoryRevenue();
            donut.SetData(cats.Select(c => c.Value).ToArray(), cats.Select(c => c.Key).ToArray());
        }

        static string Q(string s)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        void ExportCsv()
        {
            using (var d = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = "sales-report.csv" })
            {
                if (d.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("Receipt,DateTime,Cashier,Item,Qty,UnitPrice,LineTotal,Status");
                foreach (var s in Store.Sales)
                    foreach (var l in s.Lines)
                        sb.AppendLine(string.Join(",", new[]
                        {
                            s.No.ToString("D4"), s.Time.ToString("yyyy-MM-dd HH:mm"), Q(s.Cashier), Q(l.Name),
                            l.Qty.ToString(), l.Price.ToString("F2"), l.Total.ToString("F2"), s.Voided ? "Voided" : "Completed"
                        }));
                try
                {
                    File.WriteAllText(d.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Saved to:\n" + d.FileName, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not save the file:\n" + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
