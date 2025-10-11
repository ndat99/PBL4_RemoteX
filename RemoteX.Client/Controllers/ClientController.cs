using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System.Net.Sockets;
using RemoteX.Core.Services;
using RemoteX.Client.Services;

namespace RemoteX.Client.Controllers
{
    public class ClientController
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
        //public event Action<MouseEventMessage> MouseEventReceived;
        //public event Action<KeyboardEventMessage> KeyboardEventReceived;

        public void Connect(string IP, int port)
        {
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(IP, port);

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

                    Thread.Sleep(100); //đợi tcp gửi clientInfo xong
                    var udpInit = new ChatMessage
                    {
                        From = ClientId,
                        To = "Server",
                        Message = "UDP_INIT", //Dùng để server biết port UDP của client
                        Timestamp = DateTime.Now
                    };
                    _handler.Send(udpInit);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ClientConnected?.Invoke(info);
                        StatusChanged?.Invoke($" ⬤  Đã kết nối Server {IP}:{port} (UDP:{udpPort})", System.Windows.Media.Brushes.Green);
                    });
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        StatusChanged?.Invoke($" ⬤  Lỗi kết nối: {ex.Message}", System.Windows.Media.Brushes.Red);
                    });
                }
            });

            connectThread.IsBackground = true; //để luồng tự tắt khi thoát ứng dụng
            connectThread.Start();
        }
        private void OnMessageReceived(RemoteX.Core.Message msg)
        {
            switch (msg)
            {
                //tcp message
                case KeyboardEventMessage keyEvent:
                    System.Diagnostics.Debug.WriteLine($"[KEYBOARD RX] ClientController received key: {keyEvent.KeyCode}, IsUp: {keyEvent.IsKeyUp}");
                    KeyboardService.ExecuteKeyboardEvent(keyEvent);
                    break;
                case Log log:
                    LogReceived?.Invoke(log);
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
            }
        }
        public void Send(RemoteX.Core.Message msg) => _handler.Send(msg);
    }
}
