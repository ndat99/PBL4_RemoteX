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

                var config = IdGenerator.RandomDeviceConfig(); //DÙNG ĐỂ DEBUG, SAU NÀY XÓA

                //var config = IdGenerator.DeviceConfig(); //NHỚ GIỮ LẠI SAU CÒN DÙNG
                _clientInfo = new ClientInfo (_client)
                {
                    Id = config.DeviceID,
                    Password = config.Password,
                    MachineName = config.MachineName,
                };

                NetworkHelper.SendClientInfo(_client, _clientInfo);

                StatusChanged?.Invoke($" ⬤  Đã kết nối tới server {serverIp}:{port}");
                ClientConnected?.Invoke(_clientInfo);
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($" ⬤  Lỗi kết nối: {ex.Message}");
                MessageBox.Show(" ⬤  Lỗi kết nối: " + ex.Message);
            }
        }
    }
}
