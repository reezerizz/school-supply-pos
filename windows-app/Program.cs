using System;
using System.Windows.Forms;

namespace SchoolSupplyPOS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Store.Init();   // create the products and demo sales

            while (true)
            {
                frmRole roleForm = new frmRole();
                if (roleForm.ShowDialog() != DialogResult.OK)
                {
                    break;      // Exit was pressed
                }

                frmMain mainForm = new frmMain(roleForm.IsManager);
                mainForm.ShowDialog();

                if (!mainForm.LoggedOut)
                {
                    break;      // the window was closed with the X button
                }
                // Logout was pressed: loop back to the role screen
            }
        }
    }
}
