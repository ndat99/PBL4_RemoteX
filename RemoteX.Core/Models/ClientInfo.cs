using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RemoteX.Core.Utils;

namespace RemoteX.Core.Models
{
    //Định nghĩa client model chung cho server và client
    public class ClientInfo
    {
        public string Id { get; set; } //ID client
        public string Password { get; set; }
        public string MachineName { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public TcpClient TcpClient { get; set; } //Đối tượng TcpClient để giao tiếp

        public ClientInfo(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }
    }
}
