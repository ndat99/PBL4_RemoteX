using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace RemoteX.Client.ViewModels
{
    public class ClientViewModel : BaseViewModel
    {

        private string _statusText;
        private Brush _statusColor;
        private ClientInfo _clientInfo;        
        public ObservableCollection<ChatMessage> Messages { get; set; } = new();
        private string _newMessage;

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public ClientInfo ClientInfo
        {
            get => _clientInfo;
            set
            {
                _clientInfo = value;
                OnPropertyChanged(nameof(ClientInfo));
            }
        }

        public string NewMessage
        {
            get => _newMessage;
            set
            {
                _newMessage = value;
                OnPropertyChanged(nameof(NewMessage));
            }
        }

        //ICommand SendMessageCommand { get; }

        //public ClientViewModel()
        //{
        //    StatusText = " ⬤  Đang kết nối tới Server";
        //    StatusColor = Brushes.Yellow;
        //}

        //private void SendMessage()
        //{
        //    if (string.IsNullOrWhiteSpace(NewMessage))
        //        return;

        //    //Add vao danh sach (tin nhan cua minh)
        //    Messages.Add(new ChatMessage
        //    {
        //        SenderId = "Me",
        //        Content = NewMessage,
        //        Timestamp = DateTime.Now,
        //        IsMine = true
        //    });

        //    //TODO: goi NetworkService gui ra server

        //    NewMessage = string.Empty;
        //}

        ////Backend goi khi nhan tin nhan tu server
        //public void ReceiveMessage(string senderId, string content)
        //{
        //    Messages.Add(new ChatMessage
        //    {
        //        SenderId = senderId,
        //        Content = content,
        //        Timestamp = DateTime.Now,
        //        IsMine = false
        //    });
        //}
    }
}
