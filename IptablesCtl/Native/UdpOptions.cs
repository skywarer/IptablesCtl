using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*xt_udp*/
    public struct UdpOptions
    {
        public const byte XT_UDP_INV_SRCPT = 0x01;
        public const byte XT_UDP_INV_DSTPT = 0x02;
        public const byte XT_UDP_INV_MASK = 0x03; // All possible flags
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ushort[] spts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ushort[] dpts;
        public byte invflags;

        public static UdpOptions Default()
        {
            UdpOptions options = new UdpOptions();
            options.spts = new ushort[] {ushort.MinValue, ushort.MaxValue};
            options.dpts = new ushort[] {ushort.MinValue, ushort.MaxValue};
            return options;
        }
    }
};