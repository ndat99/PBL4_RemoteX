using RemoteX.Core.Enums;
using RemoteX.Core.Models;
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
        public static async Task Send<T>(TcpClient client, T message) where T : Message
        {
            var stream = client.GetStream();

            string json = Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Raw line: {data}");


            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        public static string Serialize(Message msg)
        {
            return JsonSerializer.Serialize(msg, msg.GetType(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
