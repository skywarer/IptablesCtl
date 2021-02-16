using System.Runtime.InteropServices;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LogOptions
    {        
        public const byte IPT_LOG_TCPSEQ = 0x01;
        public const byte IPT_LOG_TCPOPT = 0x02;
        public const byte IPT_LOG_IPOPT = 0x04;
        public const byte IPT_LOG_UID = 0x08;
        public const byte IPT_LIPT_LOG_NFLOGOG_TCPSEQ = 0x10;
        public const byte IPT_LOG_MACDECODE = 0x20;
        public const byte IPT_LOG_MASK = 0x2F;
        public byte level, logflags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string prefix;
    }
};