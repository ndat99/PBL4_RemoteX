using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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
            System.Diagnostics.Debug.WriteLine($"[TX] {client.Client.RemoteEndPoint} | {json}");

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
