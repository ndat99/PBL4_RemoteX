using RemoteX.Core.Models;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Net;
using RemoteX.Core.Services;
using RemoteX.Core;
using RemoteX.Core.Networking;
using System.IO;
using System.Collections.Concurrent;

namespace RemoteX.Server.Controllers
{
    public class ServerController
    {
        private TcpListener _tcpListener;
        private UdpClient _udpListener;
        private Thread _tcpListenThread;
        private Thread _udpListenThread;
        private bool _isRunning;

        private readonly List<ClientHandler> _handlers = new();
        private readonly Dictionary<string, string> _activeConnection = new();
        private readonly ConcurrentDictionary<string, IPEndPoint> _clientUdpEndPoints = new(); //mapping từ ID -> IPEndPoint để UDP biết mà gửi
        private readonly object _lockObject = new object(); // Dùng _handlers làm lock object

        public ObservableCollection<ClientInfo> Clients { get; } = new();
        public event Action<string> StatusChanged;

        public void Start(int port)
        {
            try
            {
                if (_isRunning) return;

                _tcpListener = new TcpListener(IPAddress.Any, port);
                _tcpListener.Start();
                _udpListener = new UdpClient(port + 1);
                _isRunning = true;

                _tcpListenThread = new Thread(AcceptClients) { IsBackground = true };
                _tcpListenThread.Start();

                _udpListenThread = new Thread(AcceptUdpMessages) { IsBackground = true };
                _udpListenThread.Start();

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

            try
            {
                _tcpListener.Stop();
                _udpListener.Close();

                _tcpListenThread?.Join(1000);
                _udpListenThread?.Join(1000);
                // Close all handlers
                foreach (var handler in _handlers.ToList())
                {
                    try { handler.Close(); } catch { }
                }

                _handlers.Clear();
                _activeConnection.Clear();
                _clientUdpEndPoints.Clear();
                Clients.Clear();

                StatusChanged?.Invoke("Đã tắt Server");
            }
            catch (Exception e)
            {
                StatusChanged?.Invoke($"Stop error: {e.Message}");

            }
        }

        private async void AcceptUdpMessages()
        {
            while (_isRunning)
            {
                try
                {
                    var result = await _udpListener.ReceiveAsync();
                    var json = System.Text.Encoding.UTF8.GetString(result.Buffer);
                    var msg = MessageListener.Deserialize(json);
                    System.Diagnostics.Debug.WriteLine($"[UDP] Received from {result.RemoteEndPoint}: {msg.From} -> {msg.To}");

                    //lưu mapping ID -> IPEndPoint
                    if (!string.IsNullOrEmpty(msg.From))
                    {
                        _clientUdpEndPoints[msg.From] = result.RemoteEndPoint;
                    }
                    await ForwardUdpMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        System.Diagnostics.Debug.WriteLine($"[UDP Error] {ex.Message}");
                }
            }
        }

        private async Task ForwardUdpMessageAsync(Message msg)
        {
            string targetID = null;
            lock (_lockObject)
            {
                targetID = _activeConnection.GetValueOrDefault(msg.From);
            }
            if (targetID != null && _clientUdpEndPoints.TryGetValue(targetID, out IPEndPoint targetEndPoint))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[UDP] Forwarding to {targetID} at {targetEndPoint}");
                    await MessageSender.Send(_udpListener, targetEndPoint, msg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[UDP Forward Error] {ex.Message}");
                }
            }
        }
        private async Task SafeSendAsync(ClientHandler handler, Message message)
        {
            try
            {
                //Kiểm tra handler và connection còn tồn tại ko
                if (handler?.Client?.Connected == true)
                    //còn tồn tại connection thì mới gửi message
                    await handler.SendAsync(message);
            }
            catch (Exception e) when (e is IOException || e is SocketException || e is ObjectDisposedException)
            {
                System.Diagnostics.Debug.WriteLine($"[Network Error] Failed to send to {handler?.Info?.Id} : {e.Message}");
            }
        }

        private async void AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                var serverPort = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
                var udpPort = serverPort + 1;
            
                var handler = new ClientHandler(tcpClient, "127.0.0.1", udpPort);
                    // Khi client ngắt kết nối, xóa nó khỏi danh sách
                    handler.Disconnected += async disconnectedHandler =>
                    {
                        try
                        {
                            var id = disconnectedHandler.Info?.Id;
                            if (string.IsNullOrEmpty(id)) return;

                            ClientHandler partnerHandler = null;
                            string partnerID = null;

                            // lock toàn bộ block thay vì chỉ _handlers
                            lock (_lockObject)
                            {
                                _handlers.Remove(disconnectedHandler);

                                if (_activeConnection.TryGetValue(id, out partnerID))
                                {
                                    partnerHandler = _handlers.FirstOrDefault(x => x.Info?.Id == partnerID);
                                    _activeConnection.Remove(id);
                                    _activeConnection.Remove(partnerID);
                                }
                            }

                            _clientUdpEndPoints.TryRemove(id, out _);
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                var clientToRemove = Clients.FirstOrDefault(c => c.Id == id);
                                if (clientToRemove != null)
                                    Clients.Remove(clientToRemove);
                            });

                            if (partnerHandler != null)
                            {
                                try
                                {
                                    await SafeSendAsync(partnerHandler, new Log
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
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Disconnect Handler Error] {e}");
                            StatusChanged?.Invoke($"[Disconnect Error] {e.Message}");
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



                    //handler.ChatMessageReceived += async chatMsg =>
                    //{
                    //    await ForwardMessageAsync(chatMsg);
                    //};

                    //handler.ScreenFrameReceived += async screenMsg =>
                    //{
                    //    await ForwardMessageAsync(screenMsg);
                    //};

                    lock (_lockObject)
                    {
                    _handlers.Add(handler);
                    }
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
            // Nếu client dừng điều khiển thì báo về trạng thái như cũ
            if (request.Status == "Disconnect")
            {
                lock (_lockObject)
                {
                    if (_activeConnection.ContainsKey(request.From))
                    {
                        var partnerId = _activeConnection[request.From];
                        _activeConnection.Remove(request.From);
                        _activeConnection.Remove(partnerId);
                    }
                }

                await sender.SendAsync(new Log
                {
                    From = "Server",
                    To = request.From,
                    Content = " ⬤  Đã kết nối tới Server"
                });

                return;
            }

            ClientHandler target = null;
            bool passwordValid = false;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[SERVER] Processing ConnectRequest from {request.From} to {request.To}");

                lock (_lockObject)
                {
                    System.Diagnostics.Debug.WriteLine($"[SERVER] Total handlers: {_handlers.Count}");

                    // Tìm target và validate trong lock
                    target = _handlers.FirstOrDefault(h => h.Info?.Id == request.To);

                    if (target != null && target.Info.Password == request.Password)
                    {
                        passwordValid = true;
                        // Tạo connection mapping trong lock
                        _activeConnection[request.From] = request.To;
                        _activeConnection[request.To] = request.From;
                        System.Diagnostics.Debug.WriteLine($"[SERVER] Creating connection mapping");
                    }
                }

                // Validation và gửi messages ngoài lock
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

                if (!passwordValid)
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

                System.Diagnostics.Debug.WriteLine($"[SERVER] Sending success response");
                await SafeSendAsync(sender, new Log
                {
                    From = "Server",
                    To = request.From,
                    Content = $" ✔ Đang kết nối tới {request.To}"
                });

                await SafeSendAsync(target, new Log
                {
                    From = request.From,
                    To = request.To,
                    Content = $" ✔ Đang được điều khiển bởi {request.From}"
                });

                System.Diagnostics.Debug.WriteLine($"[SERVER] HandleConnectRequest completed successfully");
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

        public async Task ForwardTcpMessageAsync(Message msg)
        {
            string targetID = null;
            ClientHandler targetHandler = null;
            ClientHandler senderHandler = null;

            lock (_lockObject)
            {
                switch (msg)
                {
                    //case ChatMessage chat:
                    //    targetID = _activeConnection.GetValueOrDefault(chat.From);
                    //    break;
                    //case ScreenFrameMessage screenFrame:
                    //    targetID = _activeConnection.GetValueOrDefault(screenFrame.From);
                    //    System.Diagnostics.Debug.WriteLine($"[SERVER] Forwarding Screen from {screenFrame.From} to {targetID}");
                    //    break;
                    case Log log:
                        targetID = _activeConnection.GetValueOrDefault(log.From);
                        break;
                    default:
                        return;
                }

                if (targetID != null)
                {
                    targetHandler = _handlers.FirstOrDefault(h => h.Info?.Id == targetID);
                }
                else
                {
                    // Tìm sender handler để gửi error message
                    var senderID = _activeConnection.GetValueOrDefault(msg.To);
                    if (!string.IsNullOrEmpty(senderID))
                    {
                        senderHandler = _handlers.FirstOrDefault(h => h.Info?.Id == senderID);
                    }
                }
            }

            // Send messages ngoài lock
            if (targetHandler != null)
            {
                try
                {
                    await SafeSendAsync(targetHandler, msg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Forward Error] {ex.Message}");
                }
            }
            else if (senderHandler != null)
            {
                try
                {
                    await SafeSendAsync(senderHandler, new Log
                    {
                        From = "Server",
                        To = msg.From,
                        Content = "❌ Không tìm thấy đối tác"
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error Send Failed] {ex.Message}");
                }
            }
        }
    }
}
