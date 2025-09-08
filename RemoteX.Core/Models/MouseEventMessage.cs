using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteX.Core.Models
{
    public class MouseEventMessage : BaseMessage
    {
        public override MessageType Type => MessageType.MouseEvent;

        public int X { get; set; }
        public int Y { get; set; }
        public MouseAction Action { get; set; }
        public MouseButton Button { get; set; }

        public enum MouseAction
        {
            Move,
            Down,
            Up,
            Scroll
        }

        public enum MouseButton
        {
            None,
            Left,
            Right,
            Middle
        }
    }
}
