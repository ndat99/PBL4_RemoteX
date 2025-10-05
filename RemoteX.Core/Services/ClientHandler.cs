using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using System.Net;
using System.Net.Sockets;

namespace RemoteX.Core.Services
{
    //Giữ TcpClient, MessageListener và ClientInfo của 1 client
    //ServerController quản lý list ClientHandler
    //Client disconnected -> báo event về ServerController -> remove khỏi list
    public class ClientHandler
    {
        private readonly TcpClient _tcpClient;
        private readonly UdpClient _udpClient;
        private readonly MessageListener _tcpListener;
        private readonly MessageListener _udpListener;
        private readonly IPEndPoint _serverUdpEndpoint;
        public TcpClient Client => _tcpClient;
        public ClientInfo Info { get; private set; }

        //Events
        public event Action<ClientHandler> Disconnected;
        public event Action<ClientInfo> ClientInfoReceived;
        public event Action<ChatMessage> ChatMessageReceived;
        public event Action<ScreenFrameMessage> ScreenFrameReceived;
        public event Action<ConnectRequest> ConnectRequestReceived;
        public event Action<Log> LogReceived;
        public ClientHandler(TcpClient client, string serverIP, int udpPort)
        {
            _tcpClient = client;
            _udpClient = new UdpClient(0); //random port

            //var remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            _serverUdpEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), udpPort);

            // tcp listener
            _tcpListener = new MessageListener(_tcpClient);
            _tcpListener.MessageReceived += OnTcpMessageReceived;
            _tcpListener.ClientDisconnected += () => Disconnected?.Invoke(this);

            //udp listener
            _udpListener = new MessageListener(_udpClient);
            _udpListener.MessageReceived += OnUdpMessageReceived;
            _udpListener.ClientDisconnected += () => Disconnected?.Invoke(this);
        }

        private void OnTcpMessageReceived(Message msg)
        {
            switch (msg)
            {
                case ClientInfo clientInfoMsg:
                    Info = clientInfoMsg;
                    ClientInfoReceived?.Invoke(Info);
                    break;

                case ConnectRequest connectRequest:
                    ConnectRequestReceived?.Invoke(connectRequest);
                    break;
                case Log logMsg:
                    LogReceived?.Invoke(logMsg);
                    break;

                default:
                    break;
            }
        }

        private void OnUdpMessageReceived(Message msg)
        {
            switch (msg)
            {
                case ChatMessage chatMsg:
                    ChatMessageReceived?.Invoke(chatMsg);
                    break;

                case ScreenFrameMessage frameMsg:
                    ScreenFrameReceived?.Invoke(frameMsg);
                    break;
                default:
                    break;
            }
        }

        public Task StartAsync()
        {
            //start cả 2 listener
            _= _tcpListener.StartAsync();
            _= _udpListener.StartAsync();
            return Task.CompletedTask;
        }

        public async Task SendAsync(Message msg)
        {
            //Kiểm tra xem client còn connect ko
            if (_tcpClient?.Connected != true)
            {
                System.Diagnostics.Debug.WriteLine($"[ClientHandler] Cannot send - client {Info?.Id} is not connected");
                return; //ko còn thì ko gửi
            }

            try
            {
                if (msg is ChatMessage || msg is ScreenFrameMessage)
                {
                    System.Diagnostics.Debug.WriteLine($"[ClientHandler] Sending UDP: {msg.GetType().Name} from {msg.From} to {msg.To}");
                    await MessageSender.Send(_udpClient, _serverUdpEndpoint, msg);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ClientHandler] Sending TCP: {msg.GetType().Name} from {msg.From} to {msg.To}");
                    await MessageSender.Send(_tcpClient, msg);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is SocketException || ex is ObjectDisposedException)
            {
                System.Diagnostics.Debug.WriteLine($"[ClientHandler] Send failed for {Info?.Id}: {ex.Message}");
            }
        }
        public void Close()
        {
            _udpListener?.Stop();

            _tcpClient.Close();
            _udpClient.Close();
        }
    }
}