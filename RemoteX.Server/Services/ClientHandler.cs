using RemoteX.Core;
using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RemoteX.Server.Services
{
    //Giữ TcpClient, MessageListener và ClientInfo của 1 client
    //ServerController quản lý list ClientHandler
    //Client disconnected -> báo event về ServerController -> remove khỏi list
    public class ClientHandler
    {
        public TcpClient Client { get; }
        public ClientInfo Info { get; private set; }
        private readonly MessageListener _listener;

        public event Action<ClientHandler> Disconnected;
        public event Action<ClientInfo> ClientInfoReceived;
        public event Action<ChatMessage> ChatMessageReceived;
        public event Action<ScreenFrameMessage> ScreenFrameReceived;

        public ClientHandler(TcpClient client)
        {
            Client = client;
            _listener = new MessageListener(client);
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

                default:
                    break;
            }
        }

        public Task StartAsync()
        {
            return _listener.StartAsync();
        }
    }
}