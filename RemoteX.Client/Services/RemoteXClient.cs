using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Client.Services
{
    public class RemoteXClient
    {
        private TcpClient _client;
        public event Action<string> StatusChanged; //Bao trang thai cho UI Client
        public void Connect(string serverIp, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(serverIp, port);

                StatusChanged?.Invoke($"Đã kết nối tới server {serverIp}:{port}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi kết nối");
            }
        }
    }
}
