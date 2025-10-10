using RemoteX.Core.Enums;

namespace RemoteX.Core.Models
{
    public class MouseEventMessage : Message
    {
        public MouseAction Action { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public MouseEventMessage()
        {
            Type = MessageType.MouseEvent;
        }

        public enum MouseAction
        {
            Move,
            LeftDown,
            LeftUp,
            RightDown,
            RightUp,
            DoubleClick,
            Scroll
        }
    }
}
