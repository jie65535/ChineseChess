using System.Windows.Controls;

using ChineseChess.GUI.ViewModels;

namespace ChineseChess.GUI.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
