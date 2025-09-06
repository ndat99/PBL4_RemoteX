using RemoteX.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public static class NetworkHelper
    {
        //Listen
        public static ClientInfo ReceiveClientInfo(TcpClient tcpClient)
        {
            //Doc du lieu tu Client gui sang
            var stream = tcpClient.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            ClientInfo client = System.Text.Json.JsonSerializer.Deserialize<ClientInfo>(json);
            client.TcpClient = tcpClient; //Gan Socket thuc te vao ClientInfo

            return client;
        }

        public static void ListenForDisconnected(ClientInfo client, Action<ClientInfo> onDisconnect, Action<string> onMessage = null)
        {
            try
            {
                var stream = client.TcpClient.GetStream();
                byte[] buffer = new byte[1024];

                while (client.TcpClient.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) //Khi client dong ket noi
                    {
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    onMessage?.Invoke(message); //callback xu ly message neu can
                }

            }
            catch (IOException)
            {
            }
            finally
            {
                onDisconnect?.Invoke(client); //Goi callback remove client
                client.TcpClient.Close();
            }
        }

        public static void ListenForMessages<T>(ClientInfo client, Action<T> onMessage, Action<ClientInfo> onDisconnected = null)
        {
            try
            {
                var reader = new StreamReader(client.TcpClient.GetStream());

                while (client.TcpClient.Connected)
                {
                    string line = reader.ReadLine(); //doc stream theo dong
                    if (line == null) break; //client ngat ket noi

                    var message = JsonSerializer.Deserialize<T>(line); //chuyen json thanh object T
                    if (message != null)
                        onMessage?.Invoke(message); //callback xu ly message
                }
            }
            catch (IOException)
            {
                //Client disconnected
            }
            finally
            {
                onDisconnected?.Invoke(client);
                client.TcpClient.Close();
            }
        }



        //Send
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
            if (tcpClient == null || !tcpClient.Connected) return;
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
