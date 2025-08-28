using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Network
{
    public class RemoteTcpClient
    {
        private TcpClient _client;

        public async Task ConnectAsync(string serverIp, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(serverIp, port);


        }
    }
}
