using RemoteX.Core.Enums;
using RemoteX.Core.Models;
using System.Net;
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

        public void Start()
        {
            _isRunning = true;
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    if (_useUdp)
                    {
                        while (_isRunning)
                        {
                            try
                            {
                                var remoteEP = new IPEndPoint(IPAddress.Any, 0);
                                byte[] buffer = _udpClient.Receive(ref remoteEP);
                                HandleMessage(buffer, remoteEP.ToString());
                            }
                            catch (SocketException)
                            {
                                //udp client bị đóng thì ném exception rồi thoát vòng lặp
                                if (!_isRunning) break;
                            }
                        }
                    }
                    else
                    {
                        using var reader = new StreamReader(_tcpClient.GetStream(), Encoding.UTF8);
                        var endpoint = _tcpClient.Client.RemoteEndPoint?.ToString();

                        while (true)
                        {
                            var line = reader.ReadLine(); //đọc từng dòng json từ tcp stream
                            if (line == null) break; //client dong stream
                            HandleMessage(Encoding.UTF8.GetBytes(line), endpoint);
                        }
                    }
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Listener IO Error] {ex.Message}");
                }
                catch (SocketException ex)
                {
                    //client tat app
                    System.Diagnostics.Debug.WriteLine($"[Listener Socket Error] {ex.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Listener General Error] {ex.ToString()}");
                }
                finally
                {
                    ClientDisconnected?.Invoke(); //báo cho client handler biết client đã disconnect

                    //cái nào có thì đóng cái đó
                    _tcpClient?.Close();
                    _udpClient?.Close();
                }
            });
            listenThread.IsBackground = true; //để luồng tự tắt khi thoát ứng dụng
            listenThread.Start();
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
                MessageType.MouseEvent => JsonSerializer.Deserialize<MouseEventMessage>(json),
                MessageType.KeyboardEvent => JsonSerializer.Deserialize<KeyboardEventMessage>(json),
                MessageType.File => DeserializeFileMessage(json, doc),
                _ => throw new NotSupportedException($"Unsupported message type {type}")
            };
        }

        private static Message DeserializeFileMessage(string json, JsonDocument doc)
        {
            if (doc.RootElement.TryGetProperty("FileName", out _))
            {
                return JsonSerializer.Deserialize<FileMessage>(json);
            }
            else
            {
                return JsonSerializer.Deserialize<FileChunk>(json);
            }
        }

        //Dừng listener (chủ động)
        public void Stop()
        {
            _isRunning = false;
        }
    }
}
