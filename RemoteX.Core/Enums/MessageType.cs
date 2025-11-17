using System;
using System.Text;

namespace RemoteX.Core.Enums
{
    public enum MessageType
    {
        ConnectRequest,
        ClientInfo,
        Chat,
        Screen,
        MouseEvent,
        KeyboardEvent,
        Log,
        File,
        FileAccept,
        QualityChange
    }
}
