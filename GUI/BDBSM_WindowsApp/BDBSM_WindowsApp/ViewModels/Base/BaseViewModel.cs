using BDBSM_WindowsApp.Infrastructure.Commands;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace BDBSM_WindowsApp.ViewModels.Base
{
    class BaseViewModel : DependencyObject
    {
        private Window _wnd = null;

        public bool DialogResult { get; set; } = false;

        public bool IsMaximize
        {
            get { return (bool)GetValue(IsMaximizeProperty); }
            set { SetValue(IsMaximizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMaximize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaximizeProperty =
            DependencyProperty.Register("IsMaximize", typeof(bool), typeof(StartWindowViewModel), new PropertyMetadata(false));

        #region ExpandWindowCommand
        public ICommand ExpandWindowCommand { get; }
        
        protected virtual void OnExpandWindowCommandExecuted(object p)
        {
            if (_wnd.WindowState == WindowState.Normal)
            {
                _wnd.WindowState = WindowState.Maximized;
                IsMaximize = true;
            }
            else
            {
                _wnd.WindowState = WindowState.Normal;
                IsMaximize = false;
            }
        }

        private bool CanExpandWindowCommand(object p) => true;
        #endregion

        #region DragMoveWindowCommand
        public ICommand DragMoveWindowCommand { get; }

        protected virtual void OnDragMoveCommandExecuted(object p)
        {
            _wnd.DragMove();
        }
        private bool CanDragMoveCommand(object p) => true;
        #endregion

        #region RollUpWindowCommand
        public ICommand RollUpWindowCommand { get; }
        protected virtual void OnRollUpWindowCommandExecuted(object p)
        {
            _wnd.WindowState = WindowState.Minimized;
        }
        private bool CanRollUpWindowCommand(object p) => true;

        #endregion

        #region CloseWindowCommand
        public ICommand CloseWindowCommand { get; }
        protected virtual void OnCloseWindowCommandExecuted(object p) => Close();
        protected virtual bool CanCloseWindowCommand(object p) => true;
        protected virtual void Closed() { }

        /// <summary> Закрыть окно. </summary>
        /// <returns>true - Окно закрыто, false - окно не найдено.</returns>
        public virtual bool Close()
        {
            if (_wnd == null)
                return false;

            _wnd.Close();
            _wnd = null;

            return true;
        }
        #endregion

        #region HelpCommand

        public ICommand HelpCommand { get; }

        private void OnHelpCommandExecuted(object p)
        {
            Help.ShowHelp(null, "Help.chm");
        }
        #endregion
        /// <summary>
        /// Метод отображения диалогового окна.
        /// </summary>
        /// <param name="viewModel">Следует указать модель данных вызываемого окна.</param>
        protected void Show(BaseViewModel viewModel, Window view)
        {
            viewModel._wnd = view;
            viewModel._wnd.DataContext = viewModel;
            viewModel._wnd.Closed += (sender, e) => Closed();
            viewModel._wnd.Show();
        }

        /// <summary>
        /// Метод отображения диалогового окна.
        /// </summary>
        /// <param name="viewModel">Следует указать модель данных вызываемого окна.</param>
        protected bool? ShowDialog(BaseViewModel viewModel, Window view)
        {
            viewModel._wnd = view;
            viewModel._wnd.DataContext = viewModel;
            viewModel._wnd.Closed += (sender, e) => Closed();
            viewModel._wnd.ShowDialog();
            
            return viewModel.DialogResult;
        }

        public BaseViewModel()
        {
            ExpandWindowCommand = new ActionCommand(OnExpandWindowCommandExecuted, CanExpandWindowCommand);
            DragMoveWindowCommand = new ActionCommand(OnDragMoveCommandExecuted, CanDragMoveCommand);
            RollUpWindowCommand = new ActionCommand(OnRollUpWindowCommandExecuted, CanRollUpWindowCommand);
            CloseWindowCommand = new ActionCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommand);
            HelpCommand = new ActionCommand(OnHelpCommandExecuted, null);
        }
    }
}
