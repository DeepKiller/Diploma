using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.ViewModels
{
    class CreateFileDialogViewModel : BaseViewModel
    {
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", 
                typeof(string), 
                typeof(CreateFileDialogViewModel), 
                new PropertyMetadata(Environment.GetFolderPath(Environment.SpecialFolder.Personal)));

        public string SafeFileName
        {
            get { return (string)GetValue(SafeFileNameProperty); }
            set { SetValue(SafeFileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SafeFileNameProperty =
            DependencyProperty.Register("SafeFileName", typeof(string), typeof(CreateFileDialogViewModel), new PropertyMetadata("default.bdb"));

        #region Команды

        #region CreateInfoDialogCommand
        public ICommand CreateDatabaseCommand { get; }

        private void OnCreateDatabaseCommandExecuted(object p)
        {
            var fileName = Path + "\\" + SafeFileName;

            DataBase.MakeBaseFile(fileName);

            DataBase.CompresByGlobalPath();

            var infoDialogViewModel = new InfoDialogViewModel();

            if (infoDialogViewModel.ShowDialog(new InfoDialog()) == false)
                return;

            DialogResult = true;
            
            DataBase.CryptData(infoDialogViewModel.InputText);

            Close();
        }
        #endregion

        #region SelectPathCommand
        public ICommand SelectPathCommand { get; }

        private void OnSelectPathCommandExecuted(object p)
        {
            var dialog = new SaveFileDialog();

            #region Настройки диалогового окна.

            dialog.Title = "Путь для новой Базы данных";
            dialog.Filter = "Biba Database (*.bdb)|*.bdb|All files (*.*)|*.*";
            dialog.FileName = SafeFileName;
            dialog.InitialDirectory = Path;

            #endregion

            if(dialog.ShowDialog() == true)
            {
                Path = System.IO.Path.GetDirectoryName(dialog.FileName);
                SafeFileName = dialog.SafeFileName;
            }
        }
        #endregion

        #endregion

        public CreateFileDialogViewModel()
        {
            CreateDatabaseCommand = new ActionCommand(OnCreateDatabaseCommandExecuted, null);
            SelectPathCommand = new ActionCommand(OnSelectPathCommandExecuted, null);
        }

    }
}
