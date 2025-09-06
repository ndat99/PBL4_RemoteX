using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;

namespace RemoteX.Client.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; set; } = new();
        private string _newMessage;

        public string NewMessage
        {
            get => _newMessage;
            set
            {
                _newMessage = value;
                OnPropertyChanged(nameof(NewMessage));
            }
        }

        public ChatViewModel()
        {
            NewMessage = "Nhập tin nhắn";
        }
    }
}
