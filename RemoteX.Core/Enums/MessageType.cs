using System;
using System.Text;

namespace RemoteX.Core.Enums
{
    public enum MessageType
    {
        ConnectRequest,
        ClientInfo,
        Chat,
        File,
        Screen,
        MouseEvent,
        KeyboardEvent,
        Log,
    }
}
