using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IcmpOptions
    {
        /*
        public IcmpOptions(byte icmpType, bool inv)
        {
            type = icmpType;

        }
        */
        public const byte IPT_ICMP_INV = 0x01;
        public byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] code;
        public byte invflags;
    }
};