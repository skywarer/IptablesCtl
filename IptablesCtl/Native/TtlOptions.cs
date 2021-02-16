using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TtlOptions
    {
        public const byte IPT_TTL_EQ = 0;
        public const byte IPT_TTL_NE = 1;
        public const byte IPT_TTL_LT = 2;
        public const byte IPT_TTL_GT = 3;
        public byte mode, ttl;
    }
};