using System.Runtime.InteropServices;
using System;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*ipt_entry*/
    public struct IptEntry
    {
        public IptIp ip;

        /* Mark with fields that we care about. */
        public uint nfcache;

        /* Size of ipt_entry + matches */
        public ushort target_offset;
        /* Size of ipt_entry + matches + target */
        public ushort next_offset;

        /* Back pointer */
        public uint comefrom;

        /* Packet and byte counters. */
        public IptCounters counters;
    };

};