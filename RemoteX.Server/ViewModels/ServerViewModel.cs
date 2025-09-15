using System.Collections.ObjectModel;
using System.Windows.Media;
using RemoteX.Core.Utils;
using RemoteX.Core.Models;
using RemoteX.Server.Controllers;
using System.Windows.Input;

namespace RemoteX.Server.ViewModels
{
    public class ServerViewModel : BaseViewModel
    {
        private readonly ServerController _server;
        private int _port = 5000;
        private bool _isRunning;

        private string _statusText; //bien chua trang thai
        private Brush _statusColor; //bien chua mau status
        private Brush _statusDot; //bien chua mau cham tron status

        private Action<string> _onStatusChanged;

        public ObservableCollection<ClientInfo> Clients => _server.Clients;

        public ICommand StartCommand { get;  }
        public ICommand StopCommand { get; }



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

        public ServerViewModel()
        {
            _server = new ServerController();
            _server.StatusChanged += OnStatusChanged;

            StartCommand = new RelayCommand(_ => StartServer(), _=> StatusText == "Đã tắt Server");
            StopCommand = new RelayCommand(_ => StopServer(), _ => StatusText.Contains("đang chạy"));
            
            StartServer();
        }

        public void StartServer()
        {
            _server.Start(_port);
            StatusText = $"Server đang chạy trên cổng {_port}";
            StatusColor = Brushes.Green;
            StatusDot = Brushes.Green;
        }

        public void StopServer()
        {
            _server.Stop();
            StatusText = "Đã tắt Server";
            StatusColor = Brushes.Gray;
            StatusDot = Brushes.Gray;
        }

        private void OnStatusChanged(string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusText = message;

                if (message.Contains("đang chạy"))
                    StatusColor = StatusDot = Brushes.Green;
                //if (message.Contains("kết nối"))
                //    _svm.StatusColor = _svm.StatusDot = Brushes.Blue;
                if (message.Contains("Lỗi"))
                    StatusColor = StatusDot = Brushes.Red;
                else if (message.Contains("tắt"))
                    StatusColor = StatusDot = Brushes.Gray;
            });
        }
    }
}
