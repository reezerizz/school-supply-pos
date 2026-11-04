using System;
using System.Collections.Generic;

class Program
{
    // ===== SHARED DATA (in memory) =====
    static List<string> names = new List<string> { "Notebook", "Ballpen", "Pencil", "Eraser", "Ruler", "Scissors", "Glue", "Crayons", "Bond Paper (ream)", "Folder" };
    static List<double> prices = new List<double> { 35.00, 12.00, 8.00, 10.00, 15.00, 45.00, 25.00, 60.00, 280.00, 7.00 };
    static List<int> stocks = new List<int> { 50, 100, 80, 60, 40, 25, 30, 20, 15, 70 };

    const int LowStockLevel = 10;

    // ===== PURCHASE RECORDS (one entry per sale) =====
    static List<string> recNumbers = new List<string>();
    static List<string> recTimes = new List<string>();
    static List<double> recTotals = new List<double>();
    static List<bool> recVoided = new List<bool>();
    static List<string> recReceipts = new List<string>();

    static int transactionCount = 0;

    // Last sale (for void)
    static List<int> lastItems = new List<int>();
    static List<int> lastQtys = new List<int>();
    static double lastTotal = 0;

    // =====================================================
    static void Main()
    {
        int choice;

        do
        {
            Console.Clear();

            Console.WriteLine("=========================");
            Console.WriteLine("SCHOOL SUPPLIES POS");
            Console.WriteLine("POINT OF SALE SYSTEM");
            Console.WriteLine("=========================");
            Console.WriteLine();
            Console.WriteLine("SELECT YOUR ROLE");
            Console.WriteLine();
            Console.WriteLine("1. Manager");
            Console.WriteLine("2. Staff");
            Console.WriteLine("3. Exit");
            Console.WriteLine();
            Console.WriteLine("==========================");
            Console.Write("Enter your choice: ");

            choice = ReadInt();

            if (choice == 1)
            {
                ManagerMenu();
            }
            else if (choice == 2)
            {
                StaffMenu();
            }
            else if (choice == 3)
            {
                Console.WriteLine("\nThank you for using School Supplies POS!");
            }
            else
            {
                Console.WriteLine("\nInvalid choice! Please try again.");
                Console.ReadKey();
            }

        } while (choice != 3);
    }

    // ---------- Safe input helpers ----------
    static int ReadInt()
    {
        int value;
        if (int.TryParse(Console.ReadLine(), out value))
            return value;
        return -1;
    }

    static double ReadDouble()
    {
        double value;
        if (double.TryParse(Console.ReadLine(), out value))
            return value;
        return -1;
    }

    static bool Confirm(string message)
    {
        Console.Write(message + " (Y/N): ");
        return Console.ReadLine().Trim().ToUpper() == "Y";
    }

    // =====================================================
    //                    MANAGER MENU
    // =====================================================
    static void ManagerMenu()
    {
        int choice;

        do
        {
            Console.Clear();

            Console.WriteLine("================================");
            Console.WriteLine("          MANAGER MENU");
            Console.WriteLine("================================");
            Console.WriteLine("1. View Supplies");
            Console.WriteLine("2. Add Supplies");
            Console.WriteLine("3. Update Stock");
            Console.WriteLine("4. Change Prices");
            Console.WriteLine("5. View Purchase Records");
            Console.WriteLine("6. Remove Supply");
            Console.WriteLine("7. Sales Report");
            Console.WriteLine("8. Logout");
            Console.WriteLine("================================");
            Console.Write("Enter choice: ");

            choice = ReadInt();

            switch (choice)
            {
                case 1: ViewSupplies(); break;
                case 2: AddSupplies(); break;
                case 3: UpdateStock(); break;
                case 4: ChangePrices(); break;
                case 5: ViewPurchaseRecords(); break;
                case 6: RemoveSupply(); break;
                case 7: SalesReport(); break;
                case 8: Console.WriteLine("\nLogging out..."); break;
                default: Console.WriteLine("\nInvalid choice! Please try again."); break;
            }

            if (choice != 8)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }

        } while (choice != 8);
    }

    // Manager: Add Supplies
    static void AddSupplies()
    {
        Console.WriteLine("\n--- ADD SUPPLY ---");
        Console.Write("Item name: ");
        string name = Console.ReadLine().Trim();

        if (name == "")
        {
            Console.WriteLine("Name cannot be empty.");
            return;
        }

        for (int i = 0; i < names.Count; i++)
        {
            if (names[i].ToLower() == name.ToLower())
            {
                Console.WriteLine("That item already exists. Use Update Stock instead.");
                return;
            }
        }

        Console.Write("Price: P");
        double price = ReadDouble();
        if (price <= 0)
        {
            Console.WriteLine("Invalid price.");
            return;
        }

        Console.Write("Starting stock: ");
        int stock = ReadInt();
        if (stock < 0)
        {
            Console.WriteLine("Invalid stock.");
            return;
        }

        names.Add(name);
        prices.Add(price);
        stocks.Add(stock);
        Console.WriteLine("\n" + name + " added successfully.");
    }

    // Manager: Update Stock (restock or set exact count)
    static void UpdateStock()
    {
        ViewSupplies();
        Console.Write("\nEnter item number: ");
        int item = ReadInt();

        if (item < 1 || item > names.Count)
        {
            Console.WriteLine("Invalid item number.");
            return;
        }

        int idx = item - 1;
        Console.WriteLine("\n" + names[idx] + " - current stock: " + stocks[idx]);
        Console.WriteLine("1. Add stock (restock)");
        Console.WriteLine("2. Set exact stock count");
        Console.Write("Choose: ");
        int mode = ReadInt();

        if (mode == 1)
        {
            Console.Write("Quantity to add: ");
            int qty = ReadInt();
            if (qty <= 0)
            {
                Console.WriteLine("Quantity must be at least 1.");
                return;
            }
            stocks[idx] += qty;
        }
        else if (mode == 2)
        {
            Console.Write("New stock count: ");
            int qty = ReadInt();
            if (qty < 0)
            {
                Console.WriteLine("Invalid count.");
                return;
            }
            stocks[idx] = qty;
        }
        else
        {
            Console.WriteLine("Invalid choice.");
            return;
        }

        Console.WriteLine("Stock updated. " + names[idx] + " now has " + stocks[idx] + ".");
    }

    // Manager: Change Prices
    static void ChangePrices()
    {
        ViewSupplies();
        Console.Write("\nEnter item number: ");
        int item = ReadInt();

        if (item < 1 || item > names.Count)
        {
            Console.WriteLine("Invalid item number.");
            return;
        }

        int idx = item - 1;
        Console.WriteLine("\n" + names[idx] + " - current price: P" + prices[idx].ToString("F2"));
        Console.Write("New price: P");
        double newPrice = ReadDouble();

        if (newPrice <= 0)
        {
            Console.WriteLine("Invalid price.");
            return;
        }

        prices[idx] = newPrice;
        Console.WriteLine("Price updated. " + names[idx] + " is now P" + newPrice.ToString("F2") + ".");
    }

    // Manager: Remove Supply
    static void RemoveSupply()
    {
        ViewSupplies();
        Console.Write("\nEnter item number to remove: ");
        int item = ReadInt();

        if (item < 1 || item > names.Count)
        {
            Console.WriteLine("Invalid item number.");
            return;
        }

        int idx = item - 1;

        if (!Confirm("Remove " + names[idx] + "?"))
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        // The last sale refers to items by position, so void is no longer safe
        lastItems = new List<int>();
        lastQtys = new List<int>();
        lastTotal = 0;

        names.RemoveAt(idx);
        prices.RemoveAt(idx);
        stocks.RemoveAt(idx);
        Console.WriteLine("Item removed.");
    }

    // Manager: View Purchase Records
    static void ViewPurchaseRecords()
    {
        Console.WriteLine("\n===== PURCHASE RECORDS =====");

        if (recNumbers.Count == 0)
        {
            Console.WriteLine("No purchases yet.");
            return;
        }

        Console.WriteLine("{0,-8}{1,-18}{2,12}  {3}", "Receipt", "Date/Time", "Total", "Status");
        Console.WriteLine(new string('-', 50));

        for (int i = 0; i < recNumbers.Count; i++)
        {
            Console.WriteLine("{0,-8}{1,-18}{2,12:F2}  {3}",
                recNumbers[i], recTimes[i], recTotals[i], recVoided[i] ? "VOIDED" : "OK");
        }

        Console.Write("\nEnter receipt number to view details (or 0 to go back): ");
        int num = ReadInt();

        if (num <= 0) return;

        for (int i = 0; i < recNumbers.Count; i++)
        {
            if (recNumbers[i] == num.ToString("D4"))
            {
                Console.WriteLine();
                Console.WriteLine(recReceipts[i]);
                if (recVoided[i]) Console.WriteLine("*** THIS SALE WAS VOIDED ***");
                return;
            }
        }

        Console.WriteLine("Receipt not found.");
    }

    // Manager: Sales Report
    static void SalesReport()
    {
        double total = 0;
        int valid = 0, voided = 0;

        for (int i = 0; i < recTotals.Count; i++)
        {
            if (recVoided[i]) voided++;
            else
            {
                valid++;
                total += recTotals[i];
            }
        }

        Console.WriteLine("\n===== SALES REPORT =====");
        Console.WriteLine("Completed sales : " + valid);
        Console.WriteLine("Voided sales    : " + voided);
        Console.WriteLine("Total revenue   : P" + total.ToString("F2"));

        if (valid > 0)
            Console.WriteLine("Average sale    : P" + (total / valid).ToString("F2"));

        Console.WriteLine();
        CheckLowStock();
    }

    // =====================================================
    //                     STAFF MENU
    // =====================================================
    static void StaffMenu()
    {
        int choice;

        do
        {
            Console.Clear();

            Console.WriteLine("================================");
            Console.WriteLine("           STAFF MENU");
            Console.WriteLine("================================");
            Console.WriteLine("1. View Supplies");
            Console.WriteLine("2. Search Supply");
            Console.WriteLine("3. Process Sale");
            Console.WriteLine("4. Check Low Stock");
            Console.WriteLine("5. View Today's Sales");
            Console.WriteLine("6. Reprint Last Receipt");
            Console.WriteLine("7. Void Last Sale");
            Console.WriteLine("8. Logout");
            Console.WriteLine("================================");
            Console.Write("Enter choice: ");

            choice = ReadInt();

            switch (choice)
            {
                case 1: ViewSupplies(); break;
                case 2: SearchSupply(); break;
                case 3: ProcessSale(); break;
                case 4: CheckLowStock(); break;
                case 5: ViewTodaysSales(); break;
                case 6: ReprintReceipt(); break;
                case 7: VoidLastSale(); break;
                case 8: Console.WriteLine("\nLogging out"); break;
                default: Console.WriteLine("\nInvalid choice! Please try again."); break;
            }

            if (choice != 8)
            {
                Console.WriteLine("\nPress any key to continue");
                Console.ReadKey();
            }

        } while (choice != 8);
    }

    // =====================================================
    //                  SHARED FEATURES
    // =====================================================
    static void ViewSupplies()
    {
        Console.WriteLine();
        Console.WriteLine("{0,-4}{1,-20}{2,10}{3,8}", "No.", "Item", "Price", "Stock");
        Console.WriteLine(new string('-', 42));

        if (names.Count == 0)
        {
            Console.WriteLine("No supplies in inventory.");
            return;
        }

        for (int i = 0; i < names.Count; i++)
        {
            Console.WriteLine("{0,-4}{1,-20}{2,10:F2}{3,8}", i + 1, names[i], prices[i], stocks[i]);
        }
    }

    static void SearchSupply()
    {
        Console.Write("\nEnter item name to search: ");
        string keyword = Console.ReadLine().Trim().ToLower();
        bool found = false;

        if (keyword == "")
        {
            Console.WriteLine("Please type a name.");
            return;
        }

        Console.WriteLine();
        for (int i = 0; i < names.Count; i++)
        {
            if (names[i].ToLower().Contains(keyword))
            {
                string status = stocks[i] == 0 ? "OUT OF STOCK" : stocks[i] + " in stock";
                Console.WriteLine("{0}. {1} - P{2:F2} ({3})", i + 1, names[i], prices[i], status);
                found = true;
            }
        }

        if (!found)
            Console.WriteLine("No item matched \"" + keyword + "\".");
    }

    static void ProcessSale()
    {
        List<int> cartItems = new List<int>();
        List<int> cartQtys = new List<int>();
        string again;

        do
        {
            ViewSupplies();
            Console.Write("\nEnter item number: ");
            int item = ReadInt();

            if (item < 1 || item > names.Count)
            {
                Console.WriteLine("Invalid item number.");
            }
            else
            {
                int idx = item - 1;

                int inCart = 0;
                for (int c = 0; c < cartItems.Count; c++)
                    if (cartItems[c] == idx) inCart += cartQtys[c];

                int available = stocks[idx] - inCart;

                Console.Write("Quantity for " + names[idx] + ": ");
                int qty = ReadInt();

                if (qty <= 0)
                    Console.WriteLine("Quantity must be at least 1.");
                else if (qty > available)
                    Console.WriteLine("Not enough stock. Only " + available + " left.");
                else
                {
                    cartItems.Add(idx);
                    cartQtys.Add(qty);
                    Console.WriteLine("Added " + qty + " x " + names[idx]);
                }
            }

            Console.Write("\nAdd another item? (Y/N): ");
            again = Console.ReadLine().Trim().ToUpper();

        } while (again == "Y");

        if (cartItems.Count == 0)
        {
            Console.WriteLine("\nCart is empty. Sale cancelled.");
            return;
        }

        double total = 0;
        for (int i = 0; i < cartItems.Count; i++)
            total += prices[cartItems[i]] * cartQtys[i];

        Console.WriteLine("\nTOTAL DUE: P" + total.ToString("F2"));

        double payment;
        while (true)
        {
            Console.Write("Cash received (or 0 to cancel): ");
            payment = ReadDouble();

            if (payment == 0)
            {
                Console.WriteLine("Sale cancelled.");
                return;
            }
            if (payment >= total) break;

            Console.WriteLine("Insufficient payment.");
        }

        double change = payment - total;

        for (int i = 0; i < cartItems.Count; i++)
            stocks[cartItems[i]] -= cartQtys[i];

        transactionCount++;
        string number = transactionCount.ToString("D4");
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        string r = "";
        r += "================================\n";
        r += "      SCHOOL SUPPLIES POS\n";
        r += "      Receipt #" + number + "\n";
        r += "      " + time + "\n";
        r += "================================\n";
        for (int i = 0; i < cartItems.Count; i++)
        {
            double line = prices[cartItems[i]] * cartQtys[i];
            r += string.Format("{0,-16}{1,3} x {2,6:F2} = {3,8:F2}\n",
                names[cartItems[i]], cartQtys[i], prices[cartItems[i]], line);
        }
        r += "--------------------------------\n";
        r += string.Format("TOTAL:   P{0:F2}\n", total);
        r += string.Format("CASH:    P{0:F2}\n", payment);
        r += string.Format("CHANGE:  P{0:F2}\n", change);
        r += "================================\n";
        r += "   Thank you! Come again!\n";

        recNumbers.Add(number);
        recTimes.Add(time);
        recTotals.Add(total);
        recVoided.Add(false);
        recReceipts.Add(r);

        lastItems = cartItems;
        lastQtys = cartQtys;
        lastTotal = total;

        Console.WriteLine();
        Console.WriteLine(r);
    }

    static void CheckLowStock()
    {
        bool any = false;
        Console.WriteLine("Items with " + LowStockLevel + " or fewer in stock:\n");

        for (int i = 0; i < names.Count; i++)
        {
            if (stocks[i] <= LowStockLevel)
            {
                string tag = stocks[i] == 0 ? "OUT OF STOCK" : "LOW";
                Console.WriteLine("{0,-20}{1,4} left  [{2}]", names[i], stocks[i], tag);
                any = true;
            }
        }

        if (!any)
            Console.WriteLine("All items are well stocked.");
    }

    static void ViewTodaysSales()
    {
        Console.WriteLine("\n===== TODAY'S SALES =====");

        double total = 0;
        int count = 0;
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        for (int i = 0; i < recNumbers.Count; i++)
        {
            if (recVoided[i] || !recTimes[i].StartsWith(today)) continue;

            Console.WriteLine("#" + recNumbers[i] + "  " + recTimes[i].Substring(11) + "  P" + recTotals[i].ToString("F2"));
            total += recTotals[i];
            count++;
        }

        if (count == 0)
        {
            Console.WriteLine("No sales yet.");
            return;
        }

        Console.WriteLine("-------------------------");
        Console.WriteLine("Transactions: " + count);
        Console.WriteLine("Total sales:  P" + total.ToString("F2"));
    }

    static void ReprintReceipt()
    {
        // reprint the latest sale that has not been voided
        for (int i = recReceipts.Count - 1; i >= 0; i--)
        {
            if (!recVoided[i])
            {
                Console.WriteLine();
                Console.WriteLine("*** DUPLICATE COPY ***");
                Console.WriteLine(recReceipts[i]);
                return;
            }
        }

        Console.WriteLine("\nNo receipt to reprint.");
    }

    static void VoidLastSale()
    {
        if (lastItems.Count == 0 || recNumbers.Count == 0 || recVoided[recVoided.Count - 1])
        {
            Console.WriteLine("\nNo sale to void.");
            return;
        }

        Console.WriteLine();
        if (!Confirm("Void the last sale (P" + lastTotal.ToString("F2") + ")?"))
        {
            Console.WriteLine("Void cancelled.");
            return;
        }

        for (int i = 0; i < lastItems.Count; i++)
            stocks[lastItems[i]] += lastQtys[i];

        recVoided[recVoided.Count - 1] = true;

        lastItems = new List<int>();
        lastQtys = new List<int>();
        lastTotal = 0;

        Console.WriteLine("Last sale voided. Stock restored.");
    }
}
