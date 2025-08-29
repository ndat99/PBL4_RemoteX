using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq.Expressions;
using RemoteX.Shared.Models; //dùng ClientInfo

namespace RemoteX.Server.Services
{
    public class RemoteXServer
    {
        private TcpListener _listener; //Lang nghe ket noi TCP
        private Thread _listenThread; //Thread de chay server song song
        private bool _isRunning;
        public bool IsRunning => _isRunning;

        private List<ClientInfo> _clients = new List<ClientInfo>(); //Ds client da ket noi
        public event Action<ClientInfo> ClientConnected; //Su kien khi co client ket noi

        public event Action<string> StatusChanged; //Bao trang thai server

        //Khoi dong server
        public void Start(int port = 5000)
        {
            try
            {
                if (_isRunning) return;  //server da chay roi thi bo qua

                _listener = new TcpListener(IPAddress.Any, port); //Tao listener
                _listener.Start(); //Bat dau lang nghe
                _isRunning = true;

                _listenThread = new Thread(ListenForClients); //Tao luong lang nghe
                _listenThread.Start();

                StatusChanged?.Invoke($"Server đang chạy trên cổng {port}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi: {ex.Message}");
            }
        }

        private void ListenForClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient tcpClient = _listener.AcceptTcpClient(); //Cho ket noi tu client moi
                    ClientInfo client = new ClientInfo(); //Tao client info ngau nhien
                    _clients.Add(client); //Them client vao danh sach

                    ClientConnected?.Invoke(client);
                    StatusChanged?.Invoke("Có client mới kết nối!");    //Bao ve UI
                    //more
                }
                catch (SocketException)
                {
                }
            }
        }

        //Tat Server
        public void Stop()
        {
            if (!_isRunning) return; //Neu chua chay thi bo qua

            _isRunning = false;
            _listener.Stop(); //Ngung lang nghe
            StatusChanged?.Invoke("Đã tắt Server");   
        }

    }
}
