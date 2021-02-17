using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    /*xt_tos_match_info*/
    public struct TosOptions
    {
        public const byte XT_TOS_INV = 0x01;        
        
        public byte mask,value,invert;
    }
};