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
            System.Windows.Point localPoint, //tọa độ click trong Image Control (màn hình điều khiển, tính cả letterbox)
            System.Windows.Size imageControlSize, //kích thước Image Control (màn hình điều khiển, tính cả letterbox)
            int remoteScreenWidth,
            int remoteScreenHeight)
        {
            //tỷ lệ màn hình remote (màn hình thật)
            double remoteAspect = (double)remoteScreenWidth / remoteScreenHeight;
            //tỷ lệ của Image Control (màn hình điều khiển)
            double controlAspect = (double)imageControlSize.Width / imageControlSize.Height;
            
            double actualImageWidth, actualImageHeight;
            double offsetX = 0, offsetY = 0;

            //xử lý phần letterbox (viền đen thừa ra trên/dưới hoặc trái/phải)
            //tỷ lệ màn hình điều khiển > tỷ lệ màn hình thật => viền đen trái/phải
            if (controlAspect > remoteAspect)
            {
                //chiều cao thực = chiều cao ảnh hiển thị
                actualImageHeight = imageControlSize.Height;
                //chiều rộng thực = chiều cao thực * tỷ lệ màn hình thật
                actualImageWidth = actualImageHeight * remoteAspect;
                //viền đen thừa ra ở mỗi bên trái/phải = (chiều dài màn hình điều khiển - chiều dài thực) chia 2
                offsetX = (imageControlSize.Width - actualImageWidth) / 2;
            }
            else //viền đen trên/dưới
            {
                //chiều rộng thực = chiều rộng ảnh hiển thị
                actualImageWidth = imageControlSize.Width;
                //chiều cao thực = chiều rộng thực / tỷ lệ màn hình thật
                actualImageHeight = actualImageWidth / remoteAspect;
                //viền đen thừa ra ở mỗi phía trên/dưới = (chiều cao màn hình điều khiển - chiều cao thực) chia 2
                offsetY = (imageControlSize.Height - actualImageHeight) / 2;
            }
            //tọa độ tương đối trong vùng ảnh thực tế (trừ đi tọa độ biên phần viền đen)
            double relativeX = localPoint.X - offsetX;
            double relativeY = localPoint.Y - offsetY;

            //kiểm tra xem có click vào phần viền đen hay không
            if (relativeX < 0 || relativeX > actualImageWidth ||
                relativeY < 0 || relativeY > actualImageHeight)
            {
                return (-1, -1);
            }

            //scale lên tọa độ thực
            int remoteX = (int)(relativeX / actualImageWidth * remoteScreenWidth);
            int remoteY = (int)(relativeY / actualImageHeight * remoteScreenHeight);

            remoteX = Math.Max(0, Math.Min(remoteScreenWidth - 1, remoteX));
            remoteY = Math.Max(0, Math.Min(remoteScreenHeight - 1, remoteY));

            return (remoteX, remoteY);
        }
    }
}
