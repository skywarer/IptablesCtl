using System.Runtime.InteropServices;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*xt_mac_info*/
    public struct MacOptions
    {
        public const int ETH_ALEN = 6;
        public const byte XT_MAC_INV = 0x01;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ETH_ALEN)]
        public byte[] srcaddr;
        public int invert;
    }
};