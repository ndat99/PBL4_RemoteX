using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using System.Net;
using System.Net.Sockets;

namespace RemoteX.Core.Services
{
    //Giữ TcpClient, MessageListener và ClientInfo của 1 client
    //RelayServer quản lý list ClientHandler
    //Client disconnected -> báo event về RelayServer -> remove khỏi list
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
        public event Action<ClientHandler, Message> TcpMessageReceived;
        public event Action<ClientHandler, Message> UdpMessageReceived;
        public ClientHandler(TcpClient client, string serverIP, int udpPort)
        {
            _tcpClient = client;
            _udpClient = new UdpClient(0); //random port
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
            if (msg is ClientInfo clientInfoMsg)
            {
                Info = clientInfoMsg;
            }
            TcpMessageReceived?.Invoke(this, msg);
        }

        private void OnUdpMessageReceived(Message msg)
        {
            UdpMessageReceived?.Invoke(this, msg);
        }

        public void Start()
        {
            //start cả 2 listener
            _tcpListener.Start();
            _udpListener.Start();
            return;
        }

        public void Send(Message msg)
        {
            //Kiểm tra xem client còn connect ko
            if (_tcpClient?.Connected != true)
            {
                System.Diagnostics.Debug.WriteLine($"[ClientHandler] Cannot send - client {Info?.Id} is not connected");
                return; //ko còn thì ko gửi
            }

            try
            {
                if (msg is ChatMessage || msg is ScreenFrameMessage || msg is MouseEventMessage)
                {
                    System.Diagnostics.Debug.WriteLine($"[ClientHandler] Sending UDP: {msg.GetType().Name} from {msg.From} to {msg.To}");
                    MessageSender.Send(_udpClient, _serverUdpEndpoint, msg);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ClientHandler] Sending TCP: {msg.GetType().Name} from {msg.From} to {msg.To}");
                    MessageSender.Send(_tcpClient, msg);
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