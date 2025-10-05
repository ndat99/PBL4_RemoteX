using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;

namespace RemoteX.Core.Networking
{
    public static class MessageSender
    {
        //tcp
        public static async Task Send<T>(TcpClient client, T message) where T : Message
        {
            var stream = client.GetStream(); //lấy stream từ tcpClient

            string json = Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n"); //thêm dấu xuống dòng để tách các message

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Raw line: {data}");
            System.Diagnostics.Debug.WriteLine($"[TX] {client.Client.RemoteEndPoint} | {json}");

            await stream.WriteAsync(data, 0, data.Length); //gửi dữ liệu qua tcp stream
            await stream.FlushAsync(); //gửi dữ liệu ngay lập tức
        }

        //udp
        public static async Task Send<T>(UdpClient client, IPEndPoint endPoint, T message) where T : Message
        {
            //endpoint là ip + port (udp phải có vì nó ko giữ kết nối như tcp nên phải luôn biết gửi đến đâu)
            string json = Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json); //Encode sang json

            System.Diagnostics.Debug.WriteLine($"[UDP TX] {endPoint}: {json}"); //debug
            //  socket UDP       dữ liệu              nơi nhận
            await client.SendAsync(data, data.Length, endPoint); //gửi dữ liệu đến endpoint (ko cần stream như tcp)
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
