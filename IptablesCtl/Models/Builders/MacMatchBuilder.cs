using System;
using System.Linq;
using IptablesCtl.Native;
using System.Text.RegularExpressions;
namespace IptablesCtl.Models.Builders
{
    public sealed class MacMatchBuilder : OptionsBuilder<MacOptions, Match>
    {
        public const string MAC_SOURCE_OPT = "--mac-source";
        static Regex macRegex = new Regex(@"[0-9a-fA-F]{2}(?:\:[0-9a-fA-F]{2}){5}");


        public MacMatchBuilder() { }

        public MacMatchBuilder(MacOptions options)
        {
            SetOptions(options);
        }

        public MacMatchBuilder(Match match) : base(match)
        {

        }

        public override void SetOptions(MacOptions options)
        {
            SetMacaddress(options.srcaddr, (options.invert & MacOptions.XT_MAC_INV) > 0);
        }

        public MacMatchBuilder SetMacaddress(byte[] mac, bool invert = false)
        {
            if (mac.Length != 6) throw new FormatException(nameof(mac));
            var macStr = string.Join(':', mac.Select(b => $"{b:X2}"));
            SetMacaddress(macStr, invert);
            return this;
        }

        public MacMatchBuilder SetMacaddress(string mac, bool invert = false)
        {
            if (!macRegex.IsMatch(mac)) throw new FormatException($"mac:{mac}");
            AddProperty(MAC_SOURCE_OPT.ToOptionName(invert), mac);
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.MAC, true, Properties);
        }

        public override MacOptions BuildNative()
        {
            throw new NotImplementedException();
        }

    }
}