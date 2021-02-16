
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace IptablesCtl.Models.Builders
{
    public abstract class OptionsBuilder<N, M> where N : struct where M : Options
    {
        protected IDictionary<string, string> Properties { get; init; }
        public OptionsBuilder()
        {
            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        public OptionsBuilder(IDictionary<string, string> prop)
        {
            Properties = new Dictionary<string, string>(prop);
        }

        protected void AddProperty<V>(OptionName name, V value) where V : notnull
        {
            Properties[$"{name}"] = $"{value}";
        }

        protected void AddProperty(OptionName name, string value = null)
        {
            Properties[$"{name}"] = value ?? String.Empty;
        }

        protected void AddMaskedProperty(OptionName name, string value, string mask, char delim)
        {
            var masked = new MaskedProperty(value, mask, delim);
            AddProperty(name, masked);
        }

        protected void AddMaskedProperty<V>(OptionName name, V value, V mask, char delim)
            where V : IComparable<V>
        {
            var masked = new MaskedProperty<V>(value, mask, delim);
            AddProperty(name, masked);
        }


        protected void AddRangeProperty<V>(OptionName name, V left, V right, char delim)
            where V : IComparable<V>
        {
            var range = new RangeProperty<V>(left, right, delim);
            AddProperty(name, range);
        }

        public abstract void SetOptions(N options);

        public abstract M Build();

        public abstract N BuildNative();
        public static readonly int _WORDLEN = Marshal.SizeOf<long>();

        public static int Align(int size)
        {
            return ((size + (_WORDLEN - 1)) & ~(_WORDLEN - 1));
        }

        public static readonly int HeaderLen = Marshal.SizeOf<Native.Header>();

        public static string ToIp4String(uint addr)
        {
            return $"{addr >> 24}.{(addr >> 16) & 0xFF}.{(addr >> 8) & 0xFF}.{addr & 0xFF}";
        }
        public static byte Ip4MaskLen(uint mask)
        {
            byte len = 32;
            while (len >= 0 && (mask & 1) == 0) { len--; mask >>= 1; }
            return len;
        }

        public static ushort ReverceEndian(ushort value)
        {
            return (ushort)((value << 8) | (value >> 8));
        }

        public static uint ReverceEndian(uint value)
        {
            return (uint)((value >> 24) | (value << 24) |
                (value >> 8 & 0xFF00) | (value << 8 & 0xFF0000));
        }

    }
}