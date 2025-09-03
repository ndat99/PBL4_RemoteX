using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RemoteX.Client.ViewModels
{
    public class ClientViewModel : BaseViewModel
    {
        private string _statusText;
        private Brush _statusColor;
        private ClientInfo _clientInfo;        

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

        public ClientViewModel()
        {
            StatusText = " • Chưa kết nối";
            StatusColor = Brushes.Gray;

        }
    }
}
