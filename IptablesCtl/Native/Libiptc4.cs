using System;
using System.Runtime.InteropServices;

namespace IptablesCtl.Native
{
    internal static class Libiptc4
    {
        [DllImport("libip4tc")]
        public static extern IntPtr iptc_init(string tablename);      
        
        [DllImport("libip4tc")]
        public static extern void iptc_free(IntPtr handle);

        [DllImport("libip4tc")]
        public static extern IntPtr iptc_strerror(int err);

        [DllImport("libip4tc", CharSet = CharSet.Ansi)]
        public static extern IntPtr iptc_first_chain(IntPtr handle);

        [DllImport("libip4tc", CharSet = CharSet.Ansi)]
        public static extern IntPtr iptc_next_chain(IntPtr handle);
        
        [DllImport("libio4tc", CharSet = CharSet.Ansi)]
        public static extern IntPtr iptc_read_counter([MarshalAs(UnmanagedType.LPStr, SizeConst = 32)] string label, uint num, IntPtr handle);        

        [DllImport("libio4tc")]
        public static extern int iptc_builtin(string chain, IntPtr handle);

        [DllImport("libio4tc")]
        public static extern IntPtr iptc_first_rule(string chain, IntPtr handle);

        [DllImport("libio4tc")]
        public static extern IntPtr iptc_next_rule(IntPtr prev, IntPtr handle);

        [DllImport("libio4tc")]
        public static extern IntPtr iptc_get_target(IntPtr entry, IntPtr handle);

        [DllImport("libio4tc", SetLastError = true)]
        public static extern int iptc_commit(IntPtr handle);

        [DllImport("libio4tc", SetLastError = true)]
        public static extern int iptc_append_entry([MarshalAs(UnmanagedType.LPStr, SizeConst = 32)] string label, IntPtr entry, IntPtr handle);
        
        [DllImport("libio4tc", SetLastError = true)]
        public static extern int iptc_replace_entry([MarshalAs(UnmanagedType.LPStr, SizeConst = 32)] string label, IntPtr entry, uint num, IntPtr handle);
        
        [DllImport("libio4tc", SetLastError = true)]
        public static extern int iptc_delete_num_entry([MarshalAs(UnmanagedType.LPStr, SizeConst = 32)] string label, uint num, IntPtr handle);

    }

}