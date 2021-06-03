using BDBSM_WindowsApp.ViewModels;
using System.Windows;

namespace BDBSM_WindowsApp.Views
{
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
        }
    }
}
