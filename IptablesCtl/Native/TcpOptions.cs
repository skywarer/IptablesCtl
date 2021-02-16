using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    /*xt_tcp*/
    [StructLayout(LayoutKind.Sequential)]
    public struct TcpOptions
    {
        public const byte XT_TCP_INV_SRCPT = 0x01;
        public const byte XT_TCP_INV_DSTPT = 0x02;
        public const byte XT_TCP_INV_FLAGS = 0x04;
        public const byte XT_TCP_INV_OPTION = 0x08;
        public const byte XT_TCP_INV_MASK = 0x0F;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ushort[] spts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ushort[] dpts;
        public byte options,flg_mask,flg_cmp,invflags;
    }
};