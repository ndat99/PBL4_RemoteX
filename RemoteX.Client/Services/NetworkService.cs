using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RemoteX.Shared.Enums;
using RemoteX.Shared.Models;

namespace RemoteX.Client.Services
{
    public class NetworkService
    {
        public void SendMessage(string ReceiverID, string Message, ClientInfo clientInfo)
        {
            var chatMsg = new ChatMessage
            {
                SenderID = clientInfo.Id,
                Message = Message,
                ReceiverID = ReceiverID,
                Timestamp = DateTime.Now
            };
            var msg = new Message
            {
                Type = MessageType.Chat,
                Data = JsonSerializer.Serialize(chatMsg)
            };

            clientInfo.Send(msg);
        }

        public void OnMessageReceived(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.Chat:
                    var chatMsg = JsonSerializer.Deserialize<ChatMessage>(msg.Data);
                    // Gọi UI update
                    //DisplayChatMessage(chatMsg.SenderId, chatMsg.Content);
                    break;

                    // có thể thêm các loại khác: FileTransfer, RemoteControl
            }
        }

        private async Task ReceiveLoop()
        {
            
        }

    }
}
