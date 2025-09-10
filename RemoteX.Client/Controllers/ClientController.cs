using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using RemoteX.Core.Utils;
using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Net.Http;
using RemoteX.Core.Services;
using RemoteX.Core;

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
        //public event Action<ClientInfo> SelfInfoReady;
        public event Action<ClientInfo> ClientConnected;
        public event Action<ConnectRequest> ConnectRequestReceived;
        public event Action<ChatMessage> ChatMessageReceived;
        public event Action<ScreenFrameMessage> ScreenFrameReceived;

        public async void Connect(string IP, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IP, port);

                _handler = new ClientHandler(_tcpClient);

                _handler.ConnectRequestReceived += request => ConnectRequestReceived?.Invoke(request);
                _handler.ChatMessageReceived += chatMsg => ChatMessageReceived?.Invoke(chatMsg);
                _handler.ScreenFrameReceived += screen => ScreenFrameReceived?.Invoke(screen);
                _handler.Disconnected += _ => StatusChanged?.Invoke("⬤ Mất kết nối server", System.Windows.Media.Brushes.Red);

                //Gui info
                var config = IdGenerator.RandomDeviceConfig(); //DÙNG ĐỂ TEST
                //var config = IdGenerator.DeviceConfig(); //DÙNG CHÍNH THỨC
                var info = new ClientInfo(config);

                await _handler.SendAsync(info);
                _ = _handler.StartAsync();

                ClientId = info.Id;

                ClientConnected?.Invoke(info);
                StatusChanged?.Invoke($" ⬤  Đã kết nối Server {IP}:{port}", System.Windows.Media.Brushes.Green);
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($" ⬤  Lỗi kết nối: {ex.Message}", System.Windows.Media.Brushes.Red);
            }
        }
        public Task SendAsync(RemoteX.Core.Message msg) => _handler.SendAsync(msg);
    }
}
