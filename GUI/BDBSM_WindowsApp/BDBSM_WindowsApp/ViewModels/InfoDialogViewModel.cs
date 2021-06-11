using BDBSM_WindowsApp.Infrastructure.Commands;
using BDBSM_WindowsApp.ViewModels.Base;
using BDBSM_WindowsApp.Views;
using System.Windows;
using System.Windows.Input;

namespace BDBSM_WindowsApp.ViewModels
{
    class InfoDialogViewModel : BaseViewModel
    {
        /// <summary>Заголовок окна.</summary>
        public string TitleWindow
        {
            get { return (string)GetValue(TitleWindowProperty); }
            set { SetValue(TitleWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleWindowProperty =
            DependencyProperty.Register("TitleWindow", typeof(string), typeof(InfoDialogViewModel), new PropertyMetadata("BDB SECURITY"));

        public string FirstButtonText
        {
            get { return (string)GetValue(FirstButtonTextProperty); }
            set { SetValue(FirstButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstButtonTextProperty =
            DependencyProperty.Register("FirstButtonText", typeof(string), typeof(InfoDialogViewModel), new PropertyMetadata(""));

        public string SecondButtonText
        {
            get { return (string)GetValue(SecondButtonTextProperty); }
            set { SetValue(SecondButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondButtonTextProperty =
            DependencyProperty.Register("SecondButtonText", typeof(string), typeof(InfoDialogViewModel), new PropertyMetadata(""));

        public string ThirdButtonText
        {
            get { return (string)GetValue(ThirdButtonTextProperty); }
            set { SetValue(ThirdButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThirdButtonTextProperty =
            DependencyProperty.Register("ThirdButtonText", typeof(string), typeof(InfoDialogViewModel), new PropertyMetadata(""));

        /// <summary>Информационное сообщение пользователю.</summary>
        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InfoText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoTextProperty =
            DependencyProperty.Register("InfoText", typeof(string), typeof(InfoDialogViewModel), new PropertyMetadata("Задайте пароль для файла БД:"));


        /// <summary>Устанавливает текстовому полю видимость.</summary>
        public Visibility TextBoxVisibility
        {
            get { return (Visibility)GetValue(TextBoxVisibilityProperty); }
            set { SetValue(TextBoxVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBoxVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxVisibilityProperty =
            DependencyProperty.Register("TextBoxVisibility", typeof(Visibility), typeof(InfoDialogViewModel), new PropertyMetadata(Visibility.Visible));
        public Visibility FirstButtonVisibility
        {
            get { return (Visibility)GetValue(FirstButtonVisibilityProperty); }
            set { SetValue(FirstButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBoxVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstButtonVisibilityProperty =
            DependencyProperty.Register("FirstButtonVisibility", typeof(Visibility), typeof(InfoDialogViewModel), new PropertyMetadata(Visibility.Visible));


        public string InputText
        {
            get { return (string)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputTextProperty =
            DependencyProperty.Register("InputText", typeof(string), typeof(InfoDialogViewModel), new PropertyMetadata(""));

        #region Команды


        public bool IsFirstClick
        {
            get { return (bool)GetValue(IsFirstClickProperty); }
            set { SetValue(IsFirstClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFirstClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFirstClickProperty =
            DependencyProperty.Register("IsFirstClick", typeof(bool), typeof(InfoDialogViewModel), new PropertyMetadata(false));

        public bool IsSecondClick
        {
            get { return (bool)GetValue(IsSecondClickProperty); }
            set { SetValue(IsSecondClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFirstClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSecondClickProperty =
            DependencyProperty.Register("IsSecondClick", typeof(bool), typeof(InfoDialogViewModel), new PropertyMetadata(false));
        #region FirstButtonCommand
        public ICommand AcceptCommand { get; }

        private void OnAcceptCommandExecute(object p)
        {
            IsFirstClick = true;
            DialogResult = true;
            Close();
        }
        #endregion

        #region SecondButtonCommand
        public ICommand SecondAcceptCommand { get; }

        private void OnSecondAcceptCommandExecute(object p)
        {
            IsSecondClick = true;
            DialogResult = true;
            Close();
        }
        #endregion

        #endregion

        public InfoDialogViewModel()
        {
            AcceptCommand = new ActionCommand(OnAcceptCommandExecute, null);
            SecondAcceptCommand = new ActionCommand(OnSecondAcceptCommandExecute, null);
        }

        public bool? ShowDialog(string title, string infoText, Visibility textBoxVisibility = Visibility.Visible, 
            string firstButtonText = "", string secondButtonText = "OK", string thirdButtonText = "Отмена", Visibility firstButtonVisibility = Visibility.Hidden)
        {
            TitleWindow = title;
            InfoText = infoText;
            TextBoxVisibility = textBoxVisibility;
            FirstButtonText = firstButtonText;
            SecondButtonText = secondButtonText;
            ThirdButtonText = thirdButtonText;
            FirstButtonVisibility = firstButtonVisibility;

            return ShowDialog(this, new InfoDialog());
        }
        public bool? ShowDialog()
        {
            FirstButtonText = "";
            SecondButtonText = "OK";
            ThirdButtonText = "Отмена";
            FirstButtonVisibility = Visibility.Hidden;

            return ShowDialog(this, new InfoDialog());

        }
    }
}
