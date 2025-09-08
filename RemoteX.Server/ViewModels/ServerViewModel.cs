using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using RemoteX.Core.Utils;
using RemoteX.Core.Models;

namespace RemoteX.Server.ViewModels
{
    public class ServerViewModel : BaseViewModel
    {
        private string _statusText; //bien chua trang thai
        private Brush _statusColor; //bien chua mau status
        private Brush _statusDot; //bien chua mau cham tron status

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

        public Brush StatusDot
        {
            get => _statusDot;
            set
            {
                _statusDot = value;
                OnPropertyChanged(nameof(StatusDot));
            }
        }

        public ObservableCollection<ClientInfo> Clients { get; set; }
        public ServerViewModel()
        {
            StatusText = "Đã tắt Server";
            StatusColor = Brushes.Gray;
            StatusDot = Brushes.Gray;
            Clients = new ObservableCollection<ClientInfo>();
        }
    }
}
