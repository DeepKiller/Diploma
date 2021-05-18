using BDB;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Table> _tables = new List<Table>();

        public MainWindow(string path)
        {
            InitializeComponent();

            foreach (string file in Directory.GetFiles(path, "*.bdbt"))
            {
                Table tab = new Table();
                tab.LoadTableData(file);
                _tables.Add(tab);
            }
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
                ButtonExpand.Content = "❐";
            }
            else
            {
                WindowState = WindowState.Normal;
                ButtonExpand.Content = "▢";
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

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Задает стандартное расширение файлу.
            saveFileDialog.DefaultExt = "*.bdb";

            // В окне сохранения задает тип файла.
            saveFileDialog.Filter = "Базы данных Biba Database |*.bdb";

            if (saveFileDialog.ShowDialog() == true)
            {
                DataBase.MakeBaseFile(saveFileDialog.FileName);
            }
        }

        private void ButtonCreateTable_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
