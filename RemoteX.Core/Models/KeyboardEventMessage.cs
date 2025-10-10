using RemoteX.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    public class KeyboardEventMessage : Message
    {
        public int KeyCode { get; set; }   //virtual key code (mã của phím được nhấn)
        public bool IsKeyUp { get; set; } //true nếu phím được thả ra, false nếu phím được nhấn xuống
        public KeyboardEventMessage()
        {
            Type = MessageType.KeyboardEvent;
        }
    }
}
