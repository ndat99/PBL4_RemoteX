using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    public class Log : Message
    {
        public string Content { get; set; }

        public Log()
        {
            Type = MessageType.Log;
        }

        public Log(string content)
        {
            Type = MessageType.Log;
            Content = content;
        }
    }
}
