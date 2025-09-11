using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    public class ConnectRequest : Message
    {
        public string Password { get; set; }
        public string Status { get; set; }
        public ConnectRequest()
        {
            Type = MessageType.ConnectRequest;
        }
    }
}
