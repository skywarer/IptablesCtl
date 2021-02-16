using System.Runtime.InteropServices;


namespace IptablesCtl.Native
{
    /*xt_tos_match_info*/
    [StructLayout(LayoutKind.Sequential)]
    public struct TosOptions
    {
        public const byte XT_TOS_INV = 0x01;        
        
        public byte mask,value,invert;
    }
};