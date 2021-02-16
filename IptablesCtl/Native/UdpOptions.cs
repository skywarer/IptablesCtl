using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    /*xt_udp*/
    [StructLayout(LayoutKind.Sequential)]
    public struct UdpOptions
    {
        public const byte XT_UDP_INV_SRCPT = 0x01;
        public const byte XT_UDP_INV_DSTPT = 0x02;
        public const byte XT_UDP_INV_MASK = 0x03;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ushort[] spts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ushort[] dpts;
        public byte invflags;
    }
};