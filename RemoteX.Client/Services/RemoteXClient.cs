using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RemoteX.Shared.Models;

namespace RemoteX.Client.Services
{
    public class RemoteXClient
    {
        private TcpClient _client;
        private ClientInfo _clientInfo;
        public event Action<string> StatusChanged; //Bao trang thai cho UI Client
        public event Action<ClientInfo> ClientConnected; //Su kien khi ket noi thanh cong
        public void Connect(string serverIp, int port)
        {
            try
            {
                //Thread.Sleep(10000);
                _client = new TcpClient();
                _client.Connect(serverIp, port);
                _clientInfo = new ClientInfo(_client);

                //Gui thong tin Client cho Server
                var stream = _client.GetStream();
                string json = System.Text.Json.JsonSerializer.Serialize(_clientInfo); //Chuyen thanh chuoi JSON
                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);

                StatusChanged?.Invoke($" ⬤  Đã kết nối tới server {serverIp}:{port}");
                ClientConnected?.Invoke(_clientInfo);
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi kết nối");
                MessageBox.Show(" ⬤  Lỗi kết nối: " + ex.Message);
            }
        }
    }
}
