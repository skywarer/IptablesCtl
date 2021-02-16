#define DEBUG
using System;
using System.Linq;
using IptablesCtl.Native;
using System.Collections.Generic;
namespace IptablesCtl.Models.Builders
{
    public sealed class RuleBuilder : OptionsBuilder<IptEntry, Rule>
    {
        public static readonly (byte code, string name)[] PROTOCOLS = {
            (0,"ip"),      // internet protocol, pseudo protocol number
            (0,"hopopt"),  // IPv6 Hop-by-Hop Option [RFC1883]
            (1, "icmp"),    // internet control message protocol
            (2, "igmp"),    // Internet Group Management
            (3,"ggp"),     // gateway-gateway protocol
            (4,"ipencap"), // IP encapsulated in IP (officially ``IP'')
            (5,"st"),     // ST datagram mode
            (6,"tcp"),	    // transmission control protocol
            (8,"egp"),     // exterior gateway protocol
            (9,"igp"),     // any private interior gateway (Cisco)
            (12,"pup"),    // PARC universal packet protocol
            (17,"udp"),    // user datagram protocol
            (20,"hmp"),    // host monitoring protocol
            (22,"xns-idp"), // Xerox NS IDP
            (27,"rdp"),    //	"reliable datagram" protocol
            (29,"iso-tp4"), // ISO Transport Protocol class 4 [RFC905]
            (33,"dccp"),   // Datagram Congestion Control Prot. [RFC4340]
            (36,"xtp"),    // Xpress Transfer Protocol
            (37,"ddp"),    // Datagram Delivery Protocol
            (38,"idpr-cmtp"), // IDPR Control Message Transport
            (41,"ipv6"),    //	Internet Protocol, version 6
            (43,"ipv6-route"), // Routing Header for IPv6
            (44,"ipv6-frag"), // Fragment Header for IPv6
            (45,"idrp"),   // Inter-Domain Routing Protocol
            (46,"rsvp"),   // Reservation Protocol
            (47,"gre"),    // General Routing Encapsulation
            (50,"esp"),    // Encap Security Payload [RFC2406]
            (51,"ah"),    //	Authentication Header [RFC2402]
            (57,"skip"),    // SKIP
            (58,"ipv6-icmp"), // ICMP for IPv6
            (59,"ipv6-nonxt"), // No Next Header for IPv6
            (60,"ipv6-opts"), // Destination Options for IPv6
            (73,"rspf"),   // Radio Shortest Path First (officially CPHB)
            (81,"vmtp"),   //	Versatile Message Transport
            (88,"eigrp"),  //	Enhanced Interior Routing Protocol (Cisco)
            (89,"ospf"),   // Open Shortest Path First IGP
            (93,"ax.25"),  //	AX.25 frames
            (94,"ipip"),   // IP-within-IP Encapsulation Protocol
            (97,"etherip"), // Ethernet-within-IP Encapsulation [RFC3378]
            (98,"encap"),  // Yet Another IP encapsulation [RFC1241]
            (99,"#"),      // any private encryption scheme
            (103,"pim"),   // Protocol Independent Multicast
            (108,"ipcomp"), // IP Payload Compression Protocol
            (112,"vrrp"),  // Virtual Router Redundancy Protocol [RFC5798]
            (115,"l2tp"),  // Layer Two Tunneling Protocol [RFC2661]
            (124,"isis"),  // IS-IS over IPv4
            (132,"sctp"),  // Stream Control Transmission Protocol
            (133,"fc"),    // Fibre Channel
            (135,"mobility-header"), // Mobility Support for IPv6 [RFC3775]
            (136,"udplite"), // UDP-Lite [RFC3828]
            (137,"mpls-in-ip"), // MPLS-in-IP [RFC4023]
            (138,"manet"), // MANET Protocols [RFC5498]
            (139,"hip"),   // Host Identity Protocol
            (140,"shim6"),  // Shim6 Protocol [RFC5533]
            (141,"wesp"),   // Wrapped Encapsulating Security Payload
            (142,"rohc")   // Robust Header Compression
        };
        public const string PROTOCOL_OPT = "-p";
        public const string SOURCE_OPT = "-s";
        public const string DESTINATION_OPT = "-d";
        public const string IN_INTERFACE_OPT = "-i";
        public const string OUT_INTERFACE_OPT = "-o";
        public const string FRAGMENT_OPT = "-f";

        IDictionary<string, Match> _matches;
        Target _target;

        public RuleBuilder()
        {
            _matches = new Dictionary<string, Match>();
        }

        public RuleBuilder(Rule rule) : base(rule)
        {
            _target = rule.Target;
            _matches = new Dictionary<string, Match>(rule.Matches.ToDictionary(e => e.Name));
        }

        public RuleBuilder(IptEntry entry, IList<Match> matches, Target target)
        {
            SetOptions(entry);
            _matches = new Dictionary<string, Match>(matches.ToDictionary(e => e.Name)); ;
            _target = target;
        }

        private byte[] NewIfaceMask(string name, bool isWildCard)
        {
            byte[] mask = new byte[IptIp.IFNAMSIZ];
            var length = isWildCard ? name.Length : IptIp.IFNAMSIZ;
            // signed part
            Array.Fill<byte>(mask, 255, 0, length);
            return mask;
        }

        public override void SetOptions(IptEntry options)
        {
            //source
            if (options.ip.src.addr > 0 || options.ip.src_mask.addr > 0)
            {
                var mask = ReverceEndian(options.ip.src_mask.addr);
                var ip = ReverceEndian(options.ip.src.addr);
                SetIp4Src($"{ToIp4String(ip)}/{Ip4MaskLen(mask)}", (options.ip.invflags & IptIp.IPT_INV_SRCIP) > 0);
            }
            //destination
            if (options.ip.dst.addr > 0 || options.ip.dst_mask.addr > 0)
            {
                var mask = ReverceEndian(options.ip.dst_mask.addr);
                var ip = ReverceEndian(options.ip.dst.addr);
                SetIp4Dst($"{ToIp4String(ip)}/{Ip4MaskLen(mask)}", (options.ip.invflags & IptIp.IPT_INV_DSTIP) > 0);
            }
            //in-interface
            if (!string.IsNullOrEmpty(options.ip.in_iface) &&
                options.ip.in_iface_mask.Length > 0 &&
                options.ip.in_iface_mask[0] > 0)
            {
                var maskLength = options.ip.in_iface_mask.TakeWhile(b => b != 0).Count();
                SetInInterface(options.ip.in_iface, maskLength <= options.ip.in_iface.Length, (options.ip.invflags & IptIp.IPT_INV_VIA_IN) > 0);
            }
            //out-interface
            if (!string.IsNullOrEmpty(options.ip.out_iface) &&
                options.ip.out_iface_mask.Length > 0 &&
                options.ip.out_iface_mask[0] > 0)
            {
                var maskLength = options.ip.out_iface_mask.TakeWhile(b => b != 0).Count();
                SetOutInterface(options.ip.out_iface, maskLength <= options.ip.out_iface.Length, (options.ip.invflags & IptIp.IPT_INV_VIA_OUT) > 0);
            }
            //protocol
            if (options.ip.proto > 0)
            {
                SetProto(options.ip.proto, (options.ip.invflags & IptIp.IPT_INV_PROTO) > 0);
            }
            //fragment
            if ((options.ip.flags & IptIp.IPT_F_FRAG) > 0)
            {
                SetFragment((options.ip.invflags & IptIp.IPT_INV_FRAG) > 0);
            }
        }

        public RuleBuilder SetInInterface(string name, bool isWildCard = false, bool invert = false)
        {
            var key = IN_INTERFACE_OPT.ToOptionName(invert);
            name = name.TrimEnd('+');
            var value = isWildCard ? name + '+' : name;
            AddProperty(key, value);
            return this;
        }

        public RuleBuilder SetOutInterface(string name, bool isWildCard = false, bool invert = false)
        {
            var key = OUT_INTERFACE_OPT.ToOptionName(invert);
            name = name.TrimEnd('+');
            var value = isWildCard ? name + '+' : name;
            AddProperty(key, value);
            return this;
        }

        public RuleBuilder SetIp4Src(string cidr, bool invert = false)
        {
            var key = SOURCE_OPT.ToOptionName(invert);
            if (!cidr.IsCidr())
            {
                throw new ArgumentException(nameof(cidr));
            }
            AddProperty(key, cidr);            
            return this;
        }

        public RuleBuilder SetIp4Dst(string cidr, bool invert = false)
        {
            var key = DESTINATION_OPT.ToOptionName(invert);
            if (!cidr.IsCidr())
            {
                throw new ArgumentException(nameof(cidr));
            }
            AddProperty(key, cidr);
            return this;
        }

        public RuleBuilder SetProto(string proto, bool invert = false)
        {
            var key = PROTOCOL_OPT.ToOptionName(invert);
            if (!PROTOCOLS.Any(p => p.name.Equals(proto, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException(nameof(proto));
            AddProperty(key, proto);
            return this;
        }
        public RuleBuilder SetProto(ushort proto, bool invert = false)
        {
            var key = PROTOCOL_OPT.ToOptionName(invert);
            var pName = PROTOCOLS.FirstOrDefault(p => p.code == proto).name;
            if (string.IsNullOrEmpty(pName))
                throw new ArgumentException(nameof(proto));
            AddProperty(key, pName);
            return this;
        }

        public RuleBuilder SetFragment(bool invert = false)
        {
            var key = FRAGMENT_OPT.ToOptionName(invert);
            AddProperty(key);
            return this;
        }

        public RuleBuilder AddMatch(Match match)
        {
            _matches[match.Name] = match;
            return this;
        }

        public RuleBuilder AddMatches(IEnumerable<Match> matches)
        {
            foreach (var m in matches)
            {
                AddMatch(m);
            }
            return this;
        }

        public RuleBuilder SetTarget(Target target)
        {
            _target = target;
            return this;
        }

        public Rule Accept()
        {
            SetTarget(new Target(TargetTypes.ACCEPT));
            return Build();
        }
        public Rule Drop()
        {
            SetTarget(new Target(TargetTypes.DROP));
            return Build();
        }

        public override Rule Build()
        {
            return new Rule(Properties, _matches.Values.ToList(), _target);
        }

        public override IptEntry BuildNative()
        {
            IptEntry entry = new IptEntry();
            var rule = Build();
            OptionValue option;
            //source
            if (rule.TryGetOption(SOURCE_OPT, out option))
            {
                var masked = option.Value.ToMaskedProperty('/', "32");
                entry.ip.src.addr = ReverceEndian(masked.Value.ParseIpv4());
                var mask = int.Parse(masked.Mask);
                entry.ip.src_mask.addr = ReverceEndian(0xFFFFFFFF << (32 - mask));
                if (option.Inverted) entry.ip.invflags |= IptIp.IPT_INV_SRCIP;
            }
            //destination
            if (rule.TryGetOption(DESTINATION_OPT, out option))
            {
                var masked = option.Value.ToMaskedProperty('/', "32");
                entry.ip.dst.addr = ReverceEndian(masked.Value.ParseIpv4());
                var mask = int.Parse(masked.Mask);
                entry.ip.dst_mask.addr = ReverceEndian(0xFFFFFFFF << (32 - mask));
                if (option.Inverted) entry.ip.invflags |= IptIp.IPT_INV_DSTIP;
            }
            //in-interface
            if (rule.TryGetOption(IN_INTERFACE_OPT, out option))
            {
                bool isWildCard = option.Value.EndsWith('+');
                var name = option.Value.TrimEnd('+');
                // set name + mask
                entry.ip.in_iface_mask = NewIfaceMask(name, isWildCard);
                entry.ip.in_iface = name;
                if (option.Inverted) entry.ip.invflags |= IptIp.IPT_INV_VIA_IN;
            }
            //out-interface
            if (rule.TryGetOption(OUT_INTERFACE_OPT, out option))
            {
                bool isWildCard = option.Value.EndsWith('+');
                var name = option.Value.TrimEnd('+');
                // set name + mask
                entry.ip.out_iface_mask = NewIfaceMask(name, isWildCard);
                entry.ip.out_iface = name;
                if (option.Inverted) entry.ip.invflags |= IptIp.IPT_INV_VIA_OUT;
            }
            //protocol
            if (rule.TryGetOption(PROTOCOL_OPT, out option))
            {
                if (ushort.TryParse(option.Value, out var proto))
                {
                    entry.ip.proto = proto;
                }
                else
                {
                    entry.ip.proto = PROTOCOLS.First(p => p.name.Equals(option.Value, StringComparison.OrdinalIgnoreCase)).code;
                }
                if (!rule.ContainsKey(PROTOCOL_OPT)) entry.ip.invflags |= IptIp.IPT_INV_PROTO;
            }
            //fragment
            if (rule.TryGetOption(FRAGMENT_OPT, out option))
            {
                entry.ip.flags |= IptIp.IPT_F_FRAG;
                if (option.Inverted) entry.ip.invflags |= IptIp.IPT_INV_FRAG;
            }
            return entry;
        }
    }
}