using BDB;
using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace BDBSM_WindowsApp.ViewModels
{
    class CreateRelationViewModel : BaseViewModel
    {
        public ICollectionView Tables
        {
            get { return (ICollectionView)GetValue(TablesProperty); }
            set { SetValue(TablesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tables.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TablesProperty =
            DependencyProperty.Register("Tables", typeof(ICollectionView), typeof(CreateRelationViewModel), new PropertyMetadata(null));



        public ICollectionView TablesWithoutSelected
        {
            get { return (ICollectionView)GetValue(TablesWithoutSelectedProperty); }
            set { SetValue(TablesWithoutSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TablesWithoutSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TablesWithoutSelectedProperty =
            DependencyProperty.Register("TablesWithoutSelected", typeof(ICollectionView), typeof(CreateRelationViewModel), new PropertyMetadata(null));

        #region FirstSelectedTable
        public Table FirstSelectedTable
        {
            get { return (Table)GetValue(FirstSelectedTableProperty); }
            set { SetValue(FirstSelectedTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstSelectedTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstSelectedTableProperty =
            DependencyProperty.Register("FirstSelectedTable", typeof(Table), typeof(CreateRelationViewModel), new PropertyMetadata(null, FirstSelectedTable_Changed));

        private static void FirstSelectedTable_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tables = DataBase.GetTables();

            if (!(d is CreateRelationViewModel current))
                return;

            if (current.FirstSelectedTable == null)
                return;

            tables.Remove(tables.Find(x => x.Name == current.FirstSelectedTable.Name));

            current.TablesWithoutSelected = CollectionViewSource.GetDefaultView(tables);

            current.SecondTableColumns = new List<Table.Row.Column>();

            current.FirstTableColumns = current.FirstSelectedTable.Rows[0].Cols;
        } 
        #endregion

        #region SecondSelectedTable
        private Table _secondSelectedTable;
        public Table SecondSelectedTable
        {
            get { return (Table)GetValue(SecondSelectedTableProperty); }
            set { SetValue(SecondSelectedTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondSelectedTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondSelectedTableProperty =
            DependencyProperty.Register("SecondSelectedTable", typeof(Table), typeof(CreateRelationViewModel), new PropertyMetadata(null, SecondSelectedTable_Changed));

        private static void SecondSelectedTable_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CreateRelationViewModel current))
                return;

            if (current.SecondSelectedTable == null)
                return;


            current.SecondTableColumns = current.SecondSelectedTable.Rows[0].Cols;
            current._secondSelectedTable = current.SecondSelectedTable;
        } 
        #endregion

        public Table.Row.Column FirstSelectedColumn
        {
            get { return (Table.Row.Column)GetValue(FirstSelectedColumnProperty); }
            set { SetValue(FirstSelectedColumnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstSelectedColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstSelectedColumnProperty =
            DependencyProperty.Register("FirstSelectedColumn", typeof(Table.Row.Column), typeof(CreateRelationViewModel), new PropertyMetadata(null));
        

        public List<Table.Row.Column> FirstTableColumns
        {
            get { return (List<Table.Row.Column>)GetValue(FirstTableColumnsProperty); }
            set { SetValue(FirstTableColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstTableColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstTableColumnsProperty =
            DependencyProperty.Register("FirstTableColumns", typeof(List<Table.Row.Column>), typeof(CreateRelationViewModel), new PropertyMetadata(new List<Table.Row.Column>()));


        public List<Table.Row.Column> SecondTableColumns
        {
            get { return (List<Table.Row.Column>)GetValue(SecondTableColumnsProperty); }
            set { SetValue(SecondTableColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondTableColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondTableColumnsProperty =
            DependencyProperty.Register("SecondTableColumns", typeof(List<Table.Row.Column>), typeof(CreateRelationViewModel), new PropertyMetadata(new List<Table.Row.Column>()));


        #region Команды

        #region CreateRelationCommand
        public ICommand CreateRelationCommand { get; }

        private void OnCreateRelationCommandExecuted(object p)
        {
            FirstSelectedTable.AddRelation(ref _secondSelectedTable, FirstSelectedColumn.Data);
            FirstSelectedTable.SaveChanges();
            SecondSelectedTable.SaveChanges();

            var infoDialogViewModel = new InfoDialogViewModel();

            infoDialogViewModel.ShowDialog( "BDB NOTIFICATION", "Связь между таблицами создана!", Visibility.Hidden);
        }
        private bool CanCreateRelationCommand(object p) => true;
        #endregion 

        #endregion

        public CreateRelationViewModel()
        {
            CreateRelationCommand = new ActionCommand(OnCreateRelationCommandExecuted, CanCreateRelationCommand);
        }

        public CreateRelationViewModel(ICollectionView tables, Table currentTable)
        {
            Tables = tables;
            FirstSelectedTable = currentTable;
            TablesWithoutSelected = tables;
        
            CreateRelationCommand = new ActionCommand(OnCreateRelationCommandExecuted, CanCreateRelationCommand);
        }
        public CreateRelationViewModel(ICollectionView tables)
        {
            Tables = tables;
            TablesWithoutSelected = tables;
        
            CreateRelationCommand = new ActionCommand(OnCreateRelationCommandExecuted, CanCreateRelationCommand);
        }
    }
}
