using System;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Store.Init();

            while (true)
            {
                bool manager;
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;   // Exit
                    manager = login.IsManager;
                }

                using (var main = new MainForm(manager))
                {
                    main.ShowDialog();
                    if (!main.LoggedOut) break;                          // window closed with X
                }
            }
        }
    }
}
