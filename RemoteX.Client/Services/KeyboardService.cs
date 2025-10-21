using System.Runtime.InteropServices;
using RemoteX.Core.Models;

namespace RemoteX.Client.Services
{
    public static class KeyboardService
    {
        //cờ cho sk nhấn phím
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        //cờ cho sk nhả phím
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
        public static bool IsSimulating { get; set; }
        //thực thi sự kiện bàn phím
        public static void ExecuteKeyboardEvent(KeyboardEventMessage e)
        {
            System.Diagnostics.Debug.WriteLine($"[KEYBOARD EXEC] Executing key: {e.KeyCode}, IsUp: {e.IsKeyUp}");
            //1. Tạo cấu trúc INPUT
            IsSimulating = true;
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = 1, //1 = INPUT_KEYBOARD
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        //2. Điền mã phím ảo (virtual key code)
                        wVk = (ushort)e.KeyCode,
                        wScan = 0,
                        //3. Xác định sự kiện nhấn hay thả phím
                        dwFlags = e.IsKeyUp ? KEYEVENTF_KEYUP : KEYEVENTF_KEYDOWN,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
