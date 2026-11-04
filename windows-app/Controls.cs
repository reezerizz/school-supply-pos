using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    // ===================== THEME =====================
    static class Theme
    {
        public static readonly Color Blue = Color.FromArgb(34, 52, 216);
        public static readonly Color BlueSoft = Color.FromArgb(238, 241, 255);
        public static readonly Color Red = Color.FromArgb(220, 38, 38);
        public static readonly Color Green = Color.FromArgb(22, 163, 74);
        public static readonly Color Amber = Color.FromArgb(245, 158, 11);
        public static readonly Color Bg = Color.FromArgb(243, 245, 250);
        public static readonly Color Border = Color.FromArgb(229, 231, 235);
        public static readonly Color GridLine = Color.FromArgb(238, 240, 245);
        public static readonly Color Ink = Color.FromArgb(17, 24, 39);
        public static readonly Color Muted = Color.FromArgb(107, 114, 128);

        public static readonly Color[] Palette =
        {
            Color.FromArgb(34, 52, 216), Color.FromArgb(245, 158, 11), Color.FromArgb(22, 163, 74),
            Color.FromArgb(220, 38, 38), Color.FromArgb(139, 92, 246), Color.FromArgb(6, 182, 212)
        };

        public static Font F(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font("Segoe UI", size, style);
        }

        public static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Max(2, radius * 2);
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static double NiceMax(double v)
        {
            if (v <= 0) return 100;
            double mag = Math.Pow(10, Math.Floor(Math.Log10(v)));
            double n = v / mag;
            double nice = n <= 1 ? 1 : n <= 2 ? 2 : n <= 4 ? 4 : n <= 8 ? 8 : 10;
            return nice * mag;
        }

        public static Button Btn(string text, bool primary = false, bool danger = false)
        {
            Color bg = primary ? Blue : danger ? Red : Color.White;
            Color fg = (primary || danger) ? Color.White : Ink;
            var b = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Height = 36,
                Width = 110,
                Font = F(9.5f, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                BackColor = bg,
                ForeColor = fg
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = (primary || danger) ? bg : Border;
            return b;
        }

        public static void StyleGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.ReadOnly = true;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            g.ColumnHeadersDefaultCellStyle.Font = F(8.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Muted;
            g.DefaultCellStyle.Font = F(9.5f);
            g.DefaultCellStyle.ForeColor = Ink;
            g.DefaultCellStyle.SelectionBackColor = BlueSoft;
            g.DefaultCellStyle.SelectionForeColor = Ink;
            g.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            g.RowTemplate.Height = 40;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = GridLine;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        public static void AddCol(DataGridView g, string header, float weight, bool right = false)
        {
            int i = g.Columns.Add(header, header);
            g.Columns[i].FillWeight = weight;
            g.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            if (right)
            {
                g.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                g.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        public static Color StockColor(int stock)
        {
            return stock <= 0 ? Red : stock <= 10 ? Red : stock <= 25 ? Amber : Green;
        }

        // Draws "number + coloured bar" inside a grid cell
        public static void PaintStock(DataGridViewCellPaintingEventArgs e, int stock)
        {
            e.PaintBackground(e.CellBounds, true);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var textRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y, 40, e.CellBounds.Height);
            TextRenderer.DrawText(g, stock.ToString(), F(9.5f, FontStyle.Bold), textRect, Ink,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            int tw = Math.Max(20, e.CellBounds.Width - 52 - 12);
            var track = new Rectangle(e.CellBounds.X + 48, e.CellBounds.Y + e.CellBounds.Height / 2 - 3, tw, 6);
            using (var tp = Round(track, 3)) using (var tb = new SolidBrush(GridLine)) g.FillPath(tb, tp);

            if (stock > 0)
            {
                int fw = Math.Max(6, (int)(tw * Math.Min(1.0, stock / 100.0)));
                var fill = new Rectangle(track.X, track.Y, fw, 6);
                using (var fp = Round(fill, 3)) using (var fb = new SolidBrush(StockColor(stock))) g.FillPath(fb, fp);
            }
            e.Handled = true;
        }

        public static void DrawTip(Graphics g, string text, float x, float y, Rectangle bounds, Font f)
        {
            var sz = g.MeasureString(text, f);
            float w = sz.Width + 16, h = sz.Height + 8;
            float rx = Math.Min(Math.Max(bounds.Left, x - w / 2), Math.Max(bounds.Left, bounds.Right - w));
            float ry = Math.Max(bounds.Top, y - h - 10);
            using (var path = Round(new Rectangle((int)rx, (int)ry, (int)w, (int)h), 6))
            using (var b = new SolidBrush(Ink))
                g.FillPath(b, path);
            g.DrawString(text, f, Brushes.White, rx + 8, ry + 4);
        }
    }

    // ===================== ROUNDED CARD =====================
    class RoundedPanel : Panel
    {
        public int Radius = 14;
        public Color Fill = Color.White;
        public Color Stroke = Theme.Border;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Theme.Round(r, Radius))
            using (var fill = new SolidBrush(Fill))
            using (var pen = new Pen(Stroke))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            }
        }
    }

    // ===================== UI HELPERS =====================
    static class UI
    {
        public static RoundedPanel Card(string title, Control content, Control right, out Label titleLabel)
        {
            var card = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(8), Padding = new Padding(16) };

            content.Dock = DockStyle.Fill;
            card.Controls.Add(content);              // Fill control first

            var head = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.Transparent };
            titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(0, 4),
                Font = Theme.F(11, FontStyle.Bold),
                ForeColor = Theme.Ink,
                BackColor = Color.Transparent
            };
            head.Controls.Add(titleLabel);
            if (right != null)
            {
                right.Dock = DockStyle.Right;
                head.Controls.Add(right);
            }
            card.Controls.Add(head);                 // then the Top header
            return card;
        }

        public static RoundedPanel Card(string title, Control content, Control right = null)
        {
            Label l;
            return Card(title, content, right, out l);
        }

        public static TableLayoutPanel Grid(int cols, int rows)
        {
            return new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cols,
                RowCount = rows,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
        }

        public static string Delta(double cur, double prev, string vs, out Color color)
        {
            if (prev <= 0)
            {
                color = Theme.Muted;
                return "No data " + vs;
            }
            double pct = (cur - prev) / prev * 100;
            color = pct >= 0 ? Theme.Green : Theme.Red;
            return (pct >= 0 ? "▲ " : "▼ ") + Math.Abs(pct).ToString("0.#") + "% " + vs;
        }
    }

    // ===================== KPI CARD =====================
    class KpiCard : RoundedPanel
    {
        readonly Label value, sub;
        readonly Color accent;

        public KpiCard(string title, Color accent)
        {
            this.accent = accent;
            Dock = DockStyle.Fill;
            Margin = new Padding(8);

            var t = new Label { Text = title, AutoSize = true, Location = new Point(18, 16), ForeColor = Theme.Muted, Font = Theme.F(9.5f), BackColor = Color.Transparent };
            value = new Label { Text = "-", AutoSize = true, Location = new Point(18, 40), ForeColor = Theme.Ink, Font = Theme.F(20, FontStyle.Bold), BackColor = Color.Transparent };
            sub = new Label { Text = "", AutoSize = true, Location = new Point(18, 86), ForeColor = Theme.Muted, Font = Theme.F(8.5f), BackColor = Color.Transparent };
            Controls.Add(t);
            Controls.Add(value);
            Controls.Add(sub);
        }

        public void Set(string v, string subText, Color subColor)
        {
            value.Text = v;
            sub.Text = subText;
            sub.ForeColor = subColor;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(accent))
                e.Graphics.FillEllipse(b, Width - 32, 18, 12, 12);
        }
    }

    // ===================== LINE CHART =====================
    class LineChart : Control
    {
        public double[] Values = new double[0];
        public string[] Labels = new string[0];
        public string Prefix = "P";
        public Color LineColor = Theme.Blue;

        readonly Font small = Theme.F(8.5f);
        int hover = -1;

        public LineChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        public void SetData(double[] values, string[] labels)
        {
            Values = values;
            Labels = labels;
            hover = -1;
            Invalidate();
        }

        Rectangle Plot()
        {
            return new Rectangle(60, 12, Math.Max(10, Width - 60 - 18), Math.Max(10, Height - 12 - 30));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int n = Values.Length;
            if (n < 2) return;
            var plot = Plot();
            int idx = (int)Math.Round((e.X - plot.Left) / (double)plot.Width * (n - 1));
            idx = Math.Max(0, Math.Min(n - 1, idx));
            if (idx != hover) { hover = idx; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int n = Values.Length;
            var plot = Plot();
            double max = Theme.NiceMax(n == 0 ? 0 : Values.Max());

            // grid + y labels
            using (var gridPen = new Pen(Theme.GridLine))
            using (var br = new SolidBrush(Theme.Muted))
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = plot.Bottom - plot.Height * i / 4f;
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    string t = Prefix + (max * i / 4).ToString("N0");
                    var sz = g.MeasureString(t, small);
                    g.DrawString(t, small, br, plot.Left - 6 - sz.Width, y - sz.Height / 2);
                }
            }

            if (n < 2)
            {
                TextRenderer.DrawText(g, "Not enough data yet", small, ClientRectangle, Theme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                float x = plot.Left + plot.Width * i / (float)(n - 1);
                float y = plot.Bottom - (float)(Values[i] / max) * plot.Height;
                pts[i] = new PointF(x, y);
            }

            // gradient fill under the curve
            using (var path = new GraphicsPath())
            {
                path.AddCurve(pts, 0.4f);
                path.AddLine(pts[n - 1], new PointF(pts[n - 1].X, plot.Bottom));
                path.AddLine(new PointF(pts[n - 1].X, plot.Bottom), new PointF(pts[0].X, plot.Bottom));
                path.CloseFigure();
                using (var lg = new LinearGradientBrush(new Rectangle(plot.Left, plot.Top - 1, plot.Width, plot.Height + 2),
                    Color.FromArgb(70, LineColor), Color.FromArgb(0, LineColor), 90f))
                    g.FillPath(lg, path);
            }

            using (var pen = new Pen(LineColor, 2.5f))
                g.DrawCurve(pen, pts, 0.4f);

            // x labels
            int step = (int)Math.Ceiling(n / 8.0);
            using (var br = new SolidBrush(Theme.Muted))
            {
                for (int i = 0; i < n; i += step)
                {
                    string t = i < Labels.Length ? Labels[i] : "";
                    var sz = g.MeasureString(t, small);
                    g.DrawString(t, small, br, pts[i].X - sz.Width / 2, plot.Bottom + 8);
                }
            }

            // hover
            if (hover >= 0 && hover < n)
            {
                var p = pts[hover];
                using (var dash = new Pen(Theme.Border) { DashStyle = DashStyle.Dash })
                    g.DrawLine(dash, p.X, plot.Top, p.X, plot.Bottom);
                g.FillEllipse(Brushes.White, p.X - 6, p.Y - 6, 12, 12);
                using (var b = new SolidBrush(LineColor)) g.FillEllipse(b, p.X - 4, p.Y - 4, 8, 8);
                string lbl = hover < Labels.Length ? Labels[hover] : "";
                Theme.DrawTip(g, lbl + ": " + Prefix + Values[hover].ToString("N2"), p.X, p.Y, ClientRectangle, small);
            }
        }
    }

    // ===================== BAR CHART =====================
    class BarChart : Control
    {
        public double[] Values = new double[0];
        public string[] Labels = new string[0];
        public string Prefix = "P";

        readonly Font small = Theme.F(8.5f);
        int hover = -1;

        public BarChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        public void SetData(double[] values, string[] labels)
        {
            Values = values;
            Labels = labels;
            hover = -1;
            Invalidate();
        }

        Rectangle Plot()
        {
            return new Rectangle(60, 12, Math.Max(10, Width - 60 - 12), Math.Max(10, Height - 12 - 30));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int n = Values.Length;
            if (n == 0) return;
            var plot = Plot();
            int idx = (int)((e.X - plot.Left) / (plot.Width / (float)n));
            if (idx < 0 || idx >= n) idx = -1;
            if (idx != hover) { hover = idx; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int n = Values.Length;
            var plot = Plot();
            double max = Theme.NiceMax(n == 0 ? 0 : Values.Max());

            using (var gridPen = new Pen(Theme.GridLine))
            using (var br = new SolidBrush(Theme.Muted))
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = plot.Bottom - plot.Height * i / 4f;
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    string t = Prefix + (max * i / 4).ToString("N0");
                    var sz = g.MeasureString(t, small);
                    g.DrawString(t, small, br, plot.Left - 6 - sz.Width, y - sz.Height / 2);
                }
            }

            if (n == 0) return;

            float slot = plot.Width / (float)n;
            float bw = slot * 0.58f;
            double best = Values.Max();
            int step = (int)Math.Ceiling(n / 10.0);

            for (int i = 0; i < n; i++)
            {
                float h = (float)(Values[i] / max) * plot.Height;
                float x = plot.Left + slot * i + (slot - bw) / 2;
                var rect = new RectangleF(x, plot.Bottom - h, bw, h);
                Color c = (i == hover) ? Theme.Blue : (Values[i] == best && best > 0 ? Theme.Blue : Color.FromArgb(170, 182, 245));

                using (var b = new SolidBrush(c))
                {
                    if (h >= 8)
                    {
                        using (var path = Theme.Round(Rectangle.Round(rect), 4)) g.FillPath(b, path);
                    }
                    else if (h > 0)
                    {
                        g.FillRectangle(b, rect);
                    }
                }

                if (i % step == 0)
                {
                    string t = i < Labels.Length ? Labels[i] : "";
                    var sz = g.MeasureString(t, small);
                    using (var br = new SolidBrush(Theme.Muted))
                        g.DrawString(t, small, br, x + bw / 2 - sz.Width / 2, plot.Bottom + 8);
                }
            }

            if (hover >= 0 && hover < n)
            {
                float h = (float)(Values[hover] / max) * plot.Height;
                float cx = plot.Left + slot * hover + slot / 2;
                string lbl = hover < Labels.Length ? Labels[hover] : "";
                Theme.DrawTip(g, lbl + ": " + Prefix + Values[hover].ToString("N2"), cx, plot.Bottom - h, ClientRectangle, small);
            }
        }
    }

    // ===================== DONUT CHART =====================
    class DonutChart : Control
    {
        public double[] Values = new double[0];
        public string[] Labels = new string[0];
        public string Prefix = "P";

        readonly Font small = Theme.F(9f);
        readonly Font big = Theme.F(13, FontStyle.Bold);

        public DonutChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        public void SetData(double[] values, string[] labels)
        {
            Values = values;
            Labels = labels;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            double total = Values.Sum();
            int size = Math.Max(60, Math.Min(Height - 20, (int)(Width * 0.55)));
            float pw = size * 0.17f;
            var ring = new RectangleF(10 + pw / 2, (Height - size) / 2f + pw / 2, size - pw, size - pw);

            using (var track = new Pen(Theme.GridLine, pw))
                g.DrawEllipse(track, ring);

            if (total > 0)
            {
                float start = -90;
                for (int i = 0; i < Values.Length; i++)
                {
                    float sweep = (float)(360 * Values[i] / total);
                    if (sweep <= 0) continue;
                    using (var pen = new Pen(Theme.Palette[i % Theme.Palette.Length], pw))
                    {
                        pen.LineJoin = LineJoin.Round;
                        g.DrawArc(pen, ring, start, Math.Max(0.5f, sweep - (Values.Length > 1 ? 1.5f : 0f)));
                    }
                    start += sweep;
                }
            }

            // centre text
            var centre = new Rectangle(10, (Height - size) / 2, size, size);
            TextRenderer.DrawText(g, total > 0 ? Prefix + total.ToString("N0") : "No data", big,
                new Rectangle(centre.X, centre.Y + size / 2 - 22, size, 24), Theme.Ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, "Revenue", small,
                new Rectangle(centre.X, centre.Y + size / 2 + 2, size, 20), Theme.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // legend
            int lx = 10 + size + 24;
            int ly = Math.Max(10, (Height - Values.Length * 28) / 2);
            for (int i = 0; i < Values.Length; i++)
            {
                int y = ly + i * 28;
                using (var b = new SolidBrush(Theme.Palette[i % Theme.Palette.Length]))
                    g.FillEllipse(b, lx, y + 5, 10, 10);
                string name = i < Labels.Length ? Labels[i] : "";
                TextRenderer.DrawText(g, name, small, new Rectangle(lx + 18, y, 110, 20), Theme.Ink,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                string pct = total > 0 ? (Values[i] / total * 100).ToString("0") + "%" : "0%";
                TextRenderer.DrawText(g, pct, small, new Rectangle(lx + 128, y, 44, 20), Theme.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
            }
        }
    }
}
