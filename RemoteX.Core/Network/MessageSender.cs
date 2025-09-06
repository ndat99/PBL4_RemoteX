using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Shared.Models;
using System.Net.Sockets;
using System.Text.Json;

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

        public static void Send<T>(TcpClient tcpClient, T message)
        {
            if(tcpClient == null || !tcpClient.Connected) return;
            try
            {
                //Lay kenh truyen du lieu tu TcpLient
                var stream = tcpClient.GetStream();
                //Tạo streamWriter để ghi dữ liệu dạng text vào stream
                using var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };
                string json = JsonSerializer.Serialize(message);
                //Ghi chuỗi Json đến stream
                writer.WriteLine(json);
            }
            catch { }
        }
    }
}
