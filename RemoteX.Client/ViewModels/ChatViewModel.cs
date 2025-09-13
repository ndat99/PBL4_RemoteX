using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            //InputMessage = "Nhập tin nhắn";
            //var msg = new ChatMessage()
            //{
            //    From = "123",
            //    To = "456",
            //    Message = "Hello, đây là tin nhắn mẫu để kiểm tra thử",
            //    IsMine = false
            //};
            //Messages.Add(msg);
        }

        public void AddMessage(ChatMessage msg)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(msg);
            });
        }

        //Gui tin nhan
        public void SendMessage(string myId, string partnerId, string messageText)
        {
            //kiem tra input
            if (string.IsNullOrWhiteSpace(InputMessage)) return;

            var chat = new ChatMessage
            {
                From = myId,
                To = partnerId,
                Message = InputMessage,
                IsMine = true,
                Timestamp = DateTime.Now
            };

            _ = _client.SendAsync(chat);

            AddMessage(chat);

            InputMessage = "";
        }

        //Nhan tin nhan
        public void ReceiveMessage(ChatMessage msg)
        {
            msg.IsMine = false;
            AddMessage(msg);
        }
    }
}