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
        //public override MessageType Type => MessageType.KeyboardEvent;

        public int KeyCode { get; set; }   // virtual key code
        public KeyAction Action { get; set; }
    }

    public enum KeyAction
    {
        Down,
        Up,
        Press
    }
}
