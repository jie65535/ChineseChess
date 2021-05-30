using System.Windows.Controls;

using ChineseChess.GUI.ViewModels;

namespace ChineseChess.GUI.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
