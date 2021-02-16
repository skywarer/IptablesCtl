using System.Runtime.InteropServices;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OwnerOptions
    {
        public const byte XT_OWNER_UID = 1;
        public const byte XT_OWNER_GID = 2;
        public const byte XT_OWNER_SOCKET = 4;
        public uint uid_min, uid_max, gid_min, gid_max;
        public byte match, invert;
    }
};