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
    public class MessageListener
    {
        private readonly TcpClient _client;
        public event Action<Message> MessageReceived;
        public event Action ClientDisconnected;

        public MessageListener(TcpClient client)
        {
            _client = client;
        }

        public async Task StartAsync()
        {
            try
            {
                var stream = _client.GetStream();
                using var reader = new StreamReader(_client.GetStream(), Encoding.UTF8);

                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var msg = JsonSerializer.Deserialize<Message>(line);
                    MessageReceived?.Invoke(msg);
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
                ClientDisconnected?.Invoke();
                _client.Close();
            }
        }
    }
}
