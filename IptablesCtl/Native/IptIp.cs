using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IptIp
    {        
        public const int IFNAMSIZ = 16;
        public const byte IPT_INV_SRCIP = 0x08;
        public const byte IPT_INV_DSTIP = 0x10;
        public const byte IPT_INV_VIA_IN = 0x01;
        public const byte IPT_INV_VIA_OUT = 0x02;
        public const byte IPT_F_FRAG = 0x01;
        public const byte IPT_INV_FRAG = 0x20;
        public const byte IPT_INV_PROTO = 0x40;
        /* Source and destination IP addr */
        public InetAddr src, dst;
        /* Mask for src and dest IP addr */
        public InetAddr src_mask, dst_mask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IFNAMSIZ)]
        public string in_iface, out_iface;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IFNAMSIZ)]
        public byte[] in_iface_mask, out_iface_mask;

        /* Protocol, 0 = ANY */
        public ushort proto;
        /* Flags word */
        public byte flags;
        /* Inverse flags */
        public byte invflags;
    }
};   