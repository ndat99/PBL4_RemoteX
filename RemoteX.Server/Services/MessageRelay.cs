using RemoteX.Shared.Models;
using RemoteX.Shared.Utils;
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
        private readonly List<ClientInfo> _clients;

        //Truyen vao danh sach client da ket noi
        public MessageRelay(List<ClientInfo> clients)
        {
            _clients = clients;
        }

        //Relay chatmessage tu sender -> receiver
        public void RelayMessage(ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.ReceiverID)) return;

            //Tim client trong danh sach
            var targetClient = _clients.FirstOrDefault(c => c.Id == message.ReceiverID);

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
            else
            {
                MessageBox.Show($"Không tìm thấy client đích: {message.ReceiverID}");
            }
        }
    }
}
