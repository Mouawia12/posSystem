using Presentation.ViewModels;
using Wpf.Ui.Controls;

namespace Presentation.Views
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
