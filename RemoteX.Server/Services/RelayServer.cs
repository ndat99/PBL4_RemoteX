using RemoteX.Core.Models;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Net;
using RemoteX.Core.Services;
using RemoteX.Core;
using RemoteX.Core.Networking;
using System.IO;
using System.Collections.Concurrent;

namespace RemoteX.Server.Services
{
    public class RelayServer
    {
        private TcpListener _tcpListener;
        private UdpClient _udpListener;
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

                Task.Run(AcceptClients);
                AcceptUdpMessages(); //gọi trực tiếp async (fire-and-forget), nó sẽ tự chạy trên một luồng khác

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
                _udpListener.Close(); //làm ReceiveAsync văng lỗi

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
            System.Diagnostics.Debug.WriteLine($"[UDP] Listener started on port {((IPEndPoint)_udpListener.Client.LocalEndPoint).Port}");

            while (_isRunning)
            {
                try
                {
                    UdpReceiveResult receiveResult = await _udpListener.ReceiveAsync(); //luồng sẽ đc giải phóng tại đây khi chờ message
                    
                    byte[] buffer = receiveResult.Buffer;
                    var remoteEP = receiveResult.RemoteEndPoint;
                    
                    var json = System.Text.Encoding.UTF8.GetString(buffer);
                    var msg = MessageListener.Deserialize(json);
                    System.Diagnostics.Debug.WriteLine($"[UDP RX] ✅ From {remoteEP}: Type={msg.Type}, From={msg.From}, To={msg.To}");

                    //lưu mapping ID -> IPEndPoint
                    if (!string.IsNullOrEmpty(msg.From))
                    {
                        _clientUdpEndPoints[msg.From] = remoteEP;
                        System.Diagnostics.Debug.WriteLine($"[UDP] Registered endpoint for {msg.From}: {remoteEP}");
                    }
                    ForwardUdpMessage(msg);
                }
                catch (ObjectDisposedException)
                {
                    //bắt lỗi khi _udpListener.Close() được gọi
                    if (_isRunning) break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        System.Diagnostics.Debug.WriteLine($"[UDP Error] {ex.Message}");
                }
            }
        }

        private void ForwardUdpMessage(Message msg)
        {
            if (msg is ChatMessage chat && chat.Message == "UDP_INIT")
            {
                //Dùng để client gửi port udp lên server
                System.Diagnostics.Debug.WriteLine($"[UDP] Received UDP_INIT from {chat.From}");
                return;
            }
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
                    MessageSender.Send(_udpListener, targetEndPoint, msg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[UDP Forward Error] {ex.Message}");
                }
            }
        }
        private void SafeSend(ClientHandler handler, Message message)
        {
            try
            {
                //Kiểm tra handler và connection còn tồn tại ko
                if (handler?.Client?.Connected == true)
                    //còn tồn tại connection thì mới gửi message
                    handler.Send(message);
            }
            catch (Exception e) when (e is IOException || e is SocketException || e is ObjectDisposedException)
            {
                System.Diagnostics.Debug.WriteLine($"[Network Error] Failed to send to {handler?.Info?.Id} : {e.Message}");
            }
        }

        private void AcceptClients()
        {
            while (_isRunning)
            {
                try
                    {
                    var tcpClient = _tcpListener.AcceptTcpClient();

                    var localEndpoint = (IPEndPoint)tcpClient.Client.LocalEndPoint;
                    string serverIP = localEndpoint.Address.ToString();
                    var serverPort = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
                    int udpPort = serverPort + 1;

                    if (serverIP == "0.0.0.0" || serverIP == "::")
                        serverIP = "127.0.0.1"; //nếu bind Any thì đổi thành localhost
                    System.Diagnostics.Debug.WriteLine($"[SERVER] Client connected, using server IP: {serverIP} for UDP");

                    var handler = new ClientHandler(tcpClient, serverIP, udpPort);
                    handler.Disconnected += OnClientDisconnected;
                    handler.TcpMessageReceived += OnTcpMessageReceived;

                    lock (_lockObject)
                    {
                        _handlers.Add(handler);
                    }
                    handler.Start();
                }
                catch(Exception ex)
                {
                    if (_isRunning)
                        StatusChanged?.Invoke($"[Accept Error] {ex.Message}");
                }
            }
        }
        private void OnClientDisconnected(ClientHandler disconnectedHandler)
        {
            try
            {
                var id = disconnectedHandler.Info?.Id;
                if (string.IsNullOrEmpty(id)) return;

                ClientHandler partnerHandler = null;
                string partnerID = null;

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
                    if (clientToRemove != null) Clients.Remove(clientToRemove);
                });

                if (partnerHandler != null)
                {
                    SafeSend(partnerHandler, new Log
                    {
                        From = "Server",
                        To = partnerID,
                        Content = "❌ Đối tác đã ngắt kết nối"
                    });
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[Disconnect Handler Error] {e}");
            }
        }

        private void OnTcpMessageReceived(ClientHandler sender, Message message)
        {
            switch (message)
            {
                case ClientInfo info:
                    App.Current.Dispatcher.Invoke(() => Clients.Add(info));
                    break;
                case ConnectRequest connectRequest:
                    Task.Run(() => HandleConnectRequest(sender, connectRequest));
                    break;
                case KeyboardEventMessage keyMsg:
                    ForwardTcpMessage(sender, keyMsg);
                    break;
                case Log log:
                    ForwardTcpMessage(sender,log);
                    break;
                case FileMessage fileMsg:
                case FileChunk fileChunk:
                    ForwardTcpMessage(sender, message);
                    break;
                case FileAcceptMessage fileAccept:
                    ForwardTcpMessage(sender, fileAccept);
                    break;
                case QualityChangeMessage qualityChange:
                    ForwardTcpMessage(sender, qualityChange);
                    break;
            }
        }

        public void ForwardTcpMessage(ClientHandler sender, Message msg)
        {
            string targetID = null;
            ClientHandler targetHandler = null;

            lock (_lockObject)
            {
                targetID = _activeConnection.GetValueOrDefault(msg.From);
                if (targetID != null)
                {
                    targetHandler = _handlers.FirstOrDefault(h => h.Info?.Id == targetID);
                }
            }

            // Send messages ngoài lock
            if (targetHandler != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[TCP FORWARD] Forwarding KeyboardEvent from {msg.From} to {targetID}");
                    SafeSend(targetHandler, msg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Forward Error] {ex.Message}");
                }
            }
            else
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[TCP FORWARD] Target not found for {msg.From}. Sending error back.");
                    SafeSend(sender, new Log
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

        private void HandleConnectRequest(ClientHandler sender, ConnectRequest request)
        {
            // Nếu client dừng điều khiển thì báo về trạng thái như cũ
            if (request.Status == "Disconnect")
            {
                string partnerId = null;
                ClientHandler partnerHandler = null; //giữ handler của phía bị điều khiển (Client B)
                lock (_lockObject)
                {
                    if (_activeConnection.TryGetValue(request.From, out partnerId)) //lấy ID của B
                    {
                        //Tìm handler của B (người đang stream)
                        partnerHandler = _handlers.FirstOrDefault(h => h.Info?.Id == partnerId);

                        //Xóa kết nối
                        _activeConnection.Remove(request.From);
                        _activeConnection.Remove(partnerId);
                    }
                }

                //gửi log cho A
                sender.Send(new Log
                {
                    From = "Server",
                    To = request.From,
                    Content = " ⬤  Đã kết nối tới Server"
                });

                //gửi log cho B
                if (partnerHandler != null)
                {
                    SafeSend(partnerHandler, new Log
                    {
                        From = "Server",
                        To = partnerId,
                        Content = "❌ Đối tác đã ngắt kết nối"
                    });
                }

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
                    sender.Send(new Log
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
                    sender.Send(new Log
                    {
                        From = "Server",
                        To = request.From,
                        Content = " ❌ Sai mật khẩu"
                    });
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[SERVER] Sending success response");
                SafeSend(sender, new Log
                {
                    From = "Server",
                    To = request.From,
                    Content = $" ✔ Đang kết nối tới {request.To}"
                });

                SafeSend(target, new Log
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
                    sender.Send(new Log
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
    }
}
