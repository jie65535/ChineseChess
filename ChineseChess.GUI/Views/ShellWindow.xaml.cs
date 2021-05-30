using System.Windows.Controls;

using ChineseChess.GUI.Contracts.Views;
using ChineseChess.GUI.ViewModels;

using MahApps.Metro.Controls;

namespace ChineseChess.GUI.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow
    {
        public ShellWindow(ShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public Frame GetNavigationFrame()
            => shellFrame;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();
    }
}
