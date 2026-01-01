using System;
using System.Runtime.InteropServices;

namespace XboxGamingBarHelper.Input
{
    internal static class XInput
    {
        // P/Invoke XInput
        // Using xinput1_4.dll which is standard on Windows 8/10/11.
        [DllImport("xinput1_4.dll")]
        public static extern int XInputGetState(int dwUserIndex, ref XInputState pState);

        [StructLayout(LayoutKind.Sequential)]
        public struct XInputState
        {
            public uint PacketNumber;
            public XInputGamepad Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XInputGamepad
        {
            public ushort Buttons;
            public byte LeftTrigger;
            public byte RightTrigger;
            public short ThumbLX;
            public short ThumbLY;
            public short ThumbRX;
            public short ThumbRY;
        }

        [Flags]
        public enum GamepadButtonFlags : ushort
        {
            None = 0,
            DPadUp = 0x0001,
            DPadDown = 0x0002,
            DPadLeft = 0x0004,
            DPadRight = 0x0008,
            Start = 0x0010,
            Back = 0x0020,
            LeftThumb = 0x0040,
            RightThumb = 0x0080,
            LeftShoulder = 0x0100,
            RightShoulder = 0x0200,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000
        }
    }
}
