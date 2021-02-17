using System;
using System.Linq;
using IptablesCtl.Native;
using System.Text.RegularExpressions;
namespace IptablesCtl.Models.Builders
{
    public sealed class MultiportMatchBuilder : OptionsBuilder<MultiportOptions, Match>
    {
        public const byte Revision = 1;
        public const string SOURCE_PORT_OPT = "--source-port";
        public const string DESTINATION_PORT_OPT = "--destination-port";
        public const string PORT_OPT = "--port";
        static Regex rangeRegex = new Regex(@"[1-9]\d{0,5}(?:\:[1-9]\d{0,5})?(?:\,[1-9]\d{0,5}(?:\:[1-9]\d{0,5})?){0,14}");

        public MultiportMatchBuilder()
        {

        }
        public MultiportMatchBuilder(Match match) : base(match)
        {

        }
        public MultiportMatchBuilder(MultiportOptions options)
        {
            SetOptions(options);
        }

        public override void SetOptions(MultiportOptions options)
        {
            var ports = options.ports.Take(options.count).ToArray();
            var pflags = options.pflags.Take(options.count).ToArray();
            var invert = (options.invert & MultiportOptions.XT_MULTIPORTS_INV) > 0;
            switch (options.flags)
            {
                case MultiportOptions.XT_MULTIPORT_SOURCE:
                    SetSrcPorts(ports, pflags, invert); break;
                case MultiportOptions.XT_MULTIPORT_DESTINATION:
                    SetDstPorts(ports, pflags, invert); break;
                case MultiportOptions.XT_MULTIPORT_EITHER:
                    SetPorts(ports, pflags, invert); break;
            }
        }

        private void SetPorts(OptionName name, string range)
        {
            if (!rangeRegex.IsMatch(range)) throw new FormatException($"range:{range}");
            AddProperty(name, range);
        }
        private void SetPorts(OptionName name, ushort[] ports, byte[] flags)
        {
            var range = ports.Zip(flags)
                .Aggregate(String.Empty, (x, y) => string.Concat(x, y.First, y.Second > 0 ? ':' : ',')).TrimEnd(',');
            SetPorts(name, range);
        }

        public MultiportMatchBuilder SetSrcPorts(ushort[] ports, byte[] flags, bool invert = false)
        {
            SetPorts(SOURCE_PORT_OPT.ToOptionName(invert), ports, flags);
            return this;
        }
        public MultiportMatchBuilder SetSrcPorts(string range, bool invert = false)
        {
            SetPorts(SOURCE_PORT_OPT.ToOptionName(invert), range);
            return this;
        }

        public MultiportMatchBuilder SetDstPorts(ushort[] ports, byte[] flags, bool invert = false)
        {
            SetPorts(DESTINATION_PORT_OPT.ToOptionName(invert), ports, flags);
            return this;
        }
        public MultiportMatchBuilder SetDstPorts(string range, bool invert = false)
        {
            SetPorts(DESTINATION_PORT_OPT.ToOptionName(invert), range);
            return this;
        }
        public MultiportMatchBuilder SetPorts(ushort[] ports, byte[] flags, bool invert = false)
        {
            SetPorts(PORT_OPT.ToOptionName(invert), ports, flags);
            return this;
        }
        public MultiportMatchBuilder SetPorts(string range, bool invert = false)
        {
            SetPorts(PORT_OPT.ToOptionName(invert), range);
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.MULTIPORT, true, Properties, Revision);
        }


        public override MultiportOptions BuildNative()
        {
            var match = Build();
            MultiportOptions opt = new MultiportOptions();
            //source-port
            if (match.TryGetOption(SOURCE_PORT_OPT, out var options))
            {
                opt.flags |= MultiportOptions.XT_MULTIPORT_SOURCE;
            }
            else if (match.TryGetOption(DESTINATION_PORT_OPT, out options))
            {
                opt.flags |= MultiportOptions.XT_MULTIPORT_DESTINATION;
            }
            else if (match.TryGetOption(PORT_OPT, out options))
            {
                opt.flags |= MultiportOptions.XT_MULTIPORT_EITHER;
            }
            else
            {
                throw new ArgumentNullException("port options");
            }
            opt.ports = options.Value.ParseMultiports(MultiportOptions.XT_MULTI_PORTS);
            opt.pflags = options.Value.ParseMultiportsFlag(MultiportOptions.XT_MULTI_PORTS);
            opt.count = (byte)opt.ports.Count(p => p > 0);
            if (options.Inverted) opt.invert |= MultiportOptions.XT_MULTIPORTS_INV;
            return opt;
        }

    }

}