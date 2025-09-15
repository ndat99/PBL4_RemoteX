using RemoteX.Core.Enums;
using System.Text.Json.Serialization;

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

        public ChatMessage()
        {
            Type = MessageType.Chat;
        }
    }
}

