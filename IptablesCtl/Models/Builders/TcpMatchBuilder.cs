using System;
using System.Linq;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{

    public sealed class TcpMatchBuilder : OptionsBuilder<TcpOptions, Match>
    {
        public static readonly (byte flag, string name)[] TCP_FLAGS = { (16, "ACK"), (2, "SYN"), (1, "FIN"), (32, "URG"), (8, "PSH"), (4, "RST") };
        public const string SPORT_OPT = "--sport";
        public const string DPORT_OPT = "--dport";
        public const string TCP_FLAGS_OPT = "--tcp-flags";
        public const string TCP_OPT = "--tcp-option";
        static string[] TcpFlagsToString(byte flag) => flag switch
        {
            0 => new string[] { "NONE" },
            var f when f >= 63 => new string[] { "ALL" },
            _ => TCP_FLAGS.Where(f => (f.flag & flag) > 0).Select(f => f.name).ToArray()
        };

        static byte NameToTcpFlag(string name) => name switch
        {
            var n when n.Equals("NONE", StringComparison.OrdinalIgnoreCase) => 0,
            var n when n.Equals("ALL", StringComparison.OrdinalIgnoreCase) => 63,
            _ => TCP_FLAGS.First(f => f.name.Equals(name, StringComparison.OrdinalIgnoreCase)).flag
        };

        public TcpMatchBuilder() { }
        public TcpMatchBuilder(TcpOptions options)
        {
            SetOptions(options);
        }
        public TcpMatchBuilder(Match match) : base(match)
        {

        }

        public override void SetOptions(TcpOptions options)
        {
            //source-port
            if (options.spts[0] > 0 || options.spts[1] < ushort.MaxValue)
            {
                SetSrcPort(options.spts[0], options.spts[1], (options.invflags & TcpOptions.XT_TCP_INV_SRCPT) > 0);
            }
            //destination-port
            if (options.dpts[0] > 0 || options.dpts[1] < ushort.MaxValue)
            {
                SetDstPort(options.dpts[0], options.dpts[1], (options.invflags & TcpOptions.XT_TCP_INV_DSTPT) > 0);
            }
            //tcp-flags
            if (options.flg_cmp > 0 || options.flg_mask > 0)
            {
                var cmp = TcpFlagsToString(options.flg_cmp);
                var mask = TcpFlagsToString(options.flg_mask);
                SetFlags(mask, cmp, (options.invflags & TcpOptions.XT_TCP_INV_FLAGS) > 0);
            }
            //tcp-option
            if (options.options > 0)
            {
                SetOption(options.options, (options.invflags & TcpOptions.XT_TCP_INV_OPTION) > 0);
            }
        }

        public TcpMatchBuilder SetSrcPort(ushort port, bool invert = false)
        {
            AddProperty(SPORT_OPT.ToOptionName(invert), port);
            return this;
        }
        public TcpMatchBuilder SetSrcPort(ushort minPort, ushort maxPort, bool invert = false)
        {
            AddRangeProperty(SPORT_OPT.ToOptionName(invert), minPort, maxPort, ':');
            return this;
        }
        public TcpMatchBuilder SetDstPort(ushort port, bool invert = false)
        {
            AddProperty(DPORT_OPT.ToOptionName(invert), port);
            return this;
        }
        public TcpMatchBuilder SetDstPort(ushort minPort, ushort maxPort, bool invert = false)
        {
            AddRangeProperty(DPORT_OPT.ToOptionName(invert), minPort, maxPort, ':');
            return this;
        }
        public TcpMatchBuilder SetFlags(string[] mask, string[] cmp, bool invert = false)
        {
            var allFlags = TCP_FLAGS.Select(tf => tf.name).Append("ALL").Append("NONE").ToArray();
            if (mask.Any(m => !allFlags.Contains(m, StringComparer.OrdinalIgnoreCase)))
                throw new FormatException($"mask:{mask}");
            if (cmp.Any(m => !allFlags.Contains(m, StringComparer.OrdinalIgnoreCase)))
                throw new FormatException($"cmp:{cmp}");
            var maskValue = string.Join(',', mask);
            var cmpValue = string.Join(',', cmp);
            // mask is the first in this case
            AddMaskedProperty(TCP_FLAGS_OPT.ToOptionName(invert), maskValue, cmpValue, ' ');
            return this;
        }
        public TcpMatchBuilder SetSyn(bool invert = false)
        {
            return SetFlags(new string[] { "syn", "rst", "fin", "ack" }, new string[] { "syn" }, invert);//
        }
        public TcpMatchBuilder SetOption(byte option, bool invert)
        {
            AddProperty(TCP_OPT.ToOptionName(invert), option);
            return this;
        }
        public override Match Build()
        {
            return new Match(MatchTypes.TCP, false, Properties);
        }

        public override TcpOptions BuildNative()
        {
            var match = Build();
            TcpOptions opt = new TcpOptions();
            //source-port
            if (match.TryGetOption(SPORT_OPT, out var options))
            {
                var range = options.Value.ToRangeProperty(':');
                opt.spts = new ushort[] { ushort.Parse(range.Left), ushort.Parse(range.Rigt) };
                if (options.Inverted) opt.invflags |= TcpOptions.XT_TCP_INV_SRCPT;
            }
            else
            {
                opt.spts = new ushort[]{ushort.MinValue, ushort.MaxValue};
            }
            //destination-port
            if (match.TryGetOption(DPORT_OPT, out options))
            {
                var range = options.Value.ToRangeProperty(':');
                opt.dpts = new ushort[] { ushort.Parse(range.Left), ushort.Parse(range.Rigt) };
                if (!match.ContainsKey(DPORT_OPT)) opt.invflags |= TcpOptions.XT_TCP_INV_DSTPT;
            }
            else
            {
                opt.dpts = new ushort[]{ushort.MinValue, ushort.MaxValue};
            }
            //tcp-flags
            if (match.TryGetOption(TCP_FLAGS_OPT, out options))
            {
                var masked = options.Value.ToMaskedProperty(' ');
                if (string.IsNullOrEmpty(masked.Mask) || string.IsNullOrEmpty(masked.Value))
                    throw new FormatException($"tcp_flags {options}");
                opt.flg_cmp = (byte)masked.Mask.Split(',').Aggregate(0, (flags, name) => flags | NameToTcpFlag(name));
                opt.flg_mask = (byte)masked.Value.Split(',').Aggregate(0, (flags, name) => flags | NameToTcpFlag(name));
                if (options.Inverted) opt.invflags |= TcpOptions.XT_TCP_INV_FLAGS;
            }
            //tcp-option
            if (match.TryGetOption(TCP_OPT, out options))
            {
                opt.options = byte.Parse(options.Value);
                if (options.Inverted) opt.invflags |= TcpOptions.XT_TCP_INV_OPTION;
            }
            return opt;
        }
    }

}