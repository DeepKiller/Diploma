using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
            set {
                SetValue(SelectedTableProperty, value);
            }
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

            current.TableName = current.SelectedTable.Name;
            
            current.SavePreviousTable();

            current._previousTable = current.SelectedTable;

            current.DataTable = DataTableUpdate(current.SelectedTable);

            current.Tables.Refresh();
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



        public DataGridColumn CurrentColumn
        {
            get { return (DataGridColumn)GetValue(CurrentColumnProperty); }
            set { SetValue(CurrentColumnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentColumnProperty =
            DependencyProperty.Register("CurrentCell", typeof(DataGridColumn), typeof(MainWindowViewModel), new PropertyMetadata(null));



        public int SelectedRowId
        {
            get { return (int)GetValue(SelectedRowIdProperty); }
            set { SetValue(SelectedRowIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedRowId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedRowIdProperty =
            DependencyProperty.Register("SelectedRowId", typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));



        public string SelectedColumnName
        {
            get { return (string)GetValue(SelectedColumnNameProperty); }
            set { SetValue(SelectedColumnNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColumnName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColumnNameProperty =
            DependencyProperty.Register("SelectedColumnName", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(""));



        public string TableName
        {
            get { return (string)GetValue(TableNameProperty); }
            set { SetValue(TableNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TableName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableNameProperty =
            DependencyProperty.Register("TableName", typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(""));




        public DataGridColumn SelectedColumn
        {
            get { return (DataGridColumn)GetValue(SelectedColumnProperty); }
            set { SetValue(SelectedColumnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColumnProperty =
            DependencyProperty.Register("SelectedColumn", typeof(DataGridColumn), typeof(MainWindowViewModel), new PropertyMetadata(null));

        #region Команды

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
            

            DataBase.Path = saveFileDialog.FileName;

            var tables = DataBase.GetTables();
            
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

            var infoDialog = new InfoDialogViewModel();

            infoDialog.ShowDialog( "BDB NOTIFICATION", "База данных успешна сохранена в новом пути!", Visibility.Hidden);
        }
        #endregion

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

            var infoDialog = new InfoDialogViewModel();

            if (infoDialog.ShowDialog("BDBSECYRITY", "Введите пароль базы данных") == false)
                return;

            #region Открытие файла.
            DataBase.DeCryptData(infoDialog.InputText);

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

            if (infoDialog.ShowDialog("BDB CREATOR", "Введите имя таблицы") == false)
                return;

            var tableName = infoDialog.InputText + ".bdbt";

            try
            {
                var tables = DataBase.GetTables();
                
                if (tables.Find(x => x.Name == infoDialog.InputText) != null)
                {
                    infoDialog.ShowDialog("BDB INFORMER", "Таблица с таким именем уже существует", Visibility.Hidden);
                    return;
                }

                var regex = new Regex(@"\W");
                
                if(regex.IsMatch(infoDialog.InputText))
                { 
                    infoDialog.ShowDialog("BDB INFORMER", "Неверно задано имя таблицы", Visibility.Hidden);
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

        #region DeleteTableCommand
        public ICommand DeleteTableCommand { get; }

        private void OnDeleteTableCommandExecuted(object p)
        {
            if (Tables == null)
                return;

            var infoDialog = new InfoDialogViewModel();

            if (infoDialog.ShowDialog("BDB CHANGER", "Введите название таблицы, которую следуте удалить") == false)
                return;

            var tables = DataBase.GetTables();

            var table = tables.Find(x => x.Name == infoDialog.InputText);

            if (table == null)
            {
                infoDialog.ShowDialog("BDB ALERT", "Таблица не найдена", Visibility.Hidden);
                return;
            }

            table.DeleteTable();

            tables.Remove(tables.Find(x => x.Name == infoDialog.InputText));

            Tables = CollectionViewSource.GetDefaultView(tables);

            infoDialog.ShowDialog("BDB INFORMER", "Таблица успешно удалена", Visibility.Hidden);
        }
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

        #region SelectedCellsChangedCommand
        public ICommand SelectedCellsChangedCommand { get; }

        private void OnSelectedCellsChangedCommandExecuted(object p)
        {
            if (!(p is Table currentTable))
                return;

            SavePreviousTable();

            currentTable.SaveChanges();

            _previousTable = currentTable;
            
            DataTable = DataTableUpdate(currentTable);
        }
        #endregion

        #region EditCurrentTableNameButtonCommand
        public ICommand EditCurrentTableNameButtonCommand { get; }

        private void OnEditCurrentTableNameButtonCommandExecuted(object p)
        {
            if (!(p is Table currentTable))
                return;

            var infoDialog = new InfoDialogViewModel();

            if (infoDialog.ShowDialog("BDB EDITOR", "Введите новое название таблицы") == false)
                return;

            var regex = new Regex(@"\W");

            if (regex.IsMatch(infoDialog.InputText))
            {
                infoDialog.ShowDialog("BDB INFORMER", "Неверно задано имя таблицы", Visibility.Hidden);
                return;
            }
            TableName = infoDialog.InputText;
            SelectedTable.Name = TableName;
            Tables.Refresh();
            //currentTable.Name = infoDialog.InfoText;
            //SelectedTable.Name = infoDialog.InputText;
            //DataTable = DataTableUpdate(currentTable);
        }
        #endregion

        #region EditCurrentTableNameCommand
        public ICommand EditCurrentTableNameCommand { get; }

        private void OnEditCurrentTableNameCommandExecuted(object p)
        {
            if (!(p is string currentTable))
                return;

            var infoDialog = new InfoDialogViewModel();

            //if (infoDialog.ShowDialog("BDB EDITOR", "Введите новое название таблицы") == false)
            //    return;

            var regex = new Regex(@"\W");

            if (regex.IsMatch(TableName))
            {
                infoDialog.ShowDialog("BDB INFORMER", "Неверно задано имя таблицы", Visibility.Hidden);
                string newname = "";
                foreach (char character in currentTable)
                    if (!regex.IsMatch(character.ToString()))
                        newname += character;
                TableName = newname;
                SelectedTable.Name = newname;
                return;
            }
            SelectedTable.Name = TableName;
            Tables.Refresh();
            //currentTable.Name = infoDialog.InfoText;
            //SelectedTable.Name = infoDialog.InputText;
            //DataTable = DataTableUpdate(currentTable);
        }

        #endregion

        #region EditCurrentColumnNameCommand
        public ICommand EditCurrentColumnNameCommand { get; }

        private void OnEditCurrentColumnNameCommandExecuted(object p)
        {
            if (!(p is DataGridColumn currentColumn))
                return;

            var infoDialog = new InfoDialogViewModel();

            if (currentColumn.Header.ToString().ToLower() == "id")
            {
                infoDialog.ShowDialog("BDB ALERT", "Изменить \"id\" нельзя");
                return;
            }
            var headerNames = new string[SelectedTable.Rows[0].Cols.Count];
            for (int i = 0; i < headerNames.Length; i++)
                headerNames[i] = SelectedTable.Rows[0].Cols[i].Data;

            infoDialog.ShowDialog("BDB EDITOR", "Введите новое имя колонки");
            for (int i = 0; i < headerNames.Length; i++)
                if (headerNames[i] == SelectedColumn.Header.ToString())
                {
                    headerNames[i] = infoDialog.InputText;
                    break;
                }
            SelectedTable.Rows[0] = new Row(headerNames);
            SelectedTable.SaveChanges();

            DataTable = DataTableUpdate(SelectedTable);
        }
        #endregion

        #region DeleteCurrentTableCommand
        public ICommand DeleteCurrentTableCommand { get; }

        private void OnDeleteCurrentTableCommandExecuted(object p)
        {
            var infoDialog = new InfoDialogViewModel();

            if (!(p is Table currentTable))
            {
                infoDialog.ShowDialog("BDB INFORMER", "Выберите таблицу, которую следует удалить", Visibility.Hidden);
                return;
            }

            if (infoDialog.ShowDialog("BDB ALERT", "Вы действительно хотите удалить выбранную таблицу?", Visibility.Hidden, "", "ДА") == false)
                return;

            currentTable.DeleteTable();

            _previousTable = null;

            var tables = DataBase.GetTables();

            tables.Remove(currentTable);

            Tables = CollectionViewSource.GetDefaultView(tables);
            Tables.Refresh();
            SelectedTable = null;
            DataTable = new DataTable().DefaultView; 
        }
        #endregion

        #region DeleteColumnCommand
        public ICommand DeleteColumnCommand { get; }

        private void OnDeleteColumnCommand(object p)
        {
            if(!(p is Table currentTable))
                return;

            var infoDialog = new InfoDialogViewModel();

            if (infoDialog.ShowDialog("BDB DELETER", "Введите название колонки для удаления") == false)
                return;

            var columnNames = new List<string>();
            int position = currentTable.Rows[0].Cols.FindIndex(x => x.Data == infoDialog.InputText);

            if(position == -1)
            {
                infoDialog.ShowDialog("BDB ALERT", "Такой колонки не существует", Visibility.Hidden);
                return;
            }
            if (infoDialog.InputText == "id")
            {
                infoDialog.ShowDialog("BDB ALERT", "Колонку \"id\" удалить нельзя", Visibility.Hidden);
                return;
            }

            foreach (Row row in currentTable.Rows)
                row.Cols.RemoveAt(position);

            foreach (Row.Column column in currentTable.Rows[0].Cols)
            { 
                if (column.Data != infoDialog.InputText)
                { 
                    columnNames.Add(column.Data);
                }
            }

            currentTable.Rows[0] = new Row(columnNames.ToArray());

            currentTable.SaveChanges();

            DataTable = DataTableUpdate(currentTable);
        }
        #endregion

        #region AddColumnCommand
        public ICommand AddColumnCommand { get; }

        private void OnAddColumnCommandExecuted(object p)
        {
            if (!(p is Table currentTable))
                return;
            
            var infoDialog = new InfoDialogViewModel();

            if (infoDialog.ShowDialog( "BDB CREATOR", "Введите название столбца") == false)
                return;
        
            var columnName = infoDialog.InputText;

            if (currentTable.Rows[0].Cols.Find(x => x.Data == columnName) != null)
            {   
                infoDialog.ShowDialog("BDB INFORMER", "Такое имя колонки уже существует", Visibility.Hidden);
                return;
            }

            var rows = new string[DataTable.Table.Columns.Count + 1];

            for (int i = 0; i < rows.Length - 1; i++)
            {
                rows[i] = DataTable.Table.Columns[i].ColumnName;
            }

            rows[rows.Length - 1] = columnName;

            currentTable.Rows[0].Cols.Add(new Row.Column(columnName));

            currentTable.SaveChanges();

            DataTable = DataTableUpdate(currentTable);
        }

        private bool CanAddColumnCommand(object p) => true;
        #endregion

        #region DeleteCurrentRowCommand
        public ICommand DeleteCurrentRowCommand { get; }

        private void OnDeleteCurrentRowCommandExecuted(object p)
        {
            if (SelectedRowId == -1)
            {
                new InfoDialogViewModel().ShowDialog("BDB ALERT", "Такой колонки не существует");
                return;
            }

            SelectedTable.DeleteRow(SelectedRowId);

            SelectedTable.SaveChanges();

            DataTable = DataTableUpdate(SelectedTable);
        }
        #endregion

        #region UpdateSelectedItemCommand
        public ICommand UpdateSelectedItemCommand { get; }
        private void OnUppdateSelectedItemCommand(object p)
        {
            if (!(p is DataRowView selectedItem))
                return;

            if(selectedItem.Row.ItemArray[0] == System.DBNull.Value || selectedItem.Row.ItemArray[0].GetType() == "".GetType())
                return;
            SelectedColumnName = SelectedColumn.Header.ToString();
            SelectedRowId = int.Parse(selectedItem.Row.ItemArray[0].ToString());
        }
        #endregion

        #region CreateRelationCommand
        public ICommand CreateRelationCommand { get; }

        private void OnCreateRelationCommandExecuted(object p)
        {
            if(DataBase.GetTables().Count < 2)
            {
                new InfoDialogViewModel().ShowDialog("BDB ALERT", "Должно быть хотя бы две таблицы для создания связей", Visibility.Hidden);
                return;
            }
            Show(new CreateRelationViewModel(DataBase.GetTables()), new CreateRelation());
        }
        private bool CanCreateRelationCommand(object p) => true;
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

        #endregion

        public MainWindowViewModel()
        {
            DatabaseName = SetDatabaseName();
            Tables = CollectionViewSource.GetDefaultView(DataBase.GetTables());
            Tables.Filter = FilterTable;

            CreateNewDatabaseCommand = new ActionCommand(OnCreateNewDatabaseCommandExecuted, CanCreateNewDatabaseCommand);
            SaveDatabaseHowCommand = new ActionCommand(OnSaveDatabaseHowCommandExecuted, CanSaveDatabaseHowCommand);
            SaveDatabaseCommand = new ActionCommand(OnSaveDatabaseCommandExecuted, CanSaveDatabaseCommand);
            OpenDatabaseCommand = new ActionCommand(OnOpenDatabaseCommand);
            
            CreateNewTableCommand = new ActionCommand(OnCreateNewTableCommandExecute, CanCreateNewTableCommand);
            DeleteCurrentTableCommand = new ActionCommand(OnDeleteCurrentTableCommandExecuted);
            DeleteTableCommand = new ActionCommand(OnDeleteTableCommandExecuted);
            SaveCurrentTableCommand = new ActionCommand(OnSaveCurrentTableCommandExecuted, CanSaveCurrentTableCommand);
            SaveTablesCommand = new ActionCommand(OnSaveTablesCommandExecuted, CanSaveTablesCommand);
            EditCurrentTableNameCommand = new ActionCommand(OnEditCurrentTableNameCommandExecuted);
            EditCurrentTableNameButtonCommand = new ActionCommand(OnEditCurrentTableNameButtonCommandExecuted);

            DeleteColumnCommand = new ActionCommand(OnDeleteColumnCommand);
            AddColumnCommand = new ActionCommand(OnAddColumnCommandExecuted, CanAddColumnCommand);
            
            SelectedCellsChangedCommand = new ActionCommand(OnSelectedCellsChangedCommandExecuted);
            EditCurrentColumnNameCommand = new ActionCommand(OnEditCurrentColumnNameCommandExecuted);
            DeleteCurrentRowCommand = new ActionCommand(OnDeleteCurrentRowCommandExecuted);
            
            CreateRelationCommand = new ActionCommand(OnCreateRelationCommandExecuted, CanCreateRelationCommand);
            UpdateSelectedItemCommand = new ActionCommand(OnUppdateSelectedItemCommand);
        }

        private static string SetDatabaseName()
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
        private static string SetOnlyPath(string path)
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

        private static DataView DataTableUpdate(Table currentTable)
        {
            var data = new DataTable();
            var id = new string[] { currentTable.Name };

            if (currentTable.Rows.Count == 0)
                currentTable.SetColNames(id);

            foreach (var column in currentTable.Rows[0].Cols)
                data.Columns.Add(column.Data);

            for (int i = 1; i < currentTable.Rows.Count; i++)
                data.Rows.Add(GetArrayFromRow(currentTable.Rows[i]));

            return data.AsDataView();
        }
    }
}
