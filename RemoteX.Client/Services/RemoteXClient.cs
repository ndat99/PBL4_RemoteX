using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;

namespace RemoteX.Client.Services
{
    public class RemoteXClient
    {
        private TcpClient _client;
        private ClientInfo _clientInfo;
        private DeviceConfig _deviceConfig;
        public event Action<string> StatusChanged; //Bao trang thai cho UI Client
        public event Action<ClientInfo> ClientConnected; //Su kien khi ket noi thanh cong
        
        // Đọc hoặc tạo mới config khi app mở

        public RemoteXClient()
        {

        }
        public void Connect(string serverIp, int port)
        {
            try
            {
                //Thread.Sleep(10000);
                _client = new TcpClient();
                _client.Connect(serverIp, port);
                //_clientInfo = new ClientInfo(_client);
                var config = IdGenerator.DeviceConfig();
                _clientInfo = new ClientInfo (_client)
                {
                    Id = config.DeviceID,
                    Password = config.Password,
                    MachineName = config.MachineName,
                };

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
