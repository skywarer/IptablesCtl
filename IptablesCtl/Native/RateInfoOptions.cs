using System.Runtime.InteropServices;
using System;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*xt_rateinfo*/
    public struct RateInfoOptions
    {
        public const int XT_LIMIT_SCALE = 10000;
        public const uint LIMIT_BURST_DEF = 5;
        public const uint LIMIT_DEF = 12000000;
        public uint avg,burst;        
        public ulong prev;
        public uint credit,credit_cap,cost;
        public IntPtr master;
    }
};