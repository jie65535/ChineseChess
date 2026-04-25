using System;
using System.Windows.Forms;

namespace ChineseChess
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--ai-bench")
            {
                AIBench.Run();
                return;
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

