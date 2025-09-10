using RemoteX.Core.Models;
using RemoteX.Core.Utils;

namespace RemoteX.Client.ViewModels
{
    public class InfoViewModel : BaseViewModel
    {

        private string _statusText;
        private System.Windows.Media.Brush _statusColor;
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

        public System.Windows.Media.Brush StatusColor
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

        public InfoViewModel()
        {
            StatusText = " ⬤  Đang kết nối tới Server";
            StatusColor = System.Windows.Media.Brushes.Yellow;
        }
    }
}
