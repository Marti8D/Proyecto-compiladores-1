using System;
using System.Windows.Forms;
using ProyectoCompiladores1.UI;

namespace ProyectoCompiladores1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormPrincipal());
        }
    }
}
