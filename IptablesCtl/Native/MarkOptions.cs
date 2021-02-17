using System.Runtime.InteropServices;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*xt_mark_mtinfo1*/
    public struct MarkOptions
    {
        public const byte XT_MARK_INV = 0x01;
        public uint mark, mask;
        public byte invert;
    }
};