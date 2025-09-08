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
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using RemoteX.Shared.Utils;
using System.Windows;

namespace RemoteX.Server.Services
{
    public class RemoteXServer
    {
        private TcpListener _listener; //Lang nghe ket noi TCP
        private Thread _listenThread; //Thread de chay server song song
        private bool _isRunning;
        public bool IsRunning => _isRunning;

        //private List<ClientInfo> _clients = new List<ClientInfo>(); //Ds client da ket noi
        private ClientManager _clientManager;
        public event Action<ClientInfo> ClientConnected; //Su kien khi co client ket noi
        public event Action<ClientInfo> ClientDisconnected; //Su kien khi co client ngat ket noi

        public event Action<string> StatusChanged; //Bao trang thai server

        //Khoi dong server
        public void Start(int port = 5000)
        {
            try
            {
                if (_isRunning) return;  //server da chay roi thi bo qua

                _clientManager = new ClientManager(); //Khoi tao quan ly client
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
                var client = NetworkHelper.ReceiveClientInfo(tcpClient); //Nhan ClientInfo tu client

                _clientManager.AddClient(client); //Them client vao danh sach quan ly

                ClientConnected?.Invoke(client);
                //StatusChanged?.Invoke("Có client mới kết nối!");    //Bao ve UI

                //Thread lang nghe Client ngat ket noi
                Thread listenThread = new Thread(() =>
                {
                    NetworkHelper.ListenForDisconnected(client, c =>
                    {
                        _clientManager.RemoveClient(c);
                        ClientDisconnected?.Invoke(c);
                    });
                });
                listenThread.Start();

                //Thread lang nghe ConnectRequest giua cac client
                Thread connectThread = new Thread(() =>
                {
                    NetworkHelper.ListenForMessages<ConnectRequest>(client, request =>
                    {
                        bool success = _clientManager.TryMapClients(request.SenderID, request.TargetID, request.Password);

                        if (!success)
                        {
                            //Bao loi cho nguoi gui (A)
                            var failMsg = new ChatMessage
                            {
                                SenderID = "Server",
                                ReceiverID = request.SenderID,
                                Message = "Kết nối thất bại: Sai ID hoặc Pass.",
                                IsMine = false
                            };
                            NetworkHelper.SendMessage(client.TcpClient, failMsg);
                        }
                        else
                        {
                            //Neu thanh cong thi bao lai cho A

                            var sender = _clientManager.FindById(request.SenderID);
                            var partner = _clientManager.FindById(request.TargetID);

                            if (partner != null && sender != null)
                            {
                                var okMsgA = new ChatMessage
                                {
                                    SenderID = "Server",
                                    ReceiverID = request.SenderID,
                                    Message = $"Kết nối tới {partner.MachineName} thành công!",
                                    IsMine = false
                                };
                                NetworkHelper.SendMessage(client.TcpClient, okMsgA);

                                // Báo cho B biết đã được kết nối
                                var okMsgB = new ChatMessage
                                {
                                    SenderID = "Server",
                                    ReceiverID = partner.Id,  // B
                                    Message = $"Bạn đã được kết nối với {sender.MachineName}!",
                                    IsMine = false
                                };
                                NetworkHelper.SendMessage(partner.TcpClient, okMsgB);
                            }
                        }
                    });
                });
                connectThread.Start();

                // Thread relay message
                Thread relayThread = new Thread(() =>
                {
                    var relay = new MessageRelay(_clientManager);

                    //Lang nghe tat ca tin nhan ChatMessage tu client
                    NetworkHelper.ListenForMessages<ChatMessage>(client, message =>
                    {
                        relay.RelayMessage(message); //Chuyen tin nhan den client dich
                    });
                });
                relayThread.Start();
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Lỗi khi xử lý client: {ex.Message}");
            }
        }

        //Tat Server
        public void Stop()
        {
            if (!_isRunning) return; //Neu chua chay thi bo qua

            _isRunning = false;
            try
            {
                _listener.Stop(); //Ngung lang nghe
                if (_listenThread != null && _listenThread.IsAlive)
                {
                    _listenThread.Join(500); //Chờ 500ms để thread kết thúc
                    if (_listenThread.IsAlive)
                    {
                        _listenThread.Interrupt(); //Buộc thread dừng nếu nó vẫn còn chạy
                    }
                }

                var allClients = _clientManager.GetAllClients().ToList();
                foreach (var client in allClients)
                {
                    try
                    {
                        client.TcpClient.Close();
                        ClientDisconnected?.Invoke(client);
                    }
                    catch { }
                }

                _clientManager.Clear();
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