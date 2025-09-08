using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Client.Services;
using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;

namespace RemoteX.Client.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private readonly RemoteXClient _client;
        public ObservableCollection<ChatMessage> Messages { get; set; } = new();
        private string _inputMessage;

        public string InputMessage
        {
            get => _inputMessage;
            set
            {
                _inputMessage = value;
                OnPropertyChanged(nameof(InputMessage));
            }
        }

        public ChatViewModel(RemoteXClient client)
        {
            _client = client;
            InputMessage = "Nhập tin nhắn";
            var msg = new ChatMessage()
            {
                SenderID = "123",
                ReceiverID = "456",
                Message = "Hello, đây là tin nhắn mẫu để kiểm tra thử",
                IsMine = false
            };
            Messages.Add(msg);

            //Dang ky su kien nhan tin nhan
            //_client.MessageReceived += ReceiveMessage;
        }

        //Gui tin nhan
        public void SendMessage(string myId, string partnerId)
        {
            if (string.IsNullOrWhiteSpace(InputMessage)) return;

            var msg = new ChatMessage
            {
                SenderID = myId,
                ReceiverID = partnerId,
                Message = InputMessage,
                IsMine = true
            };

            _client.SendMessage(msg);

            App.Current.Dispatcher.Invoke(() => {
                Messages.Add(msg);
                InputMessage = string.Empty;
            });
        }

        //Nhan tin nhan
        //public void ReceiveMessage(ChatMessage msg)
        //{
        //    App.Current.Dispatcher.Invoke(() => {
        //        msg.IsMine = false;
        //        Messages.Add(msg);
        //    });
        //}
    }
}