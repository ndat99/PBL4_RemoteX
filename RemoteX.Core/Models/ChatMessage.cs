using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    [Serializable]
    public class ChatMessage : Message
    {
        //public override MessageType Type => MessageType.Chat;
        public string Message { get; set; }

        //Chi dung o client UI de phan biet message do minh gui hay nhan
        [JsonIgnore]
        public bool IsMine { get; set; }
    }
}

