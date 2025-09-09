using RemoteX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using RemoteX.Core.Networking;
using System.Text.Json;
using System.Threading;
using RemoteX.Core.Models;
using RemoteX.Server.Services;

namespace RemoteX.Server.Controllers
{
    public class ServerController
    {
        private TcpListener _listener;
        private Thread _listenThread;
        public ObservableCollection<ClientInfo> Clients { get; } = new();
        public event Action<string> StatusChanged;
        private bool _isRunning;
        private readonly List<ClientHandler> _handlers = new();

        public void Start(int port)
        {
            try
            {
                if (_isRunning) return;

                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;

                _listenThread = new Thread(AcceptClients) { IsBackground = true };
                _listenThread.Start();
                StatusChanged?.Invoke($"Server đang chạy trên cổng {port}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi: {ex.Message}");
            }
        }
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener.Stop();
            _listenThread?.Join();
            Clients.Clear();

            StatusChanged?.Invoke("Đã tắt Server");
        }

        private async void AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                //var listener = new MessageListener(tcpClient);
                var handler = new ClientHandler(tcpClient);

                // Khi client ngắt kết nối, xóa nó khỏi danh sách
                handler.Disconnected += handler =>
                {
                    App.Current.Dispatcher.Invoke(() => Clients.Remove(handler.Info));
                };
                    // Khi nhận ClientInfo → thêm vào list
                    handler.ClientInfoReceived += info =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            StatusChanged?.Invoke($"Đã nhận client: {info.Id} - {info.Password}");
                            Clients.Add(info);
                        });
                    };
                    _handlers.Add(handler);
                    _ = handler.StartAsync();
                }
                catch(Exception ex)
                {
                    if (_isRunning)
                        StatusChanged?.Invoke($"[Accept Error] {ex.Message}");
                }
            }
        }
    }
}
