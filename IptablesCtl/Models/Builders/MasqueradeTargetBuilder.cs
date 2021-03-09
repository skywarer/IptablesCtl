using System;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class MasqueradeTargetBuilder : OptionsBuilder<NatOptions, Target>
    {
        public const string TO_PORTS_OPT = "--to-ports";
        public const string RANDOM_OPT = "--random";

        public MasqueradeTargetBuilder()
        {
        }

        public MasqueradeTargetBuilder(NatOptions options)
        {
            SetOptions(options);
        }

        public MasqueradeTargetBuilder(Target target) : base(target)
        {
        }

        public override void SetOptions(NatOptions options)
        {
            if (options.range_size > 0)
            {
                var range = options.ranges[0];
                if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_SPECIFIED) > 0)
                {
                    //to-destination 
                    var minP = ReverceEndian(range.min_proto);
                    var maxP = ReverceEndian(range.max_proto);
                    SetPorts(minP, maxP);
                }

                //random
                if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_RANDOM) > 0)
                {
                    SetRandom();
                }
            }
        }

        public MasqueradeTargetBuilder SetPorts(ushort min_proto, ushort max_proto)
        {
            AddRangeProperty(TO_PORTS_OPT.ToOptionName(), min_proto, max_proto, '-');
            return this;
        }

        public MasqueradeTargetBuilder SetRandom()
        {
            AddProperty(RANDOM_OPT.ToOptionName());
            return this;
        }

        public override Target Build()
        {
            return new Target(TargetTypes.MASQUERADE, Properties);
        }

        public override NatOptions BuildNative()
        {
            Target msqrd = Build();
            NatOptions options = new NatOptions();
            options.ranges = new NatRange[] {NatRange.Default()};
            options.range_size = 1;
            if (msqrd.TryGetValue(TO_PORTS_OPT, out var src))
            {
                var range = src.ToRangeProperty('-');
                options.ranges[0].min_proto = ReverceEndian(ushort.Parse(range.Left));
                options.ranges[0].max_proto = ReverceEndian(ushort.Parse(range.Rigt));
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_SPECIFIED;
            }

            if (msqrd.ContainsKey(RANDOM_OPT))
            {
                options.ranges[0].flags |= NatRange.NF_NAT_RANGE_PROTO_RANDOM;
            }

            return options;
        }
    }
}