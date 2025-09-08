//using RemoteX.Core.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace RemoteX.Core.Utils
//{
//    public static class NetworkHelper
//    {
//        //Listen
//        public static ClientInfo ReceiveClientInfo(TcpClient tcpClient)
//        {
//            var stream = tcpClient.GetStream();

//            // Đọc 4 byte độ dài
//            byte[] lengthBuffer = new byte[4];
//            stream.Read(lengthBuffer, 0, 4);
//            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

//            // Đọc dữ liệu JSON
//            byte[] buffer = new byte[messageLength];
//            int read = 0;
//            while (read < messageLength)
//            {
//                int chunk = stream.Read(buffer, read, messageLength - read);
//                if (chunk == 0) break;
//                read += chunk;
//            }

//            string json = Encoding.UTF8.GetString(buffer, 0, read);
//            ClientInfo client = JsonSerializer.Deserialize<ClientInfo>(json);
//            client.TcpClient = tcpClient;

//            return client;
//        }

//        public static void ListenForDisconnected(ClientInfo client, Action<ClientInfo> onDisconnect, Action<string> onMessage = null)
//        {
//            try
//            {
//                var stream = client.TcpClient.GetStream();
//                byte[] buffer = new byte[1024];

//                while (client.TcpClient.Connected)
//                {
//                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
//                    if (bytesRead == 0) //Khi client dong ket noi
//                    {
//                        break;
//                    }
//                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                    onMessage?.Invoke(message);
//                }

//            }
//            catch (IOException)
//            {
//            }
//            finally
//            {
//                onDisconnect?.Invoke(client);
//                client.TcpClient.Close();
//            }
//        }

//        public static void ListenForMessages<T>(ClientInfo client, Action<T> onMessageReceived) where T : BaseMessage
//        {
//            if (client == null || !client.TcpClient.Connected) return;

//            var stream = client.TcpClient.GetStream();
//            try
//            {
//                while (client.TcpClient.Connected)
//                {
//                    //Doc 4 byte do dai
//                    byte[] lengthBuffer = new byte[4];
//                    int bytesRead = stream.Read(lengthBuffer, 0, 4);
//                    if (bytesRead == 0) break; //client ngat ket noi

//                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

//                    //Doc noi dung message
//                    byte[] buffer = new byte[messageLength];
//                    int read = 0;
//                    while (read < messageLength)
//                    {
//                        int chunk = stream.Read(buffer, read, messageLength - read);
//                        if (chunk == 0) break;
//                        read += chunk;
//                    }

//                    string json = Encoding.UTF8.GetString(buffer);

//                    //Deserialize lai thanh object
//                    var message = JsonSerializer.Deserialize<T>(json);
//                    if (message != null)
//                        onMessageReceived?.Invoke(message);
//                }
//            }
//            catch (IOException)
//            {

//            }
//        }



//        //Send
//        public static void SendClientInfo(TcpClient client, ClientInfo info)
//        {
//            var stream = client.GetStream();
//            string json = JsonSerializer.Serialize(info);
//            byte[] data = Encoding.UTF8.GetBytes(json);
//            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

//            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
//            stream.Write(data, 0, data.Length);
//        }

//        public static void SendMessage<T>(TcpClient tcpClient, T message) where T : BaseMessage
//        {
//            if (tcpClient == null || !tcpClient.Connected) return;
//            try
//            {
//                string json = JsonSerializer.Serialize(message);    //Serialize object thanh JSON string
//                byte[] data = Encoding.UTF8.GetBytes(json);     //Chuyen sang byte array
//                byte[] lengthPrefix = BitConverter.GetBytes(data.Length); //Lay do dai du lieu

//                //Lay kenh truyen du lieu tu TcpLient
//                var stream = tcpClient.GetStream();

//                stream.Write(lengthPrefix, 0, lengthPrefix.Length); //Gui do dai du lieu truoc
//                stream.Write(data, 0, data.Length); //Gui du lieu
//                stream.Flush();
//            }
//            catch (Exception ex)
//            {

//            }
//        }
//    }
//}