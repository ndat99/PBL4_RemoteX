using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    public class QualityChangeMessage : Message
    {
        public QualityLevel Quality { get; set; }
        public QualityChangeMessage()
        {
            Type = MessageType.QualityChange;
        }
    }
}
