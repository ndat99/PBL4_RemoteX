using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    public class FileAcceptMessage : Message
    {
        public Guid FileID { get; set; }
        public FileAcceptMessage()
        {
            Type = MessageType.FileAccept;
        }
    }
}
