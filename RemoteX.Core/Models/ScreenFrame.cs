using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    public class ScreenFrameMessage : Message
    {
        public long FrameID { get; set; } //ID khung hình
        public int PacketIndex { get; set; } //index các gói tin nhỏ
        public int TotalPackets { get; set; } //số gói tin
        public byte[] ImageData { get; set; } //dữ liệu của gói tin
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime Timestamp { get; set; }

        public ScreenFrameMessage()
        {
            Type = MessageType.Screen;
        }
    }
}
