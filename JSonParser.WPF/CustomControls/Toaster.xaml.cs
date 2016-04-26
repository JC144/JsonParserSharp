using EasyMVVM.WPF.Input;
using JSonParser.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JSonParser.WPF.CustomControls
{
    public enum ToastType
    {
        Warning, Error, Infos
    }

    public class MessageModel
    {
        public string Message { get; set; }
        public ToastType ToastType { get; set; }

        public MessageModel(string message, ToastType toastType)
        {
            Message = message;
            ToastType = toastType;
        }
    }

    public partial class Toaster : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MessageModelProperty = DependencyProperty.Register(
            "MessageModel", typeof(MessageModel), typeof(Toaster), new PropertyMetadata(default(MessageModel), MessageModelChanged));

        public MessageModel MessageModel
        {
            get { return (MessageModel)GetValue(MessageModelProperty); }
            set { SetValue(MessageModelProperty, value); }
        }

        private static void MessageModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Toaster currentItem = d as Toaster;
            if (currentItem != null)
            {
                currentItem.RaisePropertyChanged("Message");
                currentItem.RaisePropertyChanged("MessageColor");
            }
        }

        public static readonly DependencyProperty DisplayMessageCommandProperty = DependencyProperty.Register(
            "DisplayMessageCommand", typeof(ICommand), typeof(Toaster), new PropertyMetadata(default(ICommand), null));
        public ICommand DisplayMessageCommand
        {
            get { return (ICommand)GetValue(DisplayMessageCommandProperty); }
            set { SetValue(DisplayMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty CloseMessageCommandProperty = DependencyProperty.Register(
            "CloseMessageCommand", typeof(ICommand), typeof(Toaster), new PropertyMetadata(default(ICommand), null));
        public ICommand CloseMessageCommand
        {
            get { return (ICommand)GetValue(CloseMessageCommandProperty); }
            set { SetValue(CloseMessageCommandProperty, value); }
        }

        public string Message
        {
            get
            {
                if (MessageModel != null)
                    return MessageModel.Message;
                return String.Empty;
            }
        }

        public SolidColorBrush MessageColor
        {
            get
            {
                if (MessageModel != null)
                {
                    switch (MessageModel.ToastType)
                    {
                        case ToastType.Error:
                            return new SolidColorBrush(Colors.Red);
                        case ToastType.Infos:
                            return new SolidColorBrush(Colors.Green);
                        case ToastType.Warning:
                            return new SolidColorBrush(Colors.Orange);
                        default:
                            return new SolidColorBrush(Colors.Green);
                    }
                }
                return new SolidColorBrush(Colors.Green); 
            }
        }

        public Toaster()
        {
            //DisplayMessageCommand = new RelayCommand(DisplayMessage);
            //CloseMessageCommand = new RelayCommand(CloseMessage);  
            InitializeComponent();
            DataContextChanged += Toaster_DataContextChanged;
        }

        void Toaster_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is MessagesViewModel)
            {
                MessagesViewModel mvm = e.NewValue as MessagesViewModel;
                mvm.MessageDisplayed = new RelayCommand(DisplayMessage);
                mvm.MessageClosed = new RelayCommand(CloseMessage);
            }
        }

        private void DisplayMessage()
        {
            Dispatcher.Invoke(() => ((Storyboard)Resources["EntranceStoryboard"]).Begin());

        }

        private void CloseMessage()
        {
            Dispatcher.Invoke(() => ((Storyboard)Resources["CloseStoryboard"]).Begin());
        }

        #region [ RaisePropertyChanged ]

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion [ RaisePropertyChanged ]
    }
}
