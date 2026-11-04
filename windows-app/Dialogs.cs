using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    static class Dialogs
    {
        static Form NewForm(string title, int w, int h)
        {
            return new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(w, h),
                Font = Theme.F(10),
                BackColor = Color.White
            };
        }

        // Simple one-box prompt. Returns null when cancelled.
        public static string Ask(string title, string prompt, string def)
        {
            using (var f = NewForm(title, 400, 160))
            {
                var l = new Label { Text = prompt, Left = 16, Top = 14, Width = 368, Height = 44 };
                var t = new TextBox { Left = 16, Top = 66, Width = 368, Text = def };
                var ok = Theme.Btn("OK", true); ok.SetBounds(200, 108, 90, 34); ok.DialogResult = DialogResult.OK;
                var cancel = Theme.Btn("Cancel"); cancel.SetBounds(298, 108, 86, 34); cancel.DialogResult = DialogResult.Cancel;
                f.AcceptButton = ok;
                f.CancelButton = cancel;
                f.Controls.AddRange(new Control[] { l, t, ok, cancel });
                t.SelectAll();
                return f.ShowDialog() == DialogResult.OK ? t.Text.Trim() : null;
            }
        }

        // Add-product form. Returns null when cancelled.
        public static Product AskProduct()
        {
            Product result = null;
            using (var f = NewForm("Add Product", 390, 290))
            {
                var name = new TextBox();
                var cat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
                cat.Items.AddRange(Store.Categories());
                var price = new TextBox();
                var stock = new TextBox { Text = "0" };

                int y = 16;
                Action<string, Control> add = (label, ctl) =>
                {
                    f.Controls.Add(new Label { Text = label, Left = 16, Top = y + 4, Width = 100 });
                    ctl.Left = 124; ctl.Top = y; ctl.Width = 244;
                    f.Controls.Add(ctl);
                    y += 42;
                };
                add("Item name", name);
                add("Category", cat);
                add("Price (P)", price);
                add("Starting stock", stock);

                var ok = Theme.Btn("Add", true); ok.SetBounds(180, 226, 90, 34);
                var cancel = Theme.Btn("Cancel"); cancel.SetBounds(278, 226, 90, 34); cancel.DialogResult = DialogResult.Cancel;
                f.CancelButton = cancel;
                f.AcceptButton = ok;
                f.Controls.Add(ok);
                f.Controls.Add(cancel);

                ok.Click += (s, e) =>
                {
                    string n = name.Text.Trim();
                    string c = cat.Text.Trim();
                    double p; int st;
                    if (n == "") { MessageBox.Show("Enter an item name."); return; }
                    if (c == "") { MessageBox.Show("Enter a category."); return; }
                    if (!double.TryParse(price.Text, out p) || p <= 0) { MessageBox.Show("Enter a valid price."); return; }
                    if (!int.TryParse(stock.Text, out st) || st < 0) { MessageBox.Show("Enter a valid stock count."); return; }
                    if (Store.Products.Any(x => x.Name.ToLower() == n.ToLower())) { MessageBox.Show("That item already exists. Use Update Stock instead."); return; }
                    result = new Product(n, c, p, st);
                    f.DialogResult = DialogResult.OK;
                };

                f.ShowDialog();
            }
            return result;
        }

        public static void ShowReceipt(string text)
        {
            using (var f = NewForm("Receipt", 400, 470))
            {
                var tb = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Text = text,
                    SelectionStart = 0,
                    ScrollBars = ScrollBars.Vertical,
                    Bounds = new Rectangle(16, 16, 368, 388)
                };
                var close = Theme.Btn("Close", true);
                close.SetBounds(294, 416, 90, 36);
                close.DialogResult = DialogResult.OK;
                f.AcceptButton = close;
                f.Controls.Add(tb);
                f.Controls.Add(close);
                tb.Select(0, 0);
                f.ShowDialog();
            }
        }
    }
}
