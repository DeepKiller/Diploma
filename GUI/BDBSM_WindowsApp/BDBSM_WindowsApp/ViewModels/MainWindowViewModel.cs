using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        #region SelectedTable
        private Table _previousTable;
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

            current.SavePreviousTable();

            current._previousTable = current.SelectedTable;

            var data = new DataTable();
            var id = new string[] { current.SelectedTable.Name };

            if (current.SelectedTable.Rows.Count == 0)
                current.SelectedTable.SetColNames(id);

            foreach (var column in current.SelectedTable.Rows[0].Cols)
                data.Columns.Add(column.Data);

            for (int i = 1; i < current.SelectedTable.Rows.Count; i++)
                data.Rows.Add(GetArrayFromRow(current.SelectedTable.Rows[i]));

            current.DataTable = data.AsDataView();
        }
        private void SavePreviousTable()
        {
            if (_previousTable == null)
                return;

            _previousTable.Rows.Clear();
            _previousTable.Ids.Clear();
            var rows = new string[DataTable.Table.Columns.Count];

            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = DataTable.Table.Columns[i].ColumnName;
            }

            _previousTable.Rows.Add(new Row(rows));

            foreach (DataRow dataRow in DataTable.Table.Rows)
            {
                rows = new string[DataTable.Table.Columns.Count - 1];

                for (int i = 0; i < rows.Length; i++)
                {
                    rows[i] = dataRow.ItemArray[i + 1].ToString();
                }

                _previousTable.AddRow(rows);
            }

            _previousTable.SaveChanges();
        }

        public static object[] GetArrayFromRow(Row row)
        {
            var arr = new object[row.Cols.Count];

            for (int i = 0; i < row.Cols.Count; i++)
                arr[i] = row.Cols[i].Data;
            return arr;
        } 
        #endregion


        public DataView DataTable
        {
            get { return (DataView)GetValue(DataTableProperty); }
            set { SetValue(DataTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataTableProperty =
            DependencyProperty.Register("DataTable", typeof(DataView), typeof(MainWindowViewModel), new PropertyMetadata(null));



        public ICollectionView Tables
        {
            get { return (ICollectionView)GetValue(TablesProperty); }
            set { SetValue(TablesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tables.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TablesProperty =
            DependencyProperty.Register("Tables", typeof(ICollectionView), typeof(MainWindowViewModel), new PropertyMetadata(null));



        #region Команды

        #region SaveDatabaseCommand

        public ICommand SaveDatabaseCommand { get; }

        private bool CanSaveDatabaseCommand(object p) => true;
        private void OnSaveDatabaseCommandExecuted(object p)
        {
            DataBase.MakeBaseFile(DataBase.Path);
            DataBase.CompresByGlobalPath();
            DataBase.CryptData();
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
            DataBase.DeCryptData();
            DataBase.DecompresByGlobalPath();
            DataBase.DisassembleBaseFile();
            #endregion

            var infoDialogViewModel = new InfoDialogViewModel();

            infoDialogViewModel.ShowDialog( "BDB NOTIFICATION", "База данных успешна сохранена в новом пути!", Visibility.Hidden);
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

            if (infoDialogViewModel.ShowDialog("BDBSECYRITY", "Введите пароль базы данных") == false)
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
            var infoDialog = new InfoDialogViewModel();

            string tableName;

            if (infoDialog.ShowDialog("BDB CREATOR", "Введите имя таблицы") == false)
                return;

            tableName = infoDialog.InputText + ".bdbt";

            try
            {
                var tables = DataBase.GetTables();

                if (tables.Find(x => x.Name == infoDialog.InputText) != null)
                {
                    infoDialog.ShowDialog("BDB INFORMER", "Таблица с таким именем уже существует", Visibility.Hidden);
                    return;
                }
            }
            catch { }

            var table = new Table(SetOnlyPath(DataBase.Path) + tableName);

            table.SaveChanges();
            

            Tables = CollectionViewSource.GetDefaultView(DataBase.GetTables());
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

        #region SaveCurrentTableCommand
        public ICommand SaveCurrentTableCommand { get; }

        private void OnSaveCurrentTableCommandExecuted(object p)
        {
            if(!(p is Table currentTable))
                return;

            currentTable.SaveChanges();
            SavePreviousTable();
        }
        private bool CanSaveCurrentTableCommand(object p) => true;
        #endregion

        #region Close
        protected override void OnCloseWindowCommandExecuted(object p)
        {
            var infoDialog = new InfoDialogViewModel();

            bool? canClose = infoDialog.ShowDialog(title:"BDB SAVER", 
                infoText:"Сохранить базу данных?", 
                firstButtonText: "Сохранить и выйти", 
                secondButtonText: "Выйти", 
                thirdButtonText: "Отмена", 
                textBoxVisibility: Visibility.Hidden,
                firstButtonVisibility: Visibility.Visible);

            if (canClose == false)
                return;

            if (infoDialog.IsFirstClick)
            {
                OnSaveDatabaseCommandExecuted(p);
                infoDialog.ShowDialog("BDB NOTIFICATION", "База данных успешна сохранена!", Visibility.Hidden);
                base.OnCloseWindowCommandExecuted(p);
            }
            else if(infoDialog.IsSecondClick)
            {
                OnSaveDatabaseCommandExecuted(p);
                base.OnCloseWindowCommandExecuted(p);
            }
        }
        private void CloseWithCreateNew(object p)
        {
            var infoDialog = new InfoDialogViewModel();

            bool? canClose = infoDialog.ShowDialog(title: "BDB SAVER",
                infoText: "Сохранить базу данных?",
                firstButtonText: "Сохранить и выйти",
                secondButtonText: "Выйти",
                thirdButtonText: "Отмена",
                textBoxVisibility: Visibility.Hidden,
                firstButtonVisibility: Visibility.Visible);

            if (canClose == false)
                return;

            if (infoDialog.IsFirstClick)
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

        #region AddColumnCommand
        public ICommand AddColumnCommand { get; }

        private void OnAddColumnCommandExecuted(object p)
        {
            var infoDialogViewModel = new InfoDialogViewModel();

            if (infoDialogViewModel.ShowDialog( "BDB CREATOR", "Введите название столбца") == false)
                return;

            var columnName = infoDialogViewModel.InputText;

            if (!(p is Table currentTable))
                return;

            //TODO: проверка наличия уже такой колонки. (c) Коля.

            var rows = new string[DataTable.Table.Columns.Count + 1];

            for (int i = 0; i < rows.Length - 1; i++)
            {
                rows[i] = DataTable.Table.Columns[i].ColumnName;
            }

            rows[rows.Length - 1] = columnName;

            currentTable.Rows[0].Cols.Add(new Row.Column(columnName));

            currentTable.SaveChanges();
            var data = new DataTable();

            foreach (var column in currentTable.Rows[0].Cols)
                data.Columns.Add(column.Data);

            for (int i = 1; i < currentTable.Rows.Count; i++)
                data.Rows.Add(GetArrayFromRow(currentTable.Rows[i]));

            DataTable = data.AsDataView();
        }

        private bool CanAddColumnCommand(object p) => true;
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
            SaveCurrentTableCommand = new ActionCommand(OnSaveCurrentTableCommandExecuted, CanSaveCurrentTableCommand);

            AddColumnCommand = new ActionCommand(OnAddColumnCommandExecuted, CanAddColumnCommand);
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
