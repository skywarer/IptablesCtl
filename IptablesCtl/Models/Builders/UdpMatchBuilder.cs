using System;
using System.Linq;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{

    public sealed class UdpMatchBuilder : OptionsBuilder<UdpOptions, Match>
    {
        public const string SPORT_OPT = "--sport";
        public const string DPORT_OPT = "--dport";
        public UdpMatchBuilder() { }
        public UdpMatchBuilder(UdpOptions options)
        {
            SetOptions(options);
        }
        public UdpMatchBuilder(Match match) : base(match)
        {
        }

        public override void SetOptions(UdpOptions options)
        {
            //source-port
            if (options.spts[0] > 0 || options.spts[1] < ushort.MaxValue)
            {
                SetSrcPort(options.spts[0], options.spts[1], (options.invflags & UdpOptions.XT_UDP_INV_SRCPT) > 0);
            }
            //destination-port
            if (options.dpts[0] > 0 || options.dpts[1] < ushort.MaxValue)
            {
                SetDstPort(options.dpts[0], options.dpts[1], (options.invflags & UdpOptions.XT_UDP_INV_DSTPT) > 0);
            }
        }

        public UdpMatchBuilder SetSrcPort(ushort port, bool invert = false)
        {
            AddProperty(SPORT_OPT.ToOptionName(invert), port);
            return this;
        }
        public UdpMatchBuilder SetSrcPort(ushort minPort, ushort maxPort, bool invert = false)
        {
            AddRangeProperty(SPORT_OPT.ToOptionName(invert), minPort, maxPort, ':');
            return this;
        }
        public UdpMatchBuilder SetDstPort(ushort port, bool invert = false)
        {
            AddProperty(DPORT_OPT.ToOptionName(invert), port);
            return this;
        }
        public UdpMatchBuilder SetDstPort(ushort minPort, ushort maxPort, bool invert = false)
        {
            AddRangeProperty(DPORT_OPT.ToOptionName(invert), minPort, maxPort, ':');
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.UDP, false, Properties);
        }

        public override UdpOptions BuildNative()
        {
            var match = Build();
            UdpOptions opt = UdpOptions.Default();
            //source-port
            if (match.TryGetOption(SPORT_OPT, out var options))
            {
                var range = options.Value.ToRangeProperty(':');
                opt.spts = new ushort[] { ushort.Parse(range.Left), ushort.Parse(range.Rigt) };
                if (options.Inverted) opt.invflags |= UdpOptions.XT_UDP_INV_SRCPT;
            }
            //destination-port
            if (match.TryGetOption(DPORT_OPT, out options))
            {
                var range = options.Value.ToRangeProperty(':');
                opt.dpts = new ushort[] { ushort.Parse(range.Left), ushort.Parse(range.Rigt) };
                if (!match.ContainsKey(DPORT_OPT)) opt.invflags |= UdpOptions.XT_UDP_INV_DSTPT;
            }
            return opt;
        }
    }
}