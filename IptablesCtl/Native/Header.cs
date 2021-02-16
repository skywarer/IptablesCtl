using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Header
    {
        public const int XT_EXTENSION_MAXNAMELEN = 29;
        public ushort size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = XT_EXTENSION_MAXNAMELEN)]
        public string name;
        public byte revision;
    }
};