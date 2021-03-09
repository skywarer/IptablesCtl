using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*nf_nat_ipv4_range*/
    public struct NatRange
    {
        public const int NF_NAT_RANGE_MAP_IPS = 1;
        public const int NF_NAT_RANGE_PROTO_SPECIFIED = 2;
        public const int NF_NAT_RANGE_PROTO_RANDOM = 4;
        public const int NF_NAT_RANGE_PERSISTENT = 8;
        public const int NF_NAT_RANGE_PROTO_RANDOM_FULLY = 16;
        public const int NF_NAT_RANGE_NETMAP = 1 << 6;
        public uint flags;
        /*litle-endian*/
        public uint min_ip, max_ip;
        /*litle-endian*/
        public ushort min_proto, max_proto;   

        public static NatRange Default()
        {
            NatRange range = new NatRange();
            range.max_proto = ushort.MaxValue;
            range.max_ip = uint.MaxValue;
            return range;
        } 
    }
};