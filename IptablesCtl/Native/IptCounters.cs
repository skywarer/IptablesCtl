using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IptCounters
    {
        /* Packet and byte counters */
        public ulong pkt_cnt, byte_cnt;      
    };
};   