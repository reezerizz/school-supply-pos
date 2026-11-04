using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolSupplyPOS
{
    // ===================== CLASSES (data) =====================
    public class Product
    {
        public string Name;
        public string Category;
        public double Price;
        public int Stock;

        public Product(string name, string category, double price, int stock)
        {
            Name = name;
            Category = category;
            Price = price;
            Stock = stock;
        }
    }

    // One row of a sale (an item and how many were bought)
    public class SaleLine
    {
        public Product Item;
        public string Name;     // copied at the time of sale
        public int Qty;
        public double Price;    // copied at the time of sale

        public double GetTotal()
        {
            return Qty * Price;
        }
    }

    // One transaction
    public class Sale
    {
        public int No;
        public DateTime Time;
        public List<SaleLine> Lines = new List<SaleLine>();
        public double Total;
        public double Cash;
        public double Change;
        public bool Voided;
        public string Cashier;

        public int ItemCount()
        {
            int n = 0;
            foreach (SaleLine line in Lines)
            {
                n += line.Qty;
            }
            return n;
        }

        public string Receipt()
        {
            string nl = Environment.NewLine;
            StringBuilder sb = new StringBuilder();
            string bar = new string('=', 36);

            sb.Append(bar + nl);
            sb.Append("        SCHOOL SUPPLIES POS" + nl);
            sb.Append("        Receipt #" + No.ToString("D4") + nl);
            sb.Append("        " + Time.ToString("yyyy-MM-dd HH:mm") + nl);
            sb.Append(bar + nl);

            foreach (SaleLine line in Lines)
            {
                string n = line.Name;
                if (n.Length > 14) n = n.Substring(0, 14);
                sb.Append(string.Format("{0,-14}{1,3} x {2,7:F2} {3,8:F2}", n, line.Qty, line.Price, line.GetTotal()) + nl);
            }

            sb.Append(new string('-', 36) + nl);
            sb.Append(string.Format("TOTAL:   P{0:F2}", Total) + nl);
            sb.Append(string.Format("CASH:    P{0:F2}", Cash) + nl);
            sb.Append(string.Format("CHANGE:  P{0:F2}", Change) + nl);
            sb.Append("Cashier: " + Cashier + nl);
            sb.Append(bar + nl);
            if (Voided) sb.Append("        *** VOIDED ***" + nl);
            else sb.Append("     Thank you! Come again!" + nl);
            return sb.ToString();
        }
    }

    // Used for the Top Selling Products table
    public class ProductStat
    {
        public Product Item;
        public int Units;
        public double Revenue;
    }

    // ===================== STORE (all data and rules) =====================
    public static class Store
    {
        public static List<Product> Products = new List<Product>();
        public static List<Sale> Sales = new List<Sale>();
        public const int LowStockLevel = 10;

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

            // DEMO DATA so the dashboard is not empty.
            // Delete the next line to start with no sales.
            SeedDemoSales();
        }

        static void SeedDemoSales()
        {
            Random rnd = new Random(7);

            for (int d = 6; d >= 0; d--)
            {
                int count;
                if (d == 0) count = rnd.Next(5, 9);
                else count = rnd.Next(9, 18);

                for (int i = 0; i < count; i++)
                {
                    DateTime t = DateTime.Today.AddDays(-d).AddHours(rnd.Next(8, 18)).AddMinutes(rnd.Next(0, 60));
                    if (t > DateTime.Now) t = DateTime.Now.AddMinutes(-rnd.Next(1, 40));

                    Sale s = new Sale();
                    s.Time = t;
                    s.Cashier = "Staff";

                    int howMany = rnd.Next(1, 5);
                    List<int> picked = new List<int>();
                    while (picked.Count < howMany)
                    {
                        int idx = rnd.Next(Products.Count);
                        if (!picked.Contains(idx)) picked.Add(idx);
                    }

                    foreach (int idx in picked)
                    {
                        Product p = Products[idx];
                        SaleLine line = new SaleLine();
                        line.Item = p;
                        line.Name = p.Name;
                        line.Qty = rnd.Next(1, 5);
                        line.Price = p.Price;
                        s.Lines.Add(line);
                        s.Total += line.GetTotal();
                    }

                    s.Cash = Math.Ceiling(s.Total / 50) * 50;
                    s.Change = s.Cash - s.Total;
                    Sales.Add(s);
                }
            }

            Sales.Sort(CompareByTime);
            for (int i = 0; i < Sales.Count; i++)
            {
                Sales[i].No = i + 1;
            }
            nextNo = Sales.Count + 1;
        }

        static int CompareByTime(Sale a, Sale b)
        {
            return a.Time.CompareTo(b.Time);
        }

        static int CompareByUnits(ProductStat a, ProductStat b)
        {
            return b.Units.CompareTo(a.Units);   // biggest first
        }

        // ---------- selling ----------
        // Returns the new Sale, or null with a message in "error".
        // All checks come first; stock is only changed at the end.
        public static Sale CompleteSale(List<SaleLine> cart, double cash, string cashier, out string error)
        {
            error = "";

            if (cart.Count == 0)
            {
                error = "The order is empty.";
                return null;
            }

            foreach (SaleLine line in cart)
            {
                if (!Products.Contains(line.Item))
                {
                    error = line.Name + " is no longer available.";
                    return null;
                }

                int wanted = 0;
                foreach (SaleLine other in cart)
                {
                    if (other.Item == line.Item) wanted += other.Qty;
                }
                if (wanted > line.Item.Stock)
                {
                    error = "Not enough stock for " + line.Name + " (only " + line.Item.Stock + " left).";
                    return null;
                }
            }

            double total = 0;
            foreach (SaleLine line in cart)
            {
                total += line.GetTotal();
            }

            if (cash < total)
            {
                error = "Cash received is less than the total.";
                return null;
            }

            Sale sale = new Sale();
            sale.No = nextNo;
            nextNo++;
            sale.Time = DateTime.Now;
            sale.Total = total;
            sale.Cash = cash;
            sale.Change = cash - total;
            sale.Cashier = cashier;

            foreach (SaleLine line in cart)
            {
                line.Item.Stock -= line.Qty;

                SaleLine copy = new SaleLine();
                copy.Item = line.Item;
                copy.Name = line.Name;
                copy.Qty = line.Qty;
                copy.Price = line.Price;
                sale.Lines.Add(copy);
            }

            Sales.Add(sale);
            return sale;
        }

        // Cancels a sale: marks it voided and puts the stock back
        public static void VoidSale(Sale s)
        {
            if (s.Voided) return;

            s.Voided = true;
            foreach (SaleLine line in s.Lines)
            {
                if (Products.Contains(line.Item)) line.Item.Stock += line.Qty;
            }
        }

        public static Sale LastValidSale()
        {
            for (int i = Sales.Count - 1; i >= 0; i--)
            {
                if (!Sales[i].Voided) return Sales[i];
            }
            return null;
        }

        // ---------- products ----------
        public static List<string> GetCategories()
        {
            List<string> list = new List<string>();
            foreach (Product p in Products)
            {
                if (!list.Contains(p.Category)) list.Add(p.Category);
            }
            list.Sort();
            return list;
        }

        public static int CountLowStock()
        {
            int n = 0;
            foreach (Product p in Products)
            {
                if (p.Stock > 0 && p.Stock <= LowStockLevel) n++;
            }
            return n;
        }

        public static int CountOutOfStock()
        {
            int n = 0;
            foreach (Product p in Products)
            {
                if (p.Stock == 0) n++;
            }
            return n;
        }

        // ---------- analytics ----------
        public static double RevenueOn(DateTime day)
        {
            double sum = 0;
            foreach (Sale s in Sales)
            {
                if (!s.Voided && s.Time.Date == day.Date) sum += s.Total;
            }
            return sum;
        }

        public static int OrdersOn(DateTime day)
        {
            int n = 0;
            foreach (Sale s in Sales)
            {
                if (!s.Voided && s.Time.Date == day.Date) n++;
            }
            return n;
        }

        // Revenue for each of the last "days" days (oldest first)
        public static double[] RevenueLastDays(int days, out string[] labels)
        {
            double[] values = new double[days];
            labels = new string[days];

            for (int i = 0; i < days; i++)
            {
                DateTime d = DateTime.Today.AddDays(i - (days - 1));
                values[i] = RevenueOn(d);
                labels[i] = d.ToString("ddd");
            }
            return values;
        }

        // Revenue per hour of the day (8 AM to 8 PM), all valid sales
        public static double[] RevenueByHour(out string[] labels)
        {
            double[] values = new double[13];
            labels = new string[13];

            for (int i = 0; i < 13; i++)
            {
                int h = 8 + i;
                int shown = h % 12;
                if (shown == 0) shown = 12;
                labels[i] = shown + (h < 12 ? " AM" : " PM");
            }

            foreach (Sale s in Sales)
            {
                if (s.Voided) continue;
                int h = s.Time.Hour;
                if (h < 8) h = 8;
                if (h > 20) h = 20;
                values[h - 8] += s.Total;
            }
            return values;
        }

        public static List<ProductStat> TopProducts(int n)
        {
            List<ProductStat> stats = new List<ProductStat>();

            foreach (Sale s in Sales)
            {
                if (s.Voided) continue;

                foreach (SaleLine line in s.Lines)
                {
                    ProductStat found = null;
                    foreach (ProductStat st in stats)
                    {
                        if (st.Item == line.Item) found = st;
                    }
                    if (found == null)
                    {
                        found = new ProductStat();
                        found.Item = line.Item;
                        stats.Add(found);
                    }
                    found.Units += line.Qty;
                    found.Revenue += line.GetTotal();
                }
            }

            stats.Sort(CompareByUnits);
            if (stats.Count > n) stats.RemoveRange(n, stats.Count - n);
            return stats;
        }

        public static Dictionary<string, double> RevenueByCategory()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            foreach (Sale s in Sales)
            {
                if (s.Voided) continue;

                foreach (SaleLine line in s.Lines)
                {
                    string cat = line.Item.Category;
                    if (!result.ContainsKey(cat)) result[cat] = 0;
                    result[cat] += line.GetTotal();
                }
            }
            return result;
        }
    }
}
