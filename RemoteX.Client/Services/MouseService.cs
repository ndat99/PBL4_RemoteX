using System.Runtime.InteropServices;
using RemoteX.Core.Models;

namespace RemoteX.Client.Services
{
    public class MouseService
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        //Thực hiện mouse event trên máy bị điều khiển
        public static void ExecuteMouseEvent(MouseEventMessage e)
        {
            switch (e.Action)
            {
                case MouseEventMessage.MouseAction.Move:
                    SetCursorPos(e.X, e.Y);
                    break;
                case MouseEventMessage.MouseAction.LeftDown:
                    SetCursorPos(e.X, e.Y);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;
                case MouseEventMessage.MouseAction.LeftUp:
                    SetCursorPos(e.X, e.Y);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case MouseEventMessage.MouseAction.RightDown:
                    SetCursorPos(e.X, e.Y);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    break;
                case MouseEventMessage.MouseAction.RightUp:
                    SetCursorPos(e.X, e.Y);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case MouseEventMessage.MouseAction.DoubleClick:
                    SetCursorPos(e.X, e.Y);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case MouseEventMessage.MouseAction.Scroll:
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)e.X, 0);
                    break;
            }
        }

        public static (int X, int Y) ConvertToRemoteCoordinates(
            System.Windows.Point localPoint,
            System.Windows.Size imageControlSize,
            int remoteScreenWidth,
            int remoteScreenHeight)
        {
            double remoteAspect = (double)remoteScreenWidth / remoteScreenHeight; //tỷ lệ màn hình remote
            double controlAspect = (double)imageControlSize.Width / imageControlSize.Height; //tỷ lệ của Image Control
            
            double actualImageWidth, actualImageHeight;
            double offsetX = 0, offsetY = 0;
        }
    }
}
