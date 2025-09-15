using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    //Định nghĩa client model chung cho server và client
    public class ClientInfo : Message
    {
        public string Id { get; set; } //ID client
        public string Password { get; set; }
        public string MachineName { get; set; }

        public ClientInfo()
        {
            Type = MessageType.ClientInfo;
        }

        public ClientInfo(DeviceConfig config)
        {
            Id = config.DeviceID;
            Password = config.Password;
            MachineName = config.MachineName;
            Type = MessageType.ClientInfo;
        }
    }
}
