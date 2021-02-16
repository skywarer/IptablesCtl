using System.Runtime.InteropServices;

/*nf_nat.h.h nf_nat_ipv4_multi_range_compat*/
namespace IptablesCtl.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NatOptions
    {
        public uint range_size;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public NatRange[] ranges;
    }

};