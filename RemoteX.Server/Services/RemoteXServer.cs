using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Server.ViewModels;
using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;

namespace RemoteX.Server.Services
{
    public class RemoteXServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private ServerViewModel _svm;

        public event Action<ClientInfo> ClientConnected;
        //public event Action<string> StatusChanged;

        public void Start(int port = 4000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;
            //_svm.Status = $"Server đang chạy trên cổng {port}";

            Task.Run(() => AcceptClients());
            //StatusChanged?.Invoke($"Server đang lắng nghe tại cổng {port}...");
            //_listener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        private async Task AcceptClients()
        {
            while (_isRunning)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();

                string ip = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

                var clientInfo = new ClientInfo
                {
                    Id = IdGenerator.GenerateId(),
                    Password = PasswordGenerator.GeneratePassword(),
                    IpAddress = ip,
                    IsConnected = true
                    //TcpClient = client,
                };

                ClientConnected?.Invoke(clientInfo);
            }
        }

        //private void OnClientConnected(IAsyncResult ar)
        //{
        //    var client = _listener.EndAcceptTcpClient(ar);

        //    string clientId = IdGenerator.GenerateId();
        //    string password = PasswordGenerator.GeneratePassword();

        //    var clientInfo = new ClientInfo
        //    {
        //        Id = clientId,
        //        Password = password,
        //        TcpClient = client,
        //    };

        //    var stream = client.GetStream();
        //    string authMsg = $"{clientId}|{password}";
        //    byte[] buffer = Encoding.UTF8.GetBytes(authMsg);
        //    stream.Write(buffer, 0, buffer.Length);

        //    ClientConnected?.Invoke(clientInfo);

        //    _listener.BeginAcceptTcpClient(OnClientConnected, null);
        //}
    }
}
