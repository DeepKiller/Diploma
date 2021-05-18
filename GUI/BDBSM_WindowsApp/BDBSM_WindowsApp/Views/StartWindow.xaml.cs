using BDB;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.Views
{
    /// <summary>
    /// Логика взаимодействия для StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonExpand_Click(object sender, RoutedEventArgs e)
        {
            MaximizeMinimizeWindow();
        }

        private void ButtonRollUp_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Обработчик события понели быстрого доступа при отпущенной левой кнопкой мыши
        /// на сворачивание/разворачивание окна.
        /// </summary>
        private void QuickAccessBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Released)
                return;

            var windowPosition = Mouse.GetPosition(this);
            var screenPosition = PointToScreen(windowPosition);

            if (screenPosition.Y == 0)
            { 
                MaximizeMinimizeWindow();
            }
        }

        /// <summary>
        /// Обработчик события понели быстрого доступа при нажатии левой кнопкой мыши
        /// на сворачивание окна и его перетаскиванию по клиентской области.
        /// </summary>
        private void QuickAccessBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (WindowState == WindowState.Maximized)
            {
                var mousePosition = Mouse.GetPosition(this);

                MaximizeMinimizeWindow();

                Top = mousePosition.Y;
                Left = mousePosition.X - mousePosition.X / 2;
            }

            DragMove();
        }

        /// <summary>
        /// Обработчик события при нажатии на кнопку создания новой БД.
        /// </summary>
        private void ButtonCreateNewDB_Click(object sender, RoutedEventArgs e)
        {
            // Создать объект диалогового окна.
            var createFileDialog = new CreateFileDialog();

            if (createFileDialog.ShowDialog() == false)
                return;

        }

        /// <summary>
        /// Обработчик события при нажатии на кнопку открыть БД.
        /// </summary>
        private void ButtonOpenDB_Click(object sender, RoutedEventArgs e)
        {
            // Инициализировать объект диалогового окна.
            OpenFileDialog openFileDialog = new OpenFileDialog();

            #region Настройки диалогового окна.

            openFileDialog.Title = "Путь для новой Базы данных";
            openFileDialog.Filter = "Biba Database|*.bdb";

            #endregion

            if (openFileDialog.ShowDialog() == false)
                return;

            DataBase.Path = openFileDialog.FileName;

            InfoDialog passwordDialog = new InfoDialog("Введите пароль","Ввод данных", "OK", "Отмена");

            if (passwordDialog.ShowDialog() == false)
                return;
               
            OpenFile(passwordDialog.Data, openFileDialog.SafeFileName);

            Close();
        }

        /// <summary>
        /// Свернуть/развернуть окно.
        /// </summary>
        private void MaximizeMinimizeWindow()
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

        /// <summary>
        /// Статический метод открытия базы данных.
        /// </summary>
        /// <param name="password">Параметр задает пароль для шифрования данных.</param>
        /// <param name="safeFileName">Параметр имени файла (файл.расширение).</param>
        public static void OpenFile(string password, string safeFileName)
        {
            DataBase.DeCryptData(password);

            DataBase.DecompresByGlobalPath();

            DataBase.DisassembleBaseFile();

            // Переменная, чтобы получить количество символов в пути к файлу.
            var countLetter = DataBase.Path.Length - (safeFileName.Length + 1);

            // Если countLetter <= 0, то испльзуется путь к документам
            // Иначе из полного имени файла удаляется его имя.
            string path = DataBase.Path.Remove(countLetter);

            new MainWindow(path).Show();
        }
    }
}
