using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System.Net.Sockets;
using RemoteX.Core.Services;

namespace RemoteX.Client.Services
{
    public class ClientNetworkManager
    {
        private TcpClient _tcpClient;
        private ClientHandler _handler;
      
        public string ClientId { get; private set; }
        public TcpClient TcpClient => _tcpClient;

        //Events
        public event Action<string, System.Windows.Media.Brush> StatusChanged;
        public event Action<ClientInfo> ClientConnected;
        //public event Action<ConnectRequest> ConnectRequestReceived;
        public event Action<ChatMessage> ChatMessageReceived;
        public event Action<ScreenFrameMessage> ScreenFrameReceived;
        public event Action<Log> LogReceived;
        public event Action<FileMessage> FileMessageReceived;
        public event Action<FileChunk> FileChunkReceived;
        public event Action<FileAcceptMessage> FileAcceptReceived;
        public event Action<QualityChangeMessage> QualityChangeMessageReceived;
        //public event Action<MouseEventMessage> MouseEventReceived;
        //public event Action<KeyboardEventMessage> KeyboardEventReceived;

        public async Task ConnectAsync(string IP, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IP, port);

                int udpPort = port + 1;
                _handler = new ClientHandler(_tcpClient, IP, udpPort);

                //_handler.ConnectRequestReceived += request => ConnectRequestReceived?.Invoke(request);
                _handler.TcpMessageReceived += (sender, msg) => OnMessageReceived(msg);
                _handler.UdpMessageReceived += (sender, msg) => OnMessageReceived(msg);
                _handler.Disconnected += _ => StatusChanged?.Invoke("⬤ Mất kết nối server", System.Windows.Media.Brushes.Red);

                _handler.Start();
                //Gui info qua tcp
                var config = IdGenerator.RandomDeviceConfig(); //DÙNG ĐỂ TEST
                //var config = IdGenerator.DeviceConfig(); //DÙNG CHÍNH THỨC
                var info = new ClientInfo(config);

                _handler.Send(info);
                ClientId = info.Id;

                var udpInit = new ChatMessage
                {
                    From = ClientId,
                    To = "Server",
                    Message = "UDP_INIT", //Dùng để server biết port UDP của client
                    Timestamp = DateTime.Now
                };
                _handler.Send(udpInit);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ClientConnected?.Invoke(info);
                    StatusChanged?.Invoke($" ⬤  Đã kết nối Server {IP}:{port} (UDP:{udpPort})", System.Windows.Media.Brushes.Green);
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusChanged?.Invoke($" ⬤  Lỗi kết nối: {ex.Message}", System.Windows.Media.Brushes.Red);
                });
                throw; //ném lỗi cho ClientWindow bắt và thông báo
            }
        }
        private void OnMessageReceived(Core.Message msg)
        {
            switch (msg)
            {
                //tcp message
                case KeyboardEventMessage keyEvent:
                    System.Diagnostics.Debug.WriteLine($"[KEYBOARD RX] ClientNetwork received key: {keyEvent.KeyCode}, IsUp: {keyEvent.IsKeyUp}");
                    KeyboardService.ExecuteKeyboardEvent(keyEvent);
                    break;
                case Log log:
                    LogReceived?.Invoke(log);
                    break;
                case QualityChangeMessage qualityChange:
                    QualityChangeMessageReceived?.Invoke(qualityChange);
                    break;

                //udp message
                case MouseEventMessage mouseEvent:
                    MouseService.ExecuteMouseEvent(mouseEvent);
                    break;
                case ChatMessage chatMsg:
                    ChatMessageReceived?.Invoke(chatMsg);
                    break;
                case ScreenFrameMessage screenFrame:
                    ScreenFrameReceived?.Invoke(screenFrame);
                    break;
                case FileMessage fileMessage:
                    FileMessageReceived?.Invoke(fileMessage);
                    break;
                case FileChunk fileChunk:
                    FileChunkReceived?.Invoke(fileChunk);
                    break;
                case FileAcceptMessage fileAccept:
                    FileAcceptReceived?.Invoke(fileAccept);
                    break;

            }
        }
        public void Send(Core.Message msg) => _handler.Send(msg);
    }
}
