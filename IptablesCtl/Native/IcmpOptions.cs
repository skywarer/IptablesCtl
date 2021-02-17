using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*ipt_icmp*/
    public struct IcmpOptions
    {        
        public const byte IPT_ICMP_INV = 0x01;
        public byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] code;
        public byte invflags;
    }
};