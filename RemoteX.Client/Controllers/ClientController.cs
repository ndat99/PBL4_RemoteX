using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System.Net.Sockets;
using RemoteX.Core.Services;

namespace RemoteX.Client.Controllers
{
    public class ClientController
    {
        private TcpClient _tcpClient;
        private ClientHandler _handler;

        //Udp rieng cho screen + info server
        private UdpClient _udp;  //Socket UDP dung gui chunk nhi phan
        public UdpClient UdpClient => _udp;
        public string ServerIP { get; set; }
        public string ServerPort { get; set; }

        public string ClientId { get; private set; }
        public TcpClient TcpClient => _tcpClient;

        //Events
        public event Action<string, System.Windows.Media.Brush> StatusChanged;
        public event Action<ClientInfo> ClientConnected;
        public event Action<ConnectRequest> ConnectRequestReceived;
        public event Action<ChatMessage> ChatMessageReceived;
        public event Action<ScreenFrameMessage> ScreenFrameReceived;
        public event Action<Log> LogReceived;

        public async Task Connect(string IP, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IP, port);

                int udpPort = port + 1;
                _handler = new ClientHandler(_tcpClient, IP, udpPort);

                //_handler.ConnectRequestReceived += request => ConnectRequestReceived?.Invoke(request);
                _handler.ChatMessageReceived += chatMsg => ChatMessageReceived?.Invoke(chatMsg);
                _handler.ScreenFrameReceived += screen => ScreenFrameReceived?.Invoke(screen);
                _handler.LogReceived += log => LogReceived?.Invoke(log);
                _handler.Disconnected += _ => StatusChanged?.Invoke("⬤ Mất kết nối server", System.Windows.Media.Brushes.Red);

                _ = _handler.StartAsync();
                //Gui info qua tcp
                var config = IdGenerator.RandomDeviceConfig(); //DÙNG ĐỂ TEST
                //var config = IdGenerator.DeviceConfig(); //DÙNG CHÍNH THỨC
                var info = new ClientInfo(config);

                await _handler.SendAsync(info);
                ClientId = info.Id;

                await Task.Delay(100); //đợi tcp gửi clientInfo xong
                var udpInit = new ChatMessage
                {
                    From = ClientId,
                    To = "Server",
                    Message = "UDP_INIT", //Dùng để server biết port UDP của client
                    Timestamp = DateTime.Now
                };
                await _handler.SendAsync(udpInit);

                ClientConnected?.Invoke(info);
                StatusChanged?.Invoke($" ⬤  Đã kết nối Server {IP}:{port} (UDP:{udpPort})", System.Windows.Media.Brushes.Green);
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($" ⬤  Lỗi kết nối: {ex.Message}", System.Windows.Media.Brushes.Red);
            }
        }
        public Task SendAsync(RemoteX.Core.Message msg) => _handler.SendAsync(msg);
    }
}
