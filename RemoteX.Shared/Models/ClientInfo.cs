using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteX.Shared.Models
{
    public class ClientInfo
    {
        public string Id { get; set; }
        public string Password { get; set; }

        public string IpAddress { get; set; }
        
        public bool IsConnected { get; set; }

        public string Display => $"ID={Id}, Pass={Password}";

        public TcpClient TcpClient { get; set; }
    }
}
