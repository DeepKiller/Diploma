using BDB;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.Views
{
    /// <summary>
    /// Логика взаимодействия для OpenOrCreateFileDialog.xaml
    /// </summary>
    public partial class CreateFileDialog : Window
    {
        /// <summary>
        /// Поле, содержащая путь стандартного сохранения (к документам).
        /// </summary>
        private readonly string DefaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        /// <summary>
        /// Получает или задает строку, содержащую только путь к файлу.
        /// </summary>
        public string Path 
        { 
            get => LabelPath.Content.ToString();
            private set => LabelPath.Content = value; 
        }

        /// <summary>
        /// Получает или задает строку, содержащую только имя файла.
        /// </summary>
        public string SafeFileName 
        { 
            get => TextBoxFileName.Text; 
            private set => TextBoxFileName.Text = value; 
        }

        public CreateFileDialog()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DialogWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ButtonChangePath_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            #region Настройки диалогового окна.

            dialog.Title = "Путь для новой Базы данных";
            dialog.Filter = "Biba Database|*.bdb";
            dialog.FileName = SafeFileName;
            dialog.InitialDirectory = Path;

            #endregion

            dialog.ShowDialog();

            // Переменная, чтобы получить количество символов в пути к файлу.
            var countLetter = dialog.FileName.Length - (SafeFileName.Length + 1);

            // Если countLetter <= 0, то испльзуется путь к документам
            // Иначе из полного имени файла удаляется его имя.
            Path = countLetter <= 0 ? DefaultPath : dialog.FileName.Remove(countLetter);

            SafeFileName = dialog.SafeFileName;
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            // Переменная полного пути для сохранения.
            var fileName = Path + "\\" + SafeFileName;
            var table = new BDB.Table();
            
            table.SaveChanges();

            DataBase.MakeBaseFile(fileName);

            DataBase.CompresByGlobalPath();

            InfoDialog passwordDialog = new InfoDialog("Введите пароль", "Ввод данных", "OK", "Отмена");

            if (passwordDialog.ShowDialog() == false)
                return;

            DataBase.CryptData(passwordDialog.Data);

            StartWindow.OpenFile(passwordDialog.Data, SafeFileName);

            DialogResult = true;
        }
    }
}
