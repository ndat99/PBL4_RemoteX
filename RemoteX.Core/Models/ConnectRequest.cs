using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    [Serializable]
    public class ConnectRequest : BaseMessage
    {
        public override MessageType Type => MessageType.ConnectRequest;
        public string TargetID { get; set; }
        public string Password { get; set; }
    }
}
