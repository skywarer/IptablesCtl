using System;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class DNatTargetBuilder : OptionsBuilder<NatOptions, Target>
    {
        public const string TO_DESTINATION_OPT = "--to-destination";
        public const string RANDOM_OPT = "--random";
        public const string PERSISTENT_OPT = "--persistent";

        public DNatTargetBuilder()
        {
        }

        public DNatTargetBuilder(NatOptions options)
        {
            SetOptions(options);
        }

        public DNatTargetBuilder(Target target) : base(target)
        {
        }

        public override void SetOptions(NatOptions options)
        {
            if (options.range_size > 0)
            {
                var range = options.ranges[0];
                var minIp = range.min_ip > 0 ? ReverceEndian(range.min_ip) : 0;
                var maxIp = range.max_ip > 0 ? ReverceEndian(range.max_ip) : 0;
                ushort minP = range.min_proto > 0 ? ReverceEndian(range.min_proto) : 0;
                ushort maxP = range.max_proto > 0 ? ReverceEndian(range.max_proto) : 0;
                var rangeDef = ((range.flags & NatRange.NF_NAT_RANGE_MAP_IPS) > 0,
                    (range.flags & NatRange.NF_NAT_RANGE_PROTO_SPECIFIED) > 0);
                switch (rangeDef)
                {
                    case (true, true):
                        SetDestination(minIp, maxIp, minP, maxP);
                        break;
                    case (true, false):
                        SetDestinationWithIp(minIp, maxIp);
                        break;
                    case (false, true):
                        SetDestinationWithProto(minP, maxP);
                        break;
                }

                //random
                if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_RANDOM) > 0)
                {
                    SetRandom();
                }

                //persistent
                if ((range.flags & NatRange.NF_NAT_RANGE_PERSISTENT) > 0)
                {
                    SetPersistent();
                }
            }
        }

        public DNatTargetBuilder SetDestinationWithIp(uint min_ip, uint max_ip)
        {
            AddRangeProperty(TO_DESTINATION_OPT.ToOptionName(), ToIp4String(min_ip), ToIp4String(max_ip), '-');
            return this;
        }

        public DNatTargetBuilder SetDestination(uint min_ip, uint max_ip, ushort min_proto, ushort max_proto)
        {
            var ipRange = new RangeProperty<string>(ToIp4String(min_ip), ToIp4String(max_ip), '-').ToString();
            var protoRange = new RangeProperty<ushort>(min_proto, max_proto, '-').ToString();
            AddMaskedProperty(TO_DESTINATION_OPT.ToOptionName(), ipRange, protoRange, ':');
            return this;
        }

        public DNatTargetBuilder SetDestination(string minIp, string maxIp, ushort min_proto, ushort max_proto)
        {
            SetDestination(minIp.ParseIpv4(), maxIp.ParseIpv4(), min_proto, max_proto);
            return this;
        }

        public DNatTargetBuilder SetDestinationWithProto(ushort min_proto, ushort max_proto)
        {
            var protoRange = new RangeProperty<ushort>(min_proto, max_proto, '-').ToString();
            AddMaskedProperty(TO_DESTINATION_OPT.ToOptionName(), string.Empty, protoRange, ':');
            return this;
        }

        public DNatTargetBuilder SetRandom()
        {
            AddProperty(RANDOM_OPT.ToOptionName());
            return this;
        }

        public DNatTargetBuilder SetPersistent()
        {
            AddProperty(PERSISTENT_OPT.ToOptionName());
            return this;
        }

        public override Target Build()
        {
            return new Target(TargetTypes.DNAT, Properties);
        }

        public override NatOptions BuildNative()
        {
            Target dnat = Build();
            NatOptions options = new NatOptions();
            options.ranges = new NatRange[] {new NatRange()};
            options.range_size = 1;
            if (dnat.TryGetValue(TO_DESTINATION_OPT, out var src))
            {
                var range = src.ParseIpProtoRange();
                options.ranges[0].min_ip = ReverceEndian(range.minIp);
                options.ranges[0].max_ip = ReverceEndian(range.maxIp);
                options.ranges[0].min_proto = ReverceEndian(range.minP);
                options.ranges[0].max_proto = ReverceEndian(range.maxP);
                if (options.ranges[0].min_ip > 0)
                {
                    options.ranges[0].flags |= NatRange.NF_NAT_RANGE_MAP_IPS;
                }

                if (options.ranges[0].min_proto > 0)
                {
                    options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_SPECIFIED;
                }
            }

            if (dnat.ContainsKey(RANDOM_OPT))
            {
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_RANDOM;
            }

            if (dnat.ContainsKey(PERSISTENT_OPT))
            {
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_RANDOM_FULLY;
            }

            return options;
        }
    }
}