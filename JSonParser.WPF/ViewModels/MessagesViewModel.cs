using EasyMVVM.WPF.ViewModels;
using JSonParser.WPF.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace JSonParser.WPF.ViewModels
{
    public class MessagesViewModel : ViewModelBase
    {
        private DispatcherTimer _timer;
        private Queue<MessageModel> _waitingMessage;

        public ICommand MessageDisplayed { get; set; }
        public ICommand MessageClosed { get; set; }

        public MessagesViewModel()
        {
            _waitingMessage = new Queue<MessageModel>();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 3);
            _timer.Tick += _timer_Tick;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if (Message!=null)
            {
                CloseMessage();
            }
        }

        private MessageModel _message;
        public MessageModel Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        public void AddMessage(MessageModel message)
        {
            if (Message!=null)
            {
                _waitingMessage.Enqueue(message);
            }
            else
            {
                ShowMessage(message);
            }
        }

        private void ShowMessage(MessageModel message)
        {
            Message = message;

            if (MessageDisplayed != null)
                MessageDisplayed.Execute(null);

            _timer.Start();
        }

        private async void CloseMessage()
        {
            _timer.Stop();

            if (MessageClosed != null)
                MessageClosed.Execute(null);

            Message = null;

            await Task.Delay(1000);

            if (_waitingMessage != null && _waitingMessage.Count > 0)
            {
                ShowMessage(_waitingMessage.Dequeue());
            }
        }

    }
}
