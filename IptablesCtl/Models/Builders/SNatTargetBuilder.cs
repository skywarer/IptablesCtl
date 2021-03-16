using System;
using System.Linq;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class SNatTargetBuilder : OptionsBuilder<NatOptions, Target>
    {
        public const string TO_SOURCE_OPT = "--to-source";
        public const string RANDOM_OPT = "--random";
        public const string RANDOM_FULLY_OPT = "--random-fully";
        public const string PERSISTENT_OPT = "--persistent";

        public SNatTargetBuilder()
        {
        }

        public SNatTargetBuilder(NatOptions options)
        {
            SetOptions(options);
        }

        public SNatTargetBuilder(Target target) : base(target)
        {
        }

        public override void SetOptions(NatOptions options)
        {
            var range = options.ranges[0];
            var minIp = range.min_ip > 0 ? ReverceEndian(range.min_ip) : 0;
            var maxIp = range.max_ip > 0 ? ReverceEndian(range.max_ip) : 0;
            ushort minP = range.min_proto > 0 ? ReverceEndian(range.min_proto) : (ushort)0;
            ushort maxP = range.max_proto > 0 ? ReverceEndian(range.max_proto) : (ushort)0;
            var rangeDef = ((range.flags & NatRange.NF_NAT_RANGE_MAP_IPS) > 0,
                (range.flags & NatRange.NF_NAT_RANGE_PROTO_SPECIFIED) > 0);
            switch (rangeDef)
            {
                case (true, true):
                    SetSource(minIp, maxIp, minP, maxP);
                    break;
                case (true, false):
                    SetSourceWithIp(minIp, maxIp);
                    break;
                case (false, true):
                    SetSourceWithProto(minP, maxP);
                    break;
            }

            //random
            if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_RANDOM) > 0)
            {
                SetRandom();
            }

            if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_RANDOM_FULLY) > 0)
            {
                SetRandomFully();
            }

            //persistent
            if ((range.flags & NatRange.NF_NAT_RANGE_PERSISTENT) > 0)
            {
                SetPersistent();
            }
        }

        public SNatTargetBuilder SetSourceWithIp(uint min_ip, uint max_ip)
        {
            AddRangeProperty(TO_SOURCE_OPT.ToOptionName(), ToIp4String(min_ip), ToIp4String(max_ip), '-');
            return this;
        }

        public SNatTargetBuilder SetSource(uint min_ip, uint max_ip, ushort min_proto, ushort max_proto)
        {
            var ipRange = new RangeProperty<string>(ToIp4String(min_ip), ToIp4String(max_ip), '-').ToString();
            var protoRange = new RangeProperty<ushort>(min_proto, max_proto, '-').ToString();
            AddMaskedProperty(TO_SOURCE_OPT.ToOptionName(), ipRange, protoRange, ':');
            return this;
        }

        public SNatTargetBuilder SetSource(string minIp, string maxIp, ushort min_proto, ushort max_proto)
        {
            SetSource(minIp.ParseIpv4(), maxIp.ParseIpv4(), min_proto, max_proto);
            return this;
        }

        public SNatTargetBuilder SetSourceWithProto(ushort min_proto, ushort max_proto)
        {
            var protoRange = new RangeProperty<ushort>(min_proto, max_proto, '-').ToString();
            AddMaskedProperty(TO_SOURCE_OPT.ToOptionName(), string.Empty, protoRange, ':');
            return this;
        }

        public SNatTargetBuilder SetRandom()
        {
            AddProperty(RANDOM_OPT.ToOptionName());
            return this;
        }

        public SNatTargetBuilder SetRandomFully()
        {
            AddProperty(RANDOM_FULLY_OPT.ToOptionName());
            return this;
        }

        public SNatTargetBuilder SetPersistent()
        {
            AddProperty(PERSISTENT_OPT.ToOptionName());
            return this;
        }

        public override Target Build()
        {
            return new Target(TargetTypes.SNAT, Properties);
        }

        public override NatOptions BuildNative()
        {
            Target snat = Build();
            NatOptions options = new NatOptions();
            options.ranges = new NatRange[] { NatRange.Default() };
            options.range_size = 1;
            if (snat.TryGetValue(TO_SOURCE_OPT, out var src))
            {
                var range = src.ToIpProtoRange();
                options.ranges[0].min_ip = ReverceEndian(range.minIp);
                options.ranges[0].max_ip = ReverceEndian(range.maxIp);
                options.ranges[0].min_proto = ReverceEndian(range.minP);
                options.ranges[0].max_proto = ReverceEndian(range.maxP);
                if (options.ranges[0].min_ip > 0 || options.ranges[0].max_ip < uint.MaxValue)
                {
                    options.ranges[0].flags |= NatRange.NF_NAT_RANGE_MAP_IPS;
                }
                if (options.ranges[0].min_proto > 0 || options.ranges[0].max_proto < ushort.MaxValue)
                {
                    options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_SPECIFIED;
                }
            }

            if (snat.ContainsKey(RANDOM_OPT))
            {
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_RANDOM;
            }

            if (snat.ContainsKey(RANDOM_FULLY_OPT))
            {
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_RANDOM_FULLY;
            }

            if (snat.ContainsKey(PERSISTENT_OPT))
            {
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_RANDOM_FULLY;
            }

            return options;
        }
    }
}