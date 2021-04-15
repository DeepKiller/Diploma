using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string a { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonExpand_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void ButtonRollUp_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void QuickAccessBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                var windowPosition = Mouse.GetPosition(this);
                var screenPosition = PointToScreen(windowPosition);

                if (screenPosition.Y == 0)
                    WindowState = WindowState.Maximized;
            }
        }

        private void QuickAccessBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    var mousePosition = Mouse.GetPosition(this);

                    WindowState = WindowState.Normal;
                    Top = mousePosition.Y;
                    Left = mousePosition.X - mousePosition.X / 2;
                }

                DragMove();
            }
        }
    }
}
