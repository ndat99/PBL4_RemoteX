using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace RemoteX.Server.Services
{
    public class MessageRelay
    {
        private ClientManager _clientManager;

        //Truyen vao danh sach client da ket noi
        public MessageRelay(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        //Relay chatmessage tu sender -> receiver
        public void RelayMessage(ChatMessage message)
        {
            var partnerId = _clientManager.GetPartnerId(message.ReceiverID);
            //if (partnerId != message.ReceiverID) return;
            
            if (partnerId != message.ReceiverID)
            {
                // Gửi thông báo lỗi về cho sender
                var errorMsg = new ChatMessage
                {
                    SenderID = "Server",
                    ReceiverID = message.SenderID,
                    Message = "❌ Chưa kết nối với đối tác này!"
                };
                var senderClient = _clientManager.FindById(message.SenderID);
                if (senderClient != null)
                    NetworkHelper.SendMessage(senderClient.TcpClient, errorMsg);
                return;
            }

            //Tim client trong danh sach
            var targetClient = _clientManager.FindById(message.ReceiverID);
            if (targetClient != null)
            {
                try
                {
                    NetworkHelper.SendMessage(targetClient.TcpClient, message);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}