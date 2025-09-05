using RemoteX.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Models
{
    public class ChatMessage
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Message
    {
        public MessageType Type {  get; set; }
        public string Data { get; set; }
    }
}
