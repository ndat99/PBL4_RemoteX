using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using System.Net.Sockets;

namespace RemoteX.Core.Services
{
    //Giữ TcpClient, MessageListener và ClientInfo của 1 client
    //ServerController quản lý list ClientHandler
    //Client disconnected -> báo event về ServerController -> remove khỏi list
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly MessageListener _listener;
        public TcpClient Client => _client;
        public ClientInfo Info { get; private set; }

        //Events
        public event Action<ClientHandler> Disconnected;
        public event Action<ClientInfo> ClientInfoReceived;
        public event Action<ChatMessage> ChatMessageReceived;
        public event Action<ScreenFrameMessage> ScreenFrameReceived;
        public event Action<ConnectRequest> ConnectRequestReceived;
        public event Action<Log> LogReceived;
        public ClientHandler(TcpClient client)
        {
            _client = client;
            _listener = new MessageListener(_client);
            _listener.MessageReceived += OnMessageReceived;
            _listener.ClientDisconnected += () => Disconnected?.Invoke(this);
        }

        private void OnMessageReceived(Message msg)
        {
            switch (msg)
            {
                case ClientInfo clientInfoMsg:
                    Info = clientInfoMsg;
                    ClientInfoReceived?.Invoke(Info);
                    break;

                case ChatMessage chatMsg:
                    ChatMessageReceived?.Invoke(chatMsg);
                    break;

                case ScreenFrameMessage frameMsg:
                    ScreenFrameReceived?.Invoke(frameMsg);
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

        public Task StartAsync()
        {
            return _listener.StartAsync();
        }

        public Task SendAsync(Message msg) => MessageSender.Send(_client, msg);
        public void Close() => _client.Close();
    }
}