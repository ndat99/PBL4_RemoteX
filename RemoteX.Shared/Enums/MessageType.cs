using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Enums
{
    public enum MessageType
    {
        ConnectRequest = 1,
        Chat = 2,
        File,
        ScreenFrame,
        MouseEvent,
        KeyboardEvent
    }
}
