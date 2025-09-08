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

namespace RemoteX.Client.Controllers
{
    public class ClientController
    {
        private TcpClient _client;
        public event Action<string, Brush> StatusChanged; 
        //public event Action<ClientInfo> InfoReceived;
        public event Action<ClientInfo> ClientConnected;

        public void Connect(string IP, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(IP, port);


                var config = IdGenerator.DeviceConfig();
                var info = new ClientInfo(config);

                MessageSender.Send(_client, info, MessageType.ClientInfo);

                ClientConnected?.Invoke(info);
                StatusChanged?.Invoke($" ⬤  Đã kết nối Server {IP}:{port}", Brushes.Green);
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($" ⬤  Lỗi kết nối: {ex.Message}", Brushes.Red);
            }
        }
    }
}
