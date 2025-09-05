using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RemoteX.Shared.Enums;
using RemoteX.Shared.Models;

namespace RemoteX.Server.Services
{
    public class MessageRouter_cs
    {
        public readonly ClientManager _clientManager; 
        public MessageRouter_cs(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public void RouteMessage(ClientInfo sender, Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.Chat:
                    HandleChatMessage(sender, msg);
                    break;
                default:
                    Console.WriteLine("[Server] khong nhan dang duoc loai message!");
                    break;
            }
        }

        public void HandleChatMessage(ClientInfo sender, Message msg)
        {
            try
            {
                var chatMsg = JsonSerializer.Deserialize<ChatMessage>(msg.Data);

                Console.WriteLine($"[Server] {chatMsg.SenderID}: {chatMsg.Message}");

                //Tim client nhan
                var receiver = _clientManager.GetClientByID(chatMsg.ReceiverID);
                if(receiver != null)
                {
                    var forwardMsg = new Message
                    {
                        Type = MessageType.Chat,
                        Data = JsonSerializer.Serialize(chatMsg)
                    };
                    receiver.Send(forwardMsg);

                }
                else
                {
                    Console.WriteLine($"[Server] Client {chatMsg.ReceiverID} không online.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Lỗi xử lý chat: {ex.Message}");
            }
        }
    }
}
