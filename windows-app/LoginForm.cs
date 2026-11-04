using System;
using System.Drawing;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    // Landing page: same look as the app (blue brand panel like the logo, white cards, red top strip)
    class LoginForm : Form
    {
        public bool IsManager;

        public LoginForm()
        {
            Text = "School Supplies POS";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 680);
            MinimumSize = new Size(900, 600);
            BackColor = Theme.Bg;
            Font = Theme.F(10);

            var root = UI.Grid(2, 1);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ---------- left: brand panel ----------
            var left = UI.Grid(1, 1);
            left.BackColor = Theme.Blue;
            var brand = new Panel { Size = new Size(340, 300), Anchor = AnchorStyles.None, BackColor = Theme.Blue };

            var mark = new Label
            {
                Text = "SP",
                ForeColor = Theme.Blue,
                BackColor = Color.White,
                Font = Theme.F(22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, 0, 76, 76)
            };
            var name = new Label
            {
                Text = "SchoolPOS",
                ForeColor = Color.White,
                Font = Theme.F(32, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(-2, 100),
                BackColor = Color.Transparent
            };
            var sub = new Label
            {
                Text = "SCHOOL SUPPLIES\nPOINT OF SALE SYSTEM",
                ForeColor = Color.FromArgb(200, 208, 255),
                Font = Theme.F(11),
                AutoSize = true,
                Location = new Point(0, 156),
                BackColor = Color.Transparent
            };
            var tag = new Label
            {
                Text = "Sales  ·  Inventory  ·  Records  ·  Reports",
                ForeColor = Color.White,
                Font = Theme.F(10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 240),
                BackColor = Color.Transparent
            };
            brand.Controls.Add(mark);
            brand.Controls.Add(name);
            brand.Controls.Add(sub);
            brand.Controls.Add(tag);
            left.Controls.Add(brand, 0, 0);

            // ---------- right: role selection ----------
            var right = UI.Grid(1, 1);
            right.BackColor = Theme.Bg;
            var choose = new Panel { Size = new Size(470, 400), Anchor = AnchorStyles.None, BackColor = Color.Transparent };

            var welcome = new Label
            {
                Text = "Welcome",
                Font = Theme.F(24, FontStyle.Bold),
                ForeColor = Theme.Ink,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            var prompt = new Label
            {
                Text = "Select your role to continue",
                Font = Theme.F(11),
                ForeColor = Theme.Muted,
                AutoSize = true,
                Location = new Point(2, 46),
                BackColor = Color.Transparent
            };

            var mgr = RoleCard("Manager", "Inventory, prices, purchase records and sales reports", "M", Theme.Blue, 96,
                () => { IsManager = true; DialogResult = DialogResult.OK; });
            var stf = RoleCard("Staff", "Process sales, search supplies and print receipts", "S", Theme.Green, 214,
                () => { IsManager = false; DialogResult = DialogResult.OK; });

            var exit = Theme.Btn("Exit");
            exit.Location = new Point(0, 340);
            exit.Width = 110;
            exit.Click += (s, e) => { DialogResult = DialogResult.Cancel; };

            choose.Controls.Add(welcome);
            choose.Controls.Add(prompt);
            choose.Controls.Add(mgr);
            choose.Controls.Add(stf);
            choose.Controls.Add(exit);
            right.Controls.Add(choose, 0, 0);

            root.Controls.Add(left, 0, 0);
            root.Controls.Add(right, 1, 0);

            Controls.Add(root);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 5, BackColor = Theme.Red });
        }

        // A clickable card in the same style as the cards inside the app
        static RoundedPanel RoleCard(string title, string desc, string letter, Color accent, int y, Action onClick)
        {
            var card = new RoundedPanel { Bounds = new Rectangle(0, y, 470, 104), Cursor = Cursors.Hand };

            var icon = new Label
            {
                Text = letter,
                Bounds = new Rectangle(20, 26, 52, 52),
                BackColor = accent,
                ForeColor = Color.White,
                Font = Theme.F(16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var t = new Label
            {
                Text = title,
                Location = new Point(90, 22),
                AutoSize = true,
                Font = Theme.F(14, FontStyle.Bold),
                ForeColor = Theme.Ink,
                BackColor = Color.Transparent
            };
            var d = new Label
            {
                Text = desc,
                Location = new Point(92, 52),
                Size = new Size(300, 40),
                Font = Theme.F(9.5f),
                ForeColor = Theme.Muted,
                BackColor = Color.Transparent
            };
            var arrow = new Label
            {
                Text = "→",
                Bounds = new Rectangle(412, 30, 40, 40),
                Font = Theme.F(18, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var all = new Control[] { card, icon, t, d, arrow };
            card.Controls.Add(icon);
            card.Controls.Add(t);
            card.Controls.Add(d);
            card.Controls.Add(arrow);

            foreach (var c in all)
            {
                c.Cursor = Cursors.Hand;
                c.Click += (s, e) => onClick();
                c.MouseEnter += (s, e) => { card.Stroke = accent; card.Invalidate(); };
                c.MouseLeave += (s, e) =>
                {
                    if (!card.ClientRectangle.Contains(card.PointToClient(Cursor.Position)))
                    {
                        card.Stroke = Theme.Border;
                        card.Invalidate();
                    }
                };
            }
            return card;
        }
    }
}
