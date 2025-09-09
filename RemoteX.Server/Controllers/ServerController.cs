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
using RemoteX.Core.Services;

namespace RemoteX.Server.Controllers
{
    public class ServerController
    {
        private TcpListener _listener;
        private Thread _listenThread;
        private bool _isRunning;
        private readonly List<ClientHandler> _handlers = new();
        public ObservableCollection<ClientInfo> Clients { get; } = new();
        public event Action<string> StatusChanged;

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
                var handler = new ClientHandler(tcpClient);

                    // Khi client ngắt kết nối, xóa nó khỏi danh sách
                    handler.Disconnected += handler =>
                    {
                        App.Current.Dispatcher.Invoke(() => Clients.Remove(handler.Info));
                    };

                    //Khi nhận Connect Request
                    handler.ConnectRequestReceived += request =>
                    {
                        HandleConnectRequest(handler, request);
                    };
                    _handlers.Add(handler);
                    _ = handler.StartAsync();

                    // Khi nhận ClientInfo → thêm vào list
                    handler.ClientInfoReceived += info =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            //StatusChanged?.Invoke($"Đã nhận client: {info.Id} - {info.Password}");
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
        private async void HandleConnectRequest(ClientHandler sender, ConnectRequest request)
        {
            var target = _handlers.FirstOrDefault(h => h.Info.Id == request.To);

            if (target == null)
            {
                await sender.SendAsync(new ConnectRequest
                {
                    From = "Server",
                    To = request.From,
                    Status = "❌ Không tìm thấy đối tác"
                });
                return;
            }

            if (target.Info.Password != request.Password)
            {
                await sender.SendAsync(new ConnectRequest
                {
                    From = "Server",
                    To = request.From,
                    Status = "❌ Sai mật khẩu"
                });
                return;
            }

            // Forward request tới target
            await target.SendAsync(request);
        }
    }
}
