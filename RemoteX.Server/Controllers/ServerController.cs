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
using RemoteX.Core;

namespace RemoteX.Server.Controllers
{
    public class ServerController
    {
        private TcpListener _listener;
        private Thread _listenThread;
        private bool _isRunning;
        private readonly List<ClientHandler> _handlers = new();
        private readonly Dictionary<string, string> _activeConnection = new();
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
                    handler.Disconnected += async handler =>
                    {
                        App.Current.Dispatcher.Invoke(() => Clients.Remove(handler.Info));

                        //Xoá khỏi dictionary nếu tồn tại session
                        if (_activeConnection.ContainsKey(handler.Info.Id))
                        {
                            var partnerID = _activeConnection[handler.Info.Id];
                            _activeConnection.Remove(handler.Info.Id);
                            _activeConnection.Remove(partnerID);

                            var partnerHandler = _handlers.FirstOrDefault(x => x.Info.Id == partnerID);
                            if (partnerHandler != null)
                            {
                                await partnerHandler.SendAsync(new ConnectRequest
                                {
                                    From = "Server",
                                    To = partnerID,
                                    Status = "❌ Đối tác đã ngắt kết nối"
                                });
                            }
                        }
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

                    handler.ChatMessageReceived += async chatMsg =>
                    {
                        await ForwardMessageAsync(chatMsg);
                    };

                    handler.ScreenFrameReceived += async screenMsg =>
                    {
                        await ForwardMessageAsync(screenMsg);
                    };

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

            //Thêm vào dictionary
            _activeConnection[request.From] = request.To;
            _activeConnection[request.To] = request.From;
            // Forward request tới target
            await target.SendAsync(request);
        }

        public async Task ForwardMessageAsync(Message msg)
        {
            string targetID = null;
            switch (msg)
            {
                case ChatMessage chat:
                    targetID = _activeConnection.GetValueOrDefault(chat.From); //trả về value của dictionary có key = from
                    break;
                case ScreenFrameMessage screenFrame:
                    targetID = _activeConnection.GetValueOrDefault(screenFrame.From);
                    break;
                default:
                    return;
            }   
            if(targetID != null)
            {
                //Tìm handler của người nhận trong danh sách đang kết nối
                var targetHandler = _handlers.FirstOrDefault(h => h.Info.Id == targetID);
                if(targetHandler != null)
                       await targetHandler.SendAsync(msg);
            }
            else
            {
                var senderHandler = _handlers.FirstOrDefault(h => h.Info.Id == _activeConnection.GetValueOrDefault(msg.To));
                await senderHandler.SendAsync(new ConnectRequest
                {
                    From = "Server",
                    To = msg.From,
                    Status = "❌ Không tìm thấy đối tác"
                });
                return;
            }
        }
    }
}
