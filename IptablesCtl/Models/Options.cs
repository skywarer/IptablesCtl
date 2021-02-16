using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;

namespace IptablesCtl.Models
{
    [JsonConverter(typeof(Serialization.OptionsConverter))]
    public class Options : ReadOnlyDictionary<string, string>
    {
        public string this[OptionName name]
        {
            get { return this[$"{name}"]; }
        }

        public Options(IDictionary<string, string> prop) : base(prop ?? ImmutableDictionary<string, string>.Empty)
        {

        }

        /// <summary>
        /// Take first one of exists option value
        /// </summary>
        /// <param name="key">name of option</param>
        /// <param name="invkey">inverse name of option</param>
        /// <param name="value">value of option</param>
        /// <returns>true if one of key exists</returns>
        public bool TryGetValue(string key, string invkey, out string value)
        {
            return TryGetValue(key, out value) || TryGetValue(invkey, out value);
        }


        /// <summary>
        /// Take first one of exists option value
        /// </summary>
        /// <param name="key">name of option</param>
        /// <param name="invkey">inverse name of option</param>
        /// <param name="value">value of option with inverse flag</param>
        /// <returns>true if one of key exists</returns>
        public bool TryGetOption(string key, string invkey, out OptionValue optValue)
        {
            optValue = new OptionValue(string.Empty, false);
            if (TryGetValue(key, out var value))
            {
                optValue = new OptionValue(value, false);
                return true;
            }
            else if (TryGetValue(invkey, out value))
            {
                optValue = new OptionValue(value, true);
                return true;
            }
            return false;
        }

        public bool TryGetOption(string key, out OptionValue optValue)
        {
            string invkey = new OptionName(key, true).ToString();
            return TryGetOption(key, invkey, out optValue);
        }

        public bool ContainsOption(string key)
        {
            return ContainsKey(key) || ContainsKey(key.ToOptionName(true).ToString());
        }

        public override string ToString()
        {
            return String.Join(' ', this.Select(entry => $"{entry.Key} {entry.Value}"));
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