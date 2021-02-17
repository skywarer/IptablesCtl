using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace IptablesCtl.Models
{
    public static class PropertyExtentions
    {
        static Regex cidrRegex = new Regex(@"(?<addr>\d{1,3}(?:\.\d{1,3}){0,3})(?:\/(?<mask>\d{1,2}))?");

        public static bool IsCidr(this string cidr)
        {
            return cidrRegex.IsMatch(cidr);
        }

        public static OptionName ToOptionName(this string name, bool inverted = false)
        {
            return new OptionName(name, inverted);
        }


        public static MaskedProperty ToMaskedProperty(this string prop, char delim)
        {
            return prop.ToMaskedProperty(delim, null);
        }
        public static MaskedProperty ToMaskedProperty(this string prop, char delim, string defMask)
        {
            prop = prop.TrimEnd(delim);
            var inx = prop.LastIndexOf(delim);
            return inx > 0 ?
                new MaskedProperty(prop.Substring(0, inx), prop.Substring(inx + 1), delim)
                : new MaskedProperty(prop, defMask, delim);
        }

        public static RangeProperty<string> ToRangeProperty(this string prop, char delim)
        {
            prop = prop.TrimEnd(delim);
            var inx = prop.LastIndexOf(delim);
            return inx > 0 ?
                new RangeProperty<string>(prop.Substring(0, inx), prop.Substring(inx + 1), delim)
                : new RangeProperty<string>(prop, prop, delim);
        }

        public static uint ParseIpv4(this string prop)
        {
            var ipv4 = prop.Trim('.').Split('.').Select(s => byte.Parse(s)).ToArray();
            Array.Resize(ref ipv4, sizeof(uint));
            return BitConverter.ToUInt32(ipv4.Reverse().ToArray(), 0);
        }

        public static byte[] ParseMacaddr(this string prop)
        {
            return prop.Split(':').Select(s => byte.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray();
        }

        public static (uint minIp, uint maxIp, ushort minP, ushort maxP) ToIpProtoRange(this string prop)
        {
            uint minIp = 0, maxIp = 0;
            ushort minP = 0, maxP = 0;
            var srcMask = prop.ToMaskedProperty(':');
            // ip range
            var sortedIp = srcMask.Value.Split('-').Where(v => !string.IsNullOrEmpty(v))
                .Select(v => v.ParseIpv4()).OrderBy(v => v).ToArray();
            if (sortedIp.Any())
            {
                minIp = sortedIp[0];
                maxIp = sortedIp.Length > 1 ? sortedIp[1] : sortedIp[0];
            }
            // proto range
            var sortedProto = srcMask.Mask.Split('-')
                .Select(v => ushort.Parse(v)).OrderBy(v => v).ToArray();
            if (sortedProto.Any())
            {
                minP = sortedProto[0];
                maxP = sortedProto.Length > 1 ? sortedProto[1] : minP;
            }
            return (minIp, maxIp, minP, maxP);
        }

        public static ushort[] ParseMultiports(this string prop, int max_size)
        {
            var ports = prop.Split(',', ':').Select(p => ushort.Parse(p)).ToArray();
            Array.Resize(ref ports, max_size);
            return ports;
        }

        public static byte[] ParseMultiportsFlag(this string prop, int max_size, params char[] delims)
        {
            // restore last 0
            var pflags = prop.Concat(",").Where(c => Char.IsPunctuation(c)).
                Select(c => c switch { ':' => (byte)1, _ => (byte)0, })
                .ToArray();
            Array.Resize(ref pflags, max_size);
            return pflags;
        }
    }
}