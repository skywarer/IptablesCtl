#define DEBUG
using System.Runtime.InteropServices;
using IptablesCtl.Native;
namespace IptablesCtl.IO
{
    public static class Sizes
    {
        /* sizeof(c_long)*/
        public static readonly int _WORDLEN = Marshal.SizeOf<long>();
        public static readonly int IptEntryLen = Marshal.SizeOf<IptEntry>();
        public static readonly int HeaderLen = Marshal.SizeOf<Header>();
        public static readonly int TcpMatchOptLen = Marshal.SizeOf<TcpOptions>();
        public static readonly int NatOptLen = Marshal.SizeOf<NatOptions>();

        /* copy as is from https://github.com/ldx/python-iptables/blob/master/iptc/xtables.py */
        public static int Align(int size)
        {
            return ((size + (_WORDLEN - 1)) & ~(_WORDLEN - 1));
        }
    }
}