using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RemoteX.Core.Networking
{
    public static class MessageSender
    {
        public static void Send<T>(TcpClient client, T obj, Enums.MessageType type)
        {
            if (client == null || !client.Connected) return;
            var stream = client.GetStream();

            var msg = new Message
            {
                Type = type,
                Payload = JsonSerializer.Serialize(obj) //Chuyen doi tuong thanh JSON
            };

            string json = JsonSerializer.Serialize(msg);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");
            stream.Write(data, 0, data.Length);
        }
    }
}
