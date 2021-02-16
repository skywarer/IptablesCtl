using System;
using System.Linq;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class IcmpMatchBuilder : OptionsBuilder<IcmpOptions, Match>
    {

        public static (byte type, byte code, string name)[] ICMP_TYPES = {
            (0xFF,0,"any"),
            (0,0,"echo-reply"),
            (3,0,"network-unreachable"),
            (3,1,"host-unreachable"),
            (3,2,"protocol-unreachable"),
            (3,3,"port-unreachable"),
            (3,4,"fragmentation-needed"),
            (3,5,"source-route-failed"),
            (3,6,"network-unknown"),
            (3,7,"host-unknown"),
            (3,9,"network-prohibited"),
            (3,10,"host-prohibited"),
            (3,11,"TOS-network-unreachable"),
            (3,12,"TOS-host-unreachable"),
            (3,13,"communication-prohibited"),
            (3,14,"host-precedence-violation"),
            (3,15,"precedence-cutoff"),
            (4,0,"source-quench"),
            (3,0,"network-redirect"),
            (3,1,"host-redirect"),
            (3,2,"TOS-network-redirect"),
            (3,3,"TOS-host-redirect"),
            (8,0,"echo-request"),
            (9,0,"router-advertisement"),
            (10,0,"router-solicitation"),
            (11,0,"ttl-zero-during-transit"),
            (11,1,"ttl-zero-during-reassembly"),
            (12,0,"ip-header-bad"),
            (12,1,"required-option-missing"),
            (13,0,"timestamp-request"),
            (14,0,"timestamp-reply"),
            (17,0,"address-mask-request"),
            (18,0,"address-mask-reply")
        };

        public const string TYPE_OPT = "--icmp-type";

        public IcmpMatchBuilder() { }

        public IcmpMatchBuilder(Match match) : base(match)
        {

        }

        public IcmpMatchBuilder(IcmpOptions options)
        {
            SetOptions(options);
        }

        public override void SetOptions(IcmpOptions options)
        {
            SetIcmpType(options.type,
                options.code[0],
                (options.invflags & IcmpOptions.IPT_ICMP_INV) > 0);
        }

        public IcmpMatchBuilder SetIcmpType(byte icmpType, bool invert = false)
        {
            var key = TYPE_OPT.ToOptionName(invert);
            var name = ICMP_TYPES.FirstOrDefault(p => p.type == icmpType && p.code == 0).name;
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(icmpType));
            }
            AddProperty(key, name);
            return this;
        }
        public IcmpMatchBuilder SetIcmpType(string icmpName, bool invert = false)
        {
            var key = TYPE_OPT.ToOptionName(invert);
            if (!ICMP_TYPES.Any(p => StringComparer.OrdinalIgnoreCase.Equals(p.name, icmpName)))
            {
                throw new ArgumentException(nameof(icmpName));
            }
            AddProperty(key, icmpName);
            return this;
        }

        public IcmpMatchBuilder SetIcmpType(byte icmpType, byte icmpCode, bool invert = false)
        {
            var key = TYPE_OPT.ToOptionName(invert);
            var name = ICMP_TYPES.FirstOrDefault(p => p.type == icmpType && p.code == icmpCode).name;
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(icmpType)}/{nameof(icmpCode)}");
            }
            AddProperty(key, name);
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.ICMP, false, Properties);
        }

        public override IcmpOptions BuildNative()
        {
            IcmpOptions opts = new IcmpOptions();
            var match = Build();
            //source
            if (match.TryGetOption(TYPE_OPT, out var topt))
            {
                if (string.IsNullOrEmpty(topt.Value))
                    throw new ArgumentException($"empty value for options [!]{TYPE_OPT}");
                var icmpType = ICMP_TYPES.FirstOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.name, topt.Value));
                opts.type = icmpType.type;
                // length must be 2
                opts.code = new byte[] { icmpType.code, icmpType.code };
                if (topt.Inverted) opts.invflags |= IcmpOptions.IPT_ICMP_INV;
            }
            return opts;
        }
    }
}