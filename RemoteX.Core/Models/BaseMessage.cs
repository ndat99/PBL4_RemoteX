using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    [Serializable]
    public abstract class BaseMessage
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public abstract MessageType Type { get; }
    }
}
