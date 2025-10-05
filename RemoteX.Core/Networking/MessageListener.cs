using RemoteX.Core.Enums;
using RemoteX.Core.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RemoteX.Core.Networking
{
    public class MessageListener
    {
        private readonly TcpClient? _tcpClient;
        private readonly UdpClient? _udpClient;
        private readonly bool _useUdp;
        private bool _isRunning; //để chủ động stop listener cho udp

        public event Action<Message> MessageReceived;
        public event Action ClientDisconnected;

        public MessageListener(TcpClient client)
        {
            _tcpClient = client;
            _useUdp = false;
        }

        public MessageListener(UdpClient client)
        {
            _udpClient = client;
            _useUdp = true;
        }

        public async Task StartAsync()
        {
            _isRunning = true;
            try
            {
                if (_useUdp)
                {
                    while (_isRunning)
                    {
                        var result = await _udpClient.ReceiveAsync();
                        //ReceiveAsync là hàm chặn, đợi có gói tin đến, nó chạy mãi nên phải có _isRunning để chủ động dừng
                        HandleMessage(result.Buffer, result.RemoteEndPoint.ToString());
                    }
                }
                else
                {
                    using var reader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8);
                    var endpoint = _tcpClient.Client.RemoteEndPoint?.ToString();

                    while (true)
                    {
                        var line = await reader.ReadLineAsync(); //đọc từng dòng json từ tcp stream
                        if (line == null) break; //client dong stream
                        HandleMessage(Encoding.UTF8.GetBytes(line), endpoint);
                    }
                }
            }
            catch (IOException)
            {

            }
            catch (SocketException)
            {
                //client tat app
            }
            finally
            {
                ClientDisconnected?.Invoke(); //báo cho client handler biết client đã disconnect

                //cái nào có thì đóng cái đó
                _tcpClient?.Close();
                _udpClient?.Close();
            }
        }

        private void HandleMessage(byte[] buffer, string endpoint)
        {
            var json = Encoding.UTF8.GetString(buffer);
            var prefix = _useUdp ? "[UDP RX]" : "[TCP RX]";
            System.Diagnostics.Debug.WriteLine($"{prefix} {endpoint} | {json}");

            var message = Deserialize(json);

            System.Diagnostics.Debug.WriteLine($"[PARSE] {endpoint} | Type={message.Type} From={message.From} To={message.To}");
            MessageReceived?.Invoke(message);
        }

        public static Message Deserialize(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var type = (MessageType)doc.RootElement.GetProperty("Type").GetInt32();
            return type switch
            {
                MessageType.ConnectRequest => JsonSerializer.Deserialize<ConnectRequest>(json),
                MessageType.ClientInfo => JsonSerializer.Deserialize<ClientInfo>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                MessageType.Chat => JsonSerializer.Deserialize<ChatMessage>(json),
                MessageType.Screen => JsonSerializer.Deserialize<ScreenFrameMessage>(json),
                MessageType.Log => JsonSerializer.Deserialize<Log>(json),
                _ => throw new NotSupportedException($"Unsupported message type {type}")
            };
        }

        //Dừng listener (chủ động)
        public void Stop()
        {
            _isRunning = false;
        }
    }
}
