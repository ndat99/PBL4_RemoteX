using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    public class ScreenFrameMessage : BaseMessage
    {
        public override MessageType Type => MessageType.ScreenFrame;

        public byte[] ImageData { get; set; } // frame nén JPEG/PNG
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
