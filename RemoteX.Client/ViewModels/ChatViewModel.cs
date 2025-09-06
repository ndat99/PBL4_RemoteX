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

        public string inputMessage
        {
            get => _inputMessage;
            set
            {
                _inputMessage = value;
                OnPropertyChanged(nameof(inputMessage));
            }
        }

        public ChatViewModel(RemoteXClient client)
        {
            _client = client;
            inputMessage = "Nhập tin nhắn";

            //Dang ky su kien nhan tin nhan
            _client.MessageReceived += ReceiveMessage;
        }

        //Gui tin nhan
        public void SendMessage(string myId, string partnerId)
        {
            if (string.IsNullOrWhiteSpace(inputMessage)) return;

            var msg = new ChatMessage
            {
                SenderID = myId,
                ReceiverID = partnerId,
                Message = inputMessage,
                IsMine = true
            };
            
            _client.SendMessage(msg);
            Messages.Add(msg);
            inputMessage = string.Empty;
        }

        //Nhan tin nhan
        public void ReceiveMessage(ChatMessage msg)
        {
            App.Current.Dispatcher.Invoke(() => {
                msg.IsMine = false;
                Messages.Add(msg);
            });
        }
    }
}
