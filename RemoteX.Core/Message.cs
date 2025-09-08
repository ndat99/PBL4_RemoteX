using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core
{
    public class Message
    {
        public MessageType Type { get; set; }
        public string Payload { get; set; } //Nội dung tin nhắn, có thể là JSON hoặc dữ liệu khác tùy loại tin nhắn
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
