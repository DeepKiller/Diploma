using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.ViewModels
{
    class StartWindowViewModel : BaseViewModel
    {
        public string ProductVersion
        {
            get { return (string)GetValue(ProductVersionProperty); }
            set { SetValue(ProductVersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for productVersion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProductVersionProperty =
            DependencyProperty.Register("ProductVersion", typeof(string), typeof(StartWindowViewModel), new PropertyMetadata("v" + Assembly.GetExecutingAssembly().GetName().Version.ToString()));

        #region Команды

        #region CloseWindowCommand

        protected override bool CanCloseWindowCommand(object p) => true;
        protected override void OnCloseWindowCommandExecuted(object p)
        {
            Application.Current.MainWindow.Close();
        }

        #endregion

        #region OpenCreateFileDialogCommand

        public ICommand OpenCreateFileDialogCommand { get; }

        private bool CanOpenCreateFileDialogCommand(object p) => true;

        private void OnOpenCreateFileDialogCommandExecuted(object p)
        {
            var createFileDialog = new CreateFileDialogViewModel();

            if (ShowDialog(createFileDialog, new CreateFileDialog()) == false)
                return;

            Show(new MainWindowViewModel(), new MainWindow());

            OnCloseWindowCommandExecuted(p);
        }

        #endregion

        #region OpenDatabaseCommand

        public ICommand OpenDatabaseCommand { get; }

        private bool CanOpenDatabaseCommand(object p) => true;

        private void OnOpenDatabaseCommandExecuted(object p)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            #region Настройки диалогового окна.

            openFileDialog.Title = "Путь для новой Базы данных";
            openFileDialog.Filter = "Biba Database|*.bdb";

            #endregion

            if (openFileDialog.ShowDialog() == false)
                return;

            DataBase.Path = openFileDialog.FileName;


            var infoDialogViewModel = new InfoDialogViewModel();
            try
            {
                if (infoDialogViewModel.ShowDialog( "BDB SECYRITY", "Введите пароль базы данных") == false)
                    return;

                #region Открытие файла.
                DataBase.DeCryptData(infoDialogViewModel.InputText);

                DataBase.DecompresByGlobalPath();

                DataBase.DisassembleBaseFile();
                #endregion

                Show(new MainWindowViewModel(), new MainWindow());

                OnCloseWindowCommandExecuted(p);
            }
            catch (DataBase.IncorrectPasswordException)
            {
                infoDialogViewModel.ShowDialog( "BDB ERROR", "Неправильный пароль", Visibility.Hidden);
            }
        }

        #endregion

        #region RollUpWindowCommand
        protected override void OnRollUpWindowCommandExecuted(object p)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        // Переопределенный метод ExpandWindow;
        protected override void OnExpandWindowCommandExecuted(object p)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Normal)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        } 
        #endregion

        #region DragMoveCommand
        // Переопределенный метод DragMove;
        protected override void OnDragMoveCommandExecuted(object p)
        {
            Application.Current.MainWindow.DragMove();
        } 
        #endregion

        #endregion

        public StartWindowViewModel()
        {
            OpenCreateFileDialogCommand = new ActionCommand(OnOpenCreateFileDialogCommandExecuted, CanOpenCreateFileDialogCommand);
            OpenDatabaseCommand = new ActionCommand(OnOpenDatabaseCommandExecuted, CanOpenDatabaseCommand);
        }
    }
}
