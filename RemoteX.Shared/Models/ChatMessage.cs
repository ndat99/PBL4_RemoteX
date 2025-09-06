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

        public ChatMessageType Type { get; set; } = ChatMessageType.Text; //Loai tin nhan

        public byte[] Data { get; set; } //Dung cho file/screen
    }
}

