using System.Runtime.InteropServices;
using System;

namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*ipt_reject_info*/
    public struct RejectOptions
    {
        public RejectWith with;
    }

};