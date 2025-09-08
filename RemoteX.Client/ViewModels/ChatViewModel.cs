using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Client.Controllers;
using RemoteX.Client.Controllers;
using RemoteX.Core.Models;
using RemoteX.Core.Utils;

namespace RemoteX.Client.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        private readonly ClientController _client;
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

        public ChatViewModel(ClientController client)
        {
            _client = client;
            InputMessage = "Nhập tin nhắn";
            var msg = new ChatMessage()
            {
                From = "123",
                To = "456",
                Message = "Hello, đây là tin nhắn mẫu để kiểm tra thử",
                IsMine = false
            };
            Messages.Add(msg);
        }

        //Gui tin nhan
        //public void SendMessage(string myId, string partnerId)
        //{
        //    if (string.IsNullOrWhiteSpace(InputMessage)) return;

        //    var msg = new ChatMessage
        //    {
        //        From = myId,
        //        To = partnerId,
        //        Message = InputMessage,
        //        IsMine = true
        //    };

        //    _client.SendMessage(msg);

        //    App.Current.Dispatcher.Invoke(() =>
        //    {
        //        Messages.Add(msg);
        //        InputMessage = string.Empty;
        //    });
        //}

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