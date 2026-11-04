using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    interface IPage
    {
        void Reload();
    }

    // Frames 2 and 3 of the Figma prototype: sidebar on the left, red top bar, content area
    class MainForm : Form
    {
        public bool LoggedOut;

        readonly bool manager;
        readonly Panel content;
        readonly Label title;
        readonly FlowLayoutPanel nav;
        readonly List<Button> navButtons = new List<Button>();
        readonly Dictionary<Button, Panel> pages = new Dictionary<Button, Panel>();

        public MainForm(bool isManager)
        {
            manager = isManager;
            string role = manager ? "Manager" : "Staff";

            Text = "School Supplies POS - " + role;
            ClientSize = new Size(1320, 800);
            MinimumSize = new Size(1150, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            Font = Theme.F(9.5f);

            content = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg };

            // ----- header -----
            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
            title = new Label
            {
                Location = new Point(24, 16),
                AutoSize = true,
                Font = Theme.F(16, FontStyle.Bold),
                ForeColor = Theme.Ink
            };
            var date = new Label
            {
                Dock = DockStyle.Right,
                Width = 260,
                TextAlign = ContentAlignment.MiddleRight,
                Text = DateTime.Now.ToString("dddd, MMMM d") + "   ",
                ForeColor = Theme.Muted,
                Font = Theme.F(10)
            };
            header.Controls.Add(title);
            header.Controls.Add(date);
            header.Paint += (s, e) =>
            {
                using (var p = new Pen(Theme.Border))
                    e.Graphics.DrawLine(p, 0, header.Height - 1, header.Width, header.Height - 1);
            };

            // ----- sidebar -----
            var sidebar = new Panel { Dock = DockStyle.Left, Width = 224, BackColor = Color.White };
            sidebar.Paint += (s, e) =>
            {
                using (var p = new Pen(Theme.Border))
                    e.Graphics.DrawLine(p, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height);
            };

            nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.White
            };
            sidebar.Controls.Add(nav);                      // Fill first

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.White };
            var who = new Label
            {
                Text = "Logged in as\n" + role,
                Font = Theme.F(10, FontStyle.Bold),
                ForeColor = Theme.Ink,
                Location = new Point(20, 8),
                AutoSize = true
            };
            var logout = Theme.Btn("Logout", false, true);
            logout.SetBounds(16, 66, 192, 38);
            logout.Click += (s, e) => { LoggedOut = true; Close(); };
            bottom.Controls.Add(who);
            bottom.Controls.Add(logout);
            sidebar.Controls.Add(bottom);

            var logo = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.White };
            var mark = new Label
            {
                Text = "SP",
                ForeColor = Color.White,
                BackColor = Theme.Blue,
                Font = Theme.F(12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(20, 22, 40, 40)
            };
            var brand = new Label
            {
                Text = "SchoolPOS",
                Font = Theme.F(14, FontStyle.Bold),
                ForeColor = Theme.Ink,
                Location = new Point(68, 22),
                AutoSize = true
            };
            var brandSub = new Label
            {
                Text = "Supplies & Sales",
                Font = Theme.F(8.5f),
                ForeColor = Theme.Muted,
                Location = new Point(70, 46),
                AutoSize = true
            };
            logo.Controls.Add(mark);
            logo.Controls.Add(brand);
            logo.Controls.Add(brandSub);
            sidebar.Controls.Add(logo);

            // ----- pages -----
            AddNav("Dashboard", new DashboardPage());
            AddNav("Sales", new SalesPage(role));
            AddNav("Inventory", new InventoryPage(manager));
            AddNav("Records", new RecordsPage(manager));
            if (manager) AddNav("Reports", new ReportsPage());

            // Fill first, then the docked edges; the last one added is laid out first
            Controls.Add(content);
            Controls.Add(header);
            Controls.Add(sidebar);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 5, BackColor = Theme.Red });

            Navigate(navButtons[0]);
        }

        void AddNav(string text, Panel page)
        {
            var b = new Button
            {
                Text = "   " + text,
                Width = 198,
                Height = 44,
                Margin = new Padding(0, 2, 0, 2),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = Theme.F(10.5f),
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Theme.Bg;
            b.Click += (s, e) => Navigate(b);

            page.Visible = false;
            content.Controls.Add(page);
            pages[b] = page;
            navButtons.Add(b);
            nav.Controls.Add(b);
        }

        void Navigate(Button active)
        {
            foreach (var b in navButtons)
            {
                bool on = b == active;
                b.BackColor = on ? Theme.BlueSoft : Color.White;
                b.ForeColor = on ? Theme.Blue : Theme.Muted;
                b.Font = Theme.F(10.5f, on ? FontStyle.Bold : FontStyle.Regular);
                pages[b].Visible = on;
            }
            title.Text = active.Text.Trim();
            ((IPage)pages[active]).Reload();
        }
    }
}
