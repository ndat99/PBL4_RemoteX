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
using RemoteX.Core.Network;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

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
        public event Action<ClientInfo> ClientDisconnected; //Su kien khi co client ngat ket noi

        public event Action<string> StatusChanged; //Bao trang thai server

        //Khoi dong server
        public void Start(int port = 5000)
        {
            try
            {
                if (_isRunning) return;  //server da chay roi thi bo qua

                _listener = new TcpListener(IPAddress.Any, port); //Tao listener
                //Cho phep reuse address de tranh loi "Address already in use"
                _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
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

                    //Tao Thread rieng de xu ly client
                    Thread clientThread = new Thread(() => HandleClient(tcpClient));
                    clientThread.Start();
                }
                catch (SocketException)
                {
                    if (!_isRunning) break; //Neu server da tat thi thoat vong lap
                }
            }
        }

        private void HandleClient(TcpClient tcpClient)
        {
            try
            {
                var client = MessageReceiver.ReceiveClientInfo(tcpClient); //Nhan ClientInfo tu client

                lock (_clients)
                    _clients.Add(client); //Them client vao danh sach

                ClientConnected?.Invoke(client);
                //StatusChanged?.Invoke("Có client mới kết nối!");    //Bao ve UI

                //Thread lang nghe Client ket noi
                Thread listenThread = new Thread(() =>
                {
                    MessageReceiver.ListenForDisconnected(client, c =>
                    {
                        lock (_clients)
                            _clients.Remove(c); //Xoa client khoi danh sach
                        ClientDisconnected?.Invoke(c);
                    });
                });
                listenThread.Start();

                // Thread relay message
                Thread relayThread = new Thread(() =>
                {
                    Relay(client);
                });
                relayThread.Start();
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi khi xử lý client: {ex.Message}");
            }
        }

        //Trung gian lang nghe va gui message (chat)
        private void Relay(ClientInfo clientInfo)
        {
            MessageReceiver.ListenForMessages<ChatMessage>(
                clientInfo,
                onMessage =>
                {
                    //Tim Client dich trong danh sach
                    ClientInfo target;
                    lock (_clients)
                    {
                        target = _clients.FirstOrDefault(c => c.Id == onMessage.ReceiverID);
                    }

                    if (target != null && target.TcpClient.Connected)
                    {
                        MessageSender.Send(target.TcpClient, onMessage);
                    }
                    else
                    {
                        Console.WriteLine($"[Relay] Không tìm thấy client đích: {onMessage.ReceiverID}");

                    }
                },
                c =>
                {
                    lock (_clients) _clients.Remove(c);
                    ClientDisconnected?.Invoke(c);
                });

        }

        //Tat Server
        public void Stop()
        {
            if (!_isRunning) return; //Neu chua chay thi bo qua

            _isRunning = false;
            try
            {
            _listener.Stop(); //Ngung lang nghe
                if (_listenThread !=null && _listenThread.IsAlive)
                {
                    _listenThread.Join(500); //Chờ 500ms để thread kết thúc
                    if (_listenThread.IsAlive)
                    {
                       _listenThread.Interrupt(); //Buộc thread dừng nếu nó vẫn còn chạy
                    }
                }
                lock (_clients)
                {
                    foreach (var client in _clients.ToList())
                    {
                        client.TcpClient.Close();
                        ClientDisconnected?.Invoke(client);
                    }
                    _clients.Clear();
                }
                StatusChanged?.Invoke("Đã tắt Server");
            }

            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi khi tắt Server: {ex.Message}");
            }
            finally
            {
                _listener = null;
                _listenThread = null;
            }
        }

    }
}
