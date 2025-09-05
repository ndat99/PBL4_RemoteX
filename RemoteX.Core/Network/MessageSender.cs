using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Shared.Models;
using System.Net.Sockets;

namespace RemoteX.Core.Network
{
    public static class MessageSender
    {
        public static void SendClientInfo(TcpClient client, ClientInfo info)
        {
            //Gui thong tin Client cho Server
            var stream = client.GetStream();
            string json = System.Text.Json.JsonSerializer.Serialize(info); //Chuyen thanh chuoi JSON
            byte[] data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }
    }
}
