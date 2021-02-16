using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StandardTarget
    {
        public const int NF_DROP = 0;
        public const int NF_ACCEPT = 1;
        public const int NF_QUEUE = 3;
        public const int NF_REPEAT = 4;
        public const int NF_STOP = 5;
        public Header target;
        public int verdict;
    }
};