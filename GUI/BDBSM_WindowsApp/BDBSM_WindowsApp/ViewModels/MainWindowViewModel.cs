using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static BDB.Table;

namespace BDBSM_WindowsApp.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        public string DatabaseName
        {
            get { return (string)GetValue(DatabaseNameProperty); }
            set { SetValue(DatabaseNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DatabaseName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DatabaseNameProperty =
            DependencyProperty.Register("DatabaseName", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(""));



        public Table SelectedTable
        {
            get { return (Table)GetValue(SelectedTableProperty); }
            set { SetValue(SelectedTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTableProperty =
            DependencyProperty.Register("SelectedTable", typeof(Table), typeof(MainWindowViewModel), new PropertyMetadata(null, SelectedTable_Changed));
        

        private static void SelectedTable_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is MainWindowViewModel current))
                return;

            if (current.SelectedTable == null)
                return;

            for (int i = 1; i < current.SelectedTable.Rows.Count; i++)
            {
                current.ListOfDynamicObject.Add(CreateDynamic(current.SelectedTable.Rows[0].Cols, current.SelectedTable.Rows[i].Cols));
            }

            current.ObservableCollection = new ObservableCollection<ExpandoObject>(current.ListOfDynamicObject);
        }

        private static ExpandoObject CreateDynamic(List<Row.Column> propertyName, List<Row.Column> propertyValue)
        {
            dynamic obj = new ExpandoObject();

            for (int i = 0; i < propertyName.Count; i++)
            {
                ((IDictionary<string, object>)obj)[propertyName[i].Data] = propertyValue[i].Data;
            }

            return obj;
        }
        public ObservableCollection<ExpandoObject> ObservableCollection
        {
            get { return (ObservableCollection<ExpandoObject>)GetValue(ObservableCollectionProperty); }
            set { SetValue(ObservableCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObservableCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObservableCollectionProperty =
            DependencyProperty.Register("ObservableCollection", typeof(ObservableCollection<ExpandoObject>), typeof(MainWindowViewModel), new PropertyMetadata(new ObservableCollection<ExpandoObject>()));




        public List<ExpandoObject> ListOfDynamicObject
        {
            get { return (List<ExpandoObject>)GetValue(ListOfDynamicObjectProperty); }
            set { SetValue(ListOfDynamicObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ListOfDynamicObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ListOfDynamicObjectProperty =
            DependencyProperty.Register("ListOfDynamicObject", typeof(List<ExpandoObject>), typeof(MainWindowViewModel), new PropertyMetadata(new List<ExpandoObject>()));


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
            if (!(d is MainWindowViewModel current))
                return;

            current.Tables.Filter = null;
            current.Tables.Filter = current.FilterTable;
        }
        private bool FilterTable(object obj)
        {
            if (!string.IsNullOrWhiteSpace(FilterText) && obj is Table current && !current.Name.Contains(FilterText))
                return false;

            return true;
        }
        #endregion



        #region Команды

        #region SaveDatabaseCommand

        public ICommand SaveDatabaseCommand { get; }

        private bool CanSaveDatabaseCommand(object p) => true;
        private void OnSaveDatabaseCommandExecuted(object p)
        {
            DataBase.MakeBaseFile(DataBase.Path);
            DataBase.CompresByGlobalPath();
            DataBase.CryptData();

            var infoDialogViewModel = new InfoDialogViewModel();

            infoDialogViewModel.ShowDialog(new InfoDialog(), "BDB NOTIFICATION", "База данных успешна сохранена!", Visibility.Hidden);
        }

        #endregion

        #region SaveDatabaseHowCommand

        public ICommand SaveDatabaseHowCommand { get; }

        private bool CanSaveDatabaseHowCommand(object p) => true;
        private void OnSaveDatabaseHowCommandExecuted(object p)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            #region Настройки диалогового окна.

            saveFileDialog.Title = "Путь для новой Базы данных";
            saveFileDialog.Filter = "Biba Database|*.bdb";

            #endregion

            if (saveFileDialog.ShowDialog() == false)
                return;
            
            var tables = DataBase.GetTables();
            DataBase.Path = saveFileDialog.FileName;

            foreach (var table in tables)
            {
                table.DeleteTable();
                table.Path = SetOnlyPath(DataBase.Path) + table.Name + ".bdbt";
                table.SaveChanges();
            }

            #region Сохранение.
            DataBase.MakeBaseFile(DataBase.Path);
            DataBase.CompresByGlobalPath();
            DataBase.CryptData();
            #endregion

            DatabaseName = SetDatabaseName();

            #region Открытие.
            DataBase.DeCryptData("1111");
            DataBase.DecompresByGlobalPath();
            DataBase.DisassembleBaseFile();
            #endregion

            var infoDialogViewModel = new InfoDialogViewModel();

            infoDialogViewModel.ShowDialog(new InfoDialog(), "BDB NOTIFICATION", "База данных успешна сохранена в новом пути!", Visibility.Hidden);
        }
        #endregion

        #region OpenDatabaseCommand
        public ICommand OpenDatabaseCommand { get; }

        private void OnOpenDatabaseCommand(object p)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

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

            var table = new Table(SetOnlyPath(DataBase.Path) + tableName);
            string[] Cols = { "one", "two", "id" };
            string[] Data = { "1", "2" };
            string[] st = { "4", "6" };
            table.SetColNames(Cols);
            table.AddRow(Data);
            table.AddRow(st);
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

        #region SaveTablesCommand
        public ICommand SaveTablesCommand { get; }
        private void OnSaveTablesCommandExecuted(object p)
        {
            var tables = DataBase.GetTables();

            foreach (var table in tables)
            {
                table.SaveChanges();
            }
        }

        private bool CanSaveTablesCommand(object p) => true;
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

        #region CreateRelationCommand
        public ICommand CreateRelationCommand { get; }

        private void OnCreateRelationCommandExecuted(object p)
        {
            Show(new CreateRelationViewModel(Tables), new CreateRelation());
        }
        private bool CanCreateRelationCommand(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            DatabaseName = SetDatabaseName();
            Tables = CollectionViewSource.GetDefaultView(DataBase.GetTables());
            Tables.Filter = FilterTable;

            SaveDatabaseCommand = new ActionCommand(OnSaveDatabaseCommandExecuted, CanSaveDatabaseCommand);
            SaveDatabaseHowCommand = new ActionCommand(OnSaveDatabaseHowCommandExecuted, CanSaveDatabaseHowCommand);
            OpenDatabaseCommand = new ActionCommand(OnOpenDatabaseCommand, null);
            CreateNewDatabaseCommand = new ActionCommand(OnCreateNewDatabaseCommandExecuted, CanCreateNewDatabaseCommand);
            
            CreateNewTableCommand = new ActionCommand(OnCreateNewTableCommandExecute, CanCreateNewTableCommand);
            SaveTablesCommand = new ActionCommand(OnSaveTablesCommandExecuted, CanSaveTablesCommand);
            SetCurrentTableRowsCommand = new ActionCommand(OnSetCurrentTableRows, CanSetCurrentTableRows);

            CreateRelationCommand = new ActionCommand(OnCreateRelationCommandExecuted, CanCreateRelationCommand);
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
        private string SetOnlyPath(string path)
        {
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
