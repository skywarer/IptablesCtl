using System;
namespace IptablesCtl.Native
{
    [Flags]
    public enum OWNER_FLAGS
    {
        XT_OWNER_UID = 1, XT_OWNER_GID = 2, XT_OWNER_SOCKET = 4
    }
}