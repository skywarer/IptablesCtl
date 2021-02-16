using System.Runtime.InteropServices;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MultiportOptions
    {
        public const byte XT_MULTIPORT_SOURCE = 0;
        public const byte XT_MULTIPORT_DESTINATION = 1;
        public const byte XT_MULTIPORT_EITHER = 2;
        public const int XT_MULTI_PORTS = 15;
        public const byte XT_MULTIPORTS_INV = 0x01;
        public byte flags, count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = XT_MULTI_PORTS)]
        public ushort[] ports;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = XT_MULTI_PORTS)]
        public byte[] pflags;
        public byte invert;
    }
};