using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static BDB.Table;

namespace BDBSM_WindowsApp.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        public Table SelectedTable
        {
            get { return (Table)GetValue(SelectedTableProperty); }
            set { SetValue(SelectedTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTableProperty =
            DependencyProperty.Register("SelectedTable", typeof(Table), typeof(MainWindowViewModel), new PropertyMetadata(null, SelectedTable_Changed));



        public List<Row> Rows
        {
            get { return (List<Row>)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(List<Row>), typeof(MainWindowViewModel), new PropertyMetadata(null));

        private static void SelectedTable_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var current = d as MainWindowViewModel;
            
            if (current == null)
                return;

            if (current.SelectedTable == null)
                return;

            current.Rows = null;
            current.Rows = current.SelectedTable.Rows;
        }

        public ArrayList Cols
        {
            get { return (ArrayList)GetValue(ColsProperty); }
            set { SetValue(ColsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Cols.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColsProperty =
            DependencyProperty.Register("Cols", typeof(ArrayList), typeof(MainWindowViewModel), new PropertyMetadata(null));



        public ICollectionView Tables
        {
            get { return (ICollectionView)GetValue(TablesProperty); }
            set { SetValue(TablesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tables.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TablesProperty =
            DependencyProperty.Register("Tables", typeof(ICollectionView), typeof(MainWindowViewModel), new PropertyMetadata(null));


        #region FilterText
        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register("FilterText", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata("", FilterText_Changed));

        private static void FilterText_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var current = d as MainWindowViewModel;

            if (current == null)
                return;

            current.Tables.Filter = null;
            current.Tables.Filter = current.FilterTable;
        }
        private bool FilterTable(object obj)
        {
            Table current = obj as Table;

            if (!string.IsNullOrWhiteSpace(FilterText) && current != null && !current.Name.Contains(FilterText))
                return false;

            return true;
        }
        #endregion

        public string DatabaseName
        {
            get { return (string)GetValue(DatabaseNameProperty); }
            set { SetValue(DatabaseNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DatabaseName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DatabaseNameProperty =
            DependencyProperty.Register("DatabaseName", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(""));

        #region Команды

        #region SaveDatabaseCommand

        public ICommand SaveDatabaseCommand { get; }

        private bool CanSaveDatabaseCommand(object p) => true;
        private void OnSaveDatabaseCommandExecuted(object p) => DataBase.MakeBaseFile(DataBase.Path);

        #endregion

        #region CreateNewTableCommand

        public ICommand CreateNewTableCommand { get; }

        public bool CanCreateNewTableCommand(object p) => true;
        public void OnCreateNewTableCommandExecute(object p) 
        {
            var infoDialogViewModel = new InfoDialogViewModel();

            string tableName;

            if (infoDialogViewModel.ShowDialog(new InfoDialog(),"BDBCREATOR", "Введите имя таблицы") == false)
                return;

            tableName = infoDialogViewModel.InputText + ".bdbt";

            var table = new Table(SetOnlyPath() + tableName);

            table.SaveChanges();

            Tables = CollectionViewSource.GetDefaultView(DataBase.GetTables());
        }

        #endregion

        #region SetCurrentTableRowsCommand

        public ICommand SetCurrentTableRowsCommand { get; }

        public bool CanSetCurrentTableRows(object p) => true;
        public void OnSetCurrentTableRows(object p)
        {

        }

        #endregion

        #region OpenDatabaseCommand
        public ICommand OpenDatabaseCommand { get; }
        
        private void OnOpenDatabaseCommand(object p)
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

            if (infoDialogViewModel.ShowDialog(new InfoDialog(), "BDBSECYRITY", "Введите пароль базы данных") == false)
                return;

            #region Открытие файла.

            DataBase.DeCryptData(infoDialogViewModel.InputText);

            DataBase.DecompresByGlobalPath();

            DataBase.DisassembleBaseFile();
            #endregion
        }
        #endregion

        #region Close
        protected override void OnCloseWindowCommandExecuted(object p)
        {
            var infoDialogViewModel = new InfoDialogViewModel();
            bool? canClose = infoDialogViewModel.ShowDialog(new InfoDialog(),
                "BDBSAVER", "Сохранить базу данных?", Visibility.Hidden,
                "Сохранить и выйти", "Выйти", "Отмена", Visibility.Visible);

            if (canClose == false)
                return;

            if (infoDialogViewModel.IsFirstClick)
                OnSaveDatabaseCommandExecuted(p);

            base.OnCloseWindowCommandExecuted(p);
        }
        private void CloseWithCreateNew(object p)
        {
            var infoDialogViewModel = new InfoDialogViewModel();
            bool? canClose = infoDialogViewModel.ShowDialog(new InfoDialog(),
                "BDBSAVER", "Сохранить базу данных?", Visibility.Hidden,
                "Сохранить и выйти", "Выйти", "Отмена", Visibility.Visible);

            if (canClose == false)
                return;

            if (infoDialogViewModel.IsFirstClick)
                OnSaveDatabaseCommandExecuted(p);

            Show(new MainWindowViewModel(), new MainWindow());

            base.OnCloseWindowCommandExecuted(p);
        }
        #endregion

        #region SaveDatabaseHowCommand

        public ICommand SaveDatabaseHowCommand { get; }

        private bool CanSaveDatabaseHowCommand(object p) => true;
        private void OnSaveDatabaseHowCommandExecuted(object p)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            #region Настройки диалогового окна.

            saveFileDialog.Title = "Путь для новой Базы данных";
            saveFileDialog.Filter = "Biba Database|*.bdb";

            #endregion

            if (saveFileDialog.ShowDialog() == false)
                return;

            DataBase.Path = saveFileDialog.FileName;

            DataBase.MakeBaseFile(DataBase.Path);
        }
        #endregion

        #region CreateNewDatabaseCommand
        public ICommand CreateNewDatabaseCommand { get; }

        private bool CanCreateNewDatabaseCommand(object p) => true;
        private void OnCreateNewDatabaseCommandExecuted(object p)
        {
            var createFileDialog = new CreateFileDialogViewModel();

            if (ShowDialog(createFileDialog, new CreateFileDialog()) == false)
                return;

            CloseWithCreateNew(p);
        }
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            DatabaseName = SetDatabaseName();
            Tables = CollectionViewSource.GetDefaultView(DataBase.GetTables());
            Tables.Filter = FilterTable;

            SaveDatabaseCommand = new ActionCommand(OnSaveDatabaseCommandExecuted, CanSaveDatabaseCommand);
            OpenDatabaseCommand = new ActionCommand(OnOpenDatabaseCommand, null);
            CreateNewDatabaseCommand = new ActionCommand(OnCreateNewDatabaseCommandExecuted, CanCreateNewDatabaseCommand);
            
            CreateNewTableCommand = new ActionCommand(OnCreateNewTableCommandExecute, CanCreateNewTableCommand);

            SetCurrentTableRowsCommand = new ActionCommand(OnSetCurrentTableRows, CanSetCurrentTableRows);
        }

        private string SetDatabaseName()
        {
            var path = DataBase.Path;
            var countSymbolInName = 1;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] != '\\')
                    countSymbolInName++;
                else
                    break;
            }
            // Переменная, чтобы получить количество символов в пути к файлу.
            var countLetter = path.Length - countSymbolInName + 1;

            // Если countLetter <= 0, то испльзуется путь к документам
            // Иначе из полного имени файла удаляется его имя.
            return countLetter <= 0 ? path : path.Remove(0, DataBase.Path.Length - (DataBase.Path.Length - countLetter));
        }
        private string SetOnlyPath()
        {
            var path = DataBase.Path;
            var countSymbolInName = 1;
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] != '\\')
                    countSymbolInName++;
                else
                    break;
            }
            // Переменная, чтобы получить количество символов в пути к файлу.
            var countLetter = path.Length - countSymbolInName + 1;

            // Если countLetter <= 0, то испльзуется путь к документам
            // Иначе из полного имени файла удаляется его имя.
            return countLetter <= 0 ? path : path.Remove(countLetter);
        }
    }
}
