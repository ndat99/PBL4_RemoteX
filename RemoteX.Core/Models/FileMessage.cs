using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    [Serializable]
    public class FileMessage : Message 
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public bool IsMine { get; set; }

        public FileMessage()
        {
            Type = MessageType.File;
        }
    }
}
