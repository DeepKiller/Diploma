using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BDBSM_WindowsApp.Views
{
    /// <summary>
    /// Логика взаимодействия для InfoDialog.xaml
    /// </summary>
    public partial class InfoDialog : Window
    {
        /// <summary>
        /// Свойство Data, которое возвращает то, что введет пользователь.
        /// </summary>
        public string Data { get => TextBoxData.Text; }
        /// <summary>
        /// Свойство заголовка.
        /// </summary>
        public string Title { set => LabelTitle.Content = value; }
        /// <summary>
        /// Свойство текста кнопки ОК.
        /// </summary>
        public string ButtonOKText { set => ButtonOK.Content = value; }
        /// <summary>
        /// Свойство текста кнопки Отмена.
        /// </summary>
        public string ButtonCancelText { set => ButtonCancel.Content = value; }
        /// <summary>
        /// Свойство текста сообщения пользователю.
        /// </summary>
        public string InfoDialogText { set => InfoDialogMessage.Text = value; }

        /// <summary>
        /// Конструктор класса InfoDialog.
        /// </summary>
        /// <param name="infoDialogMessage">Параметр задает сообщение пользователю.</param>
        /// <param name="title">Параметр заголовка окна.</param>
        /// <param name="buttonOKText">Параметр текста кнопки ОК.</param>
        /// <param name="buttonCancelText">Параметр текста кнопки Отмена.</param>
        public InfoDialog(string infoDialogMessage,string title, string buttonOKText, string buttonCancelText)
        {
            InitializeComponent();

            InfoDialogText = infoDialogMessage;
            Title = title;
            ButtonOKText = buttonOKText;
            ButtonCancelText = buttonCancelText;
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InfoDialogWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
