using RemoteX.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteX.Shared.Models
{
    public class ChatMessage : BaseMessage
    {
        public override MessageType Type => MessageType.Chat;
        public string Message { get; set; }
        [JsonIgnore]
        public bool IsMine { get; set; }
    }
}

