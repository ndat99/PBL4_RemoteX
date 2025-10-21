using System.Collections.ObjectModel;
using System.Windows.Media;
using RemoteX.Core.Utils;
using RemoteX.Core.Models;
using RemoteX.Server.Controllers;

namespace RemoteX.Server.ViewModels
{
    public class ServerViewModel : BaseViewModel
    {
        private readonly ServerController _serverController;
        private int _port = 5000;
        private bool _isRunning;

        private string _statusText; //bien chua trang thai
        private Brush _statusColor; //bien chua mau status
        private Brush _statusDot; //bien chua mau cham tron status

        private bool _canStart;
        private bool _canStop;

        private Action<string> _onStatusChanged;

        public ObservableCollection<ClientInfo> Clients => _serverController.Clients;

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

        public bool CanStart
        {
            get => _canStart;
            set
            {
                _canStart = value;
                OnPropertyChanged(nameof(CanStart));
            }
        }

        public bool CanStop
        {
            get => _canStop;
            set
            {
                _canStop = value;
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public ServerViewModel()
        {
            _serverController = new ServerController();
            _serverController.StatusChanged += OnStatusChanged;
   
            StartServer();
        }

        public void StartServer()
        {
            _serverController.Start(_port);
            StatusText = $"Server đang chạy";
            StatusColor = Brushes.Green;
            StatusDot = Brushes.Green;

            CanStart = false;
            CanStop = true;
        }

        public void StopServer()
        {
            _serverController.Stop();
            StatusText = "Đã tắt Server";
            StatusColor = Brushes.Gray;
            StatusDot = Brushes.Gray;

            CanStart = true;
            CanStop = false;
        }

        private void OnStatusChanged(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusText = message;

                if (message.Contains("đang chạy"))
                    StatusColor = StatusDot = Brushes.Green;
                if (message.Contains("Lỗi"))
                    StatusColor = StatusDot = Brushes.Red;
                else if (message.Contains("tắt"))
                    StatusColor = StatusDot = Brushes.Gray;
            });
        }
    }
}
