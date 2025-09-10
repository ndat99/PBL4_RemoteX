using RemoteX.Core.Models;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Net;
using RemoteX.Core.Services;
using RemoteX.Core;
using System;

namespace RemoteX.Server.Controllers
{
    public class ServerController
    {
        private TcpListener _listener;
        private Thread _listenThread;
        private bool _isRunning;
        private readonly List<ClientHandler> _handlers = new();
        private readonly Dictionary<string, string> _activeConnection = new();

        private readonly bool RequireAccept = true; //Auto accept thì chuyển sang false
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
            // Close all handlers
            foreach (var handler in _handlers.ToList())
            {
                try { handler.Close(); } catch { }
            }

            _handlers.Clear();
            _activeConnection.Clear();
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
                    handler.Disconnected += async disconnectedHandler =>
                    {
                        App.Current.Dispatcher.Invoke(() => Clients.Remove(disconnectedHandler.Info));

                        //Xoá khỏi dictionary nếu tồn tại session
                        if (_activeConnection.ContainsKey(disconnectedHandler.Info.Id))
                        {
                            var partnerID = _activeConnection[disconnectedHandler.Info.Id];
                            _activeConnection.Remove(disconnectedHandler.Info.Id);
                            _activeConnection.Remove(partnerID);

                            var partnerHandler = _handlers.FirstOrDefault(x => x.Info.Id == partnerID);
                            if (partnerHandler != null)
                            {
                                try
                                {
                                    await partnerHandler.SendAsync(new Log
                                    {
                                        From = "Server",
                                        To = partnerID,
                                        Content = "❌ Đối tác đã ngắt kết nối"
                                    });
                                }
                                catch (Exception ex)
                                {
                                    StatusChanged?.Invoke($"[Disconnect Error] {ex.Message}");
                                }
                            }
                        }
                    };
                    // Khi nhận ClientInfo → thêm vào list
                    handler.ClientInfoReceived += info =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            //StatusChanged?.Invoke($"Đã nhận client: {info.Id} - {info.Password}");
                            Clients.Add(info);
                        });
                    };
                    //Khi nhận Connect Request
                    handler.ConnectRequestReceived += request =>
                    {
                        _ = Task.Run(() => HandleConnectRequest(handler, request));
                    };



                    handler.ChatMessageReceived += async chatMsg =>
                    {
                        await ForwardMessageAsync(chatMsg);
                    };

                    handler.ScreenFrameReceived += async screenMsg =>
                    {
                        await ForwardMessageAsync(screenMsg);
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
        private async Task HandleConnectRequest(ClientHandler sender, ConnectRequest request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SERVER] Processing ConnectRequest from {request.From} to {request.To}");
                System.Diagnostics.Debug.WriteLine($"[SERVER] Total handlers: {_handlers.Count}");

                // Debug all handlers
                foreach (var h in _handlers)
                {
                    System.Diagnostics.Debug.WriteLine($"[SERVER] Handler: {h.Info?.Id} (Info is null: {h.Info == null})");
                }

                var target = _handlers.FirstOrDefault(h => h.Info.Id == request.To);

                if (target == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SERVER] Target {request.To} not found");
                    await sender.SendAsync(new Log
                    {
                        From = "Server",
                        To = request.From,
                        Content = " ❌ Không tìm thấy đối tác"
                    });
                    return;
                }

                if (target.Info.Password != request.Password)
                {
                    System.Diagnostics.Debug.WriteLine($"[SERVER] Wrong password");
                    await sender.SendAsync(new Log
                    {
                        From = "Server",
                        To = request.From,
                        Content = " ❌ Sai mật khẩu"
                    });
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[SERVER] Creating connection mapping");
                //Thêm vào dictionary
                _activeConnection[request.From] = request.To;
                _activeConnection[request.To] = request.From;
                // Forward request tới target
                //await target.SendAsync(request);

                System.Diagnostics.Debug.WriteLine($"[SERVER] Sending success response");
                await sender.SendAsync(new Log
                {
                    From = "Server",
                    To = request.From,
                    Content = $" ✅ Đang kết nối tới {request.To}"
                });
                System.Diagnostics.Debug.WriteLine($"[SERVER] HandleConnectRequest completed successfully");

                await target.SendAsync(new Log
                {
                    From = request.From,
                    To = request.To,
                    Content = $" ✅ Đang được điều khiển bởi {request.From}"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SERVER ERROR] {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SERVER ERROR] {ex.StackTrace}");
                StatusChanged?.Invoke($"[Lỗi kết nối] {ex.Message}");
                try
                {
                    await sender.SendAsync(new Log
                    {
                        From = "Server",
                        To = request.From,
                        Content = " ❌ Lỗi kết nối"
                    });
                }
                catch (Exception sendEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[SERVER ERROR] Failed to send error response: {sendEx.Message}");
                }
            }
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
                    System.Diagnostics.Debug.WriteLine($"[SERVER] Forwarding Screen from {screenFrame.From} to {targetID}");
                    targetID = _activeConnection.GetValueOrDefault(screenFrame.From);
                    break;
                case Log log:
                    targetID = _activeConnection.GetValueOrDefault(log.From);
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
                await senderHandler.SendAsync(new Log
                {
                    From = "Server",
                    To = msg.From,
                    Content = "❌ Không tìm thấy đối tác"
                });
                return;
            }
        }
    }
}
