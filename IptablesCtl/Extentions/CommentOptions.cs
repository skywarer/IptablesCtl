using System.Runtime.InteropServices;


namespace IptablesCtl.Native.Extentions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CommentOptions
    {
        public const int XT_MAX_COMMENT_LEN = 256;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = XT_MAX_COMMENT_LEN)]
        public string comment;
    }
}