using RemoteX.Core.Models;
using RemoteX.Core.Utils;
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
        private string _currentPartnerId;

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

        public string CurrentPartnerId
        {
            get => _currentPartnerId;
            set
            {
                _currentPartnerId = value;
                OnPropertyChanged(nameof(CurrentPartnerId));
            }
        }

        public ClientViewModel()
        {
            StatusText = " ⬤  Đang kết nối tới Server";
            StatusColor = Brushes.Yellow;
        }
    }
}
