using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    public class FileMessage : BaseMessage
    {
        public override MessageType Type => MessageType.File;

        public string FileName { get; set; }
        public long FileSize { get; set; }
        public byte[] Data { get; set; } // có thể chunk sau
    }
}
