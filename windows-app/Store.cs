using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolSupplyPOS
{
    // ===================== MODELS =====================
    public class Product
    {
        public string Name;
        public string Category;
        public double Price;
        public int Stock;

        public Product(string name, string category, double price, int stock)
        {
            Name = name; Category = category; Price = price; Stock = stock;
        }
    }

    public class SaleLine
    {
        public Product P;
        public string Name;
        public int Qty;
        public double Price;
        public double Total { get { return Qty * Price; } }
    }

    public class Sale
    {
        public int No;
        public DateTime Time;
        public List<SaleLine> Lines = new List<SaleLine>();
        public double Total, Cash, Change;
        public bool Voided;
        public string Cashier;

        public int Items { get { return Lines.Sum(l => l.Qty); } }

        public string Receipt()
        {
            var sb = new StringBuilder();
            string bar = new string('=', 36);
            sb.AppendLine(bar);
            sb.AppendLine("        SCHOOL SUPPLIES POS");
            sb.AppendLine("        Receipt #" + No.ToString("D4"));
            sb.AppendLine("        " + Time.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine(bar);
            foreach (var l in Lines)
            {
                string n = l.Name.Length > 14 ? l.Name.Substring(0, 14) : l.Name;
                sb.AppendLine(string.Format("{0,-14}{1,3} x {2,7:F2} {3,8:F2}", n, l.Qty, l.Price, l.Total));
            }
            sb.AppendLine(new string('-', 36));
            sb.AppendLine(string.Format("TOTAL:   P{0:F2}", Total));
            sb.AppendLine(string.Format("CASH:    P{0:F2}", Cash));
            sb.AppendLine(string.Format("CHANGE:  P{0:F2}", Change));
            sb.AppendLine("Cashier: " + Cashier);
            sb.AppendLine(bar);
            if (Voided) sb.AppendLine("        *** VOIDED ***");
            else sb.AppendLine("     Thank you! Come again!");
            return sb.ToString();
        }
    }

    public class ProductStat
    {
        public Product P;
        public string Name;
        public int Units;
        public double Revenue;
    }

    // ===================== DATA + LOGIC =====================
    public static class Store
    {
        public static readonly List<Product> Products = new List<Product>();
        public static readonly List<Sale> Sales = new List<Sale>();
        public const int LowStockLevel = 10;

        const int FirstHour = 8, LastHour = 20;
        static int nextNo = 1;

        public static void Init()
        {
            Products.Add(new Product("Notebook", "Paper", 35, 50));
            Products.Add(new Product("Bond Paper (ream)", "Paper", 280, 15));
            Products.Add(new Product("Folder", "Paper", 7, 70));
            Products.Add(new Product("Ballpen", "Writing", 12, 100));
            Products.Add(new Product("Pencil", "Writing", 8, 80));
            Products.Add(new Product("Eraser", "Writing", 10, 60));
            Products.Add(new Product("Crayons", "Art", 60, 20));
            Products.Add(new Product("Ruler", "Tools", 15, 40));
            Products.Add(new Product("Scissors", "Tools", 45, 8));
            Products.Add(new Product("Glue", "Tools", 25, 30));

            // DEMO DATA so the charts are not empty. Delete this line to start with no sales.
            SeedDemoSales();
        }

        static void SeedDemoSales()
        {
            var rnd = new Random(7);
            for (int d = 6; d >= 0; d--)
            {
                DateTime day = DateTime.Today.AddDays(-d);
                int count = d == 0 ? rnd.Next(5, 9) : rnd.Next(9, 18);
                for (int i = 0; i < count; i++)
                {
                    DateTime t = day.AddHours(rnd.Next(8, 18)).AddMinutes(rnd.Next(0, 60));
                    if (t > DateTime.Now) t = DateTime.Now.AddMinutes(-rnd.Next(1, 40));

                    var s = new Sale { Time = t, Cashier = "Staff" };
                    int lines = rnd.Next(1, 5);
                    var picked = new HashSet<int>();
                    while (picked.Count < lines) picked.Add(rnd.Next(Products.Count));
                    foreach (int idx in picked)
                    {
                        var p = Products[idx];
                        s.Lines.Add(new SaleLine { P = p, Name = p.Name, Qty = rnd.Next(1, 5), Price = p.Price });
                    }
                    s.Total = s.Lines.Sum(l => l.Total);
                    s.Cash = Math.Ceiling(s.Total / 50) * 50;
                    s.Change = s.Cash - s.Total;
                    Sales.Add(s);
                }
            }
            Sales.Sort((a, b) => a.Time.CompareTo(b.Time));
            for (int i = 0; i < Sales.Count; i++) Sales[i].No = i + 1;
            nextNo = Sales.Count + 1;
        }

        public static IEnumerable<Sale> Valid { get { return Sales.Where(s => !s.Voided); } }

        public static string[] Categories()
        {
            return Products.Select(p => p.Category).Distinct().OrderBy(x => x).ToArray();
        }

        // ---------- Sales ----------
        public static Sale CompleteSale(List<SaleLine> cart, double cash, string cashier, out string error)
        {
            error = null;
            if (cart.Count == 0) { error = "The order is empty."; return null; }

            foreach (var g in cart.GroupBy(l => l.P))
            {
                int q = g.Sum(l => l.Qty);
                if (!Products.Contains(g.Key)) { error = g.Key.Name + " is no longer available."; return null; }
                if (q > g.Key.Stock) { error = "Not enough stock for " + g.Key.Name + " (only " + g.Key.Stock + " left)."; return null; }
            }

            double total = cart.Sum(l => l.Total);
            if (cash < total) { error = "Cash received is less than the total."; return null; }

            foreach (var l in cart) l.P.Stock -= l.Qty;

            var sale = new Sale
            {
                No = nextNo++,
                Time = DateTime.Now,
                Total = total,
                Cash = cash,
                Change = cash - total,
                Cashier = cashier,
                Lines = cart.Select(l => new SaleLine { P = l.P, Name = l.Name, Qty = l.Qty, Price = l.Price }).ToList()
            };
            Sales.Add(sale);
            return sale;
        }

        public static void Void(Sale s)
        {
            if (s.Voided) return;
            s.Voided = true;
            foreach (var l in s.Lines)
                if (Products.Contains(l.P)) l.P.Stock += l.Qty;
        }

        public static Sale LastValidSale()
        {
            return Sales.LastOrDefault(s => !s.Voided);
        }

        // ---------- Analytics ----------
        public static double RevenueOn(DateTime day)
        {
            return Valid.Where(s => s.Time.Date == day.Date).Sum(s => s.Total);
        }

        public static int OrdersOn(DateTime day)
        {
            return Valid.Count(s => s.Time.Date == day.Date);
        }

        public static double[] RevenueByDay(int days, out string[] labels)
        {
            var v = new double[days];
            labels = new string[days];
            for (int i = 0; i < days; i++)
            {
                DateTime d = DateTime.Today.AddDays(i - (days - 1));
                v[i] = RevenueOn(d);
                labels[i] = d.ToString("ddd");
            }
            return v;
        }

        static double[] HourBuckets(IEnumerable<Sale> sales, out string[] labels)
        {
            int n = LastHour - FirstHour + 1;
            var v = new double[n];
            labels = new string[n];
            for (int i = 0; i < n; i++)
            {
                int h = FirstHour + i;
                labels[i] = (h % 12 == 0 ? 12 : h % 12) + (h < 12 ? "AM" : "PM");
            }
            foreach (var s in sales)
            {
                int h = Math.Min(Math.Max(s.Time.Hour, FirstHour), LastHour);
                v[h - FirstHour] += s.Total;
            }
            return v;
        }

        public static double[] RevenueByHour(DateTime day, out string[] labels)
        {
            return HourBuckets(Valid.Where(s => s.Time.Date == day.Date), out labels);
        }

        public static double[] RevenueByHourAll(out string[] labels)
        {
            return HourBuckets(Valid, out labels);
        }

        public static List<ProductStat> TopProducts(int n)
        {
            return Valid.SelectMany(s => s.Lines)
                .GroupBy(l => l.P)
                .Select(g => new ProductStat { P = g.Key, Name = g.Key.Name, Units = g.Sum(l => l.Qty), Revenue = g.Sum(l => l.Total) })
                .OrderByDescending(x => x.Units)
                .Take(n)
                .ToList();
        }

        public static List<KeyValuePair<string, double>> CategoryRevenue()
        {
            return Valid.SelectMany(s => s.Lines)
                .GroupBy(l => l.P.Category)
                .Select(g => new KeyValuePair<string, double>(g.Key, g.Sum(l => l.Total)))
                .OrderByDescending(k => k.Value)
                .ToList();
        }
    }
}
