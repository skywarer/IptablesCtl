using System.Runtime.InteropServices;
using System;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RejectOptions
    {
        public RejectWith with;
    }

};