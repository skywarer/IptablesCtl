using System;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class RedirectTargetBuilder : OptionsBuilder<NatOptions, Target>
    {

        public const string TO_PORTS_OPT = "--to-ports";
        public const string RANDOM_OPT = "--random";

        public RedirectTargetBuilder() { }
        public RedirectTargetBuilder(NatOptions options)
        {
            SetOptions(options);
        }
        public RedirectTargetBuilder(Target target) : base(target)
        {

        }

        public override void SetOptions(NatOptions options)
        {
            if (options.range_size > 0)
            {
                var range = options.ranges[0];
                if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_SPECIFIED) > 0)
                {
                    //redirect port 
                    var minP = ReverceEndian(range.min_proto);
                    var maxP = ReverceEndian(range.max_proto);
                    SetRedirectWithProto(minP, maxP);
                }
                //random
                if ((range.flags & NatRange.NF_NAT_RANGE_PROTO_RANDOM) > 0)
                {
                    SetRandom();
                }
            }
        }

        public RedirectTargetBuilder SetRedirectWithProto(ushort min_proto, ushort max_proto)
        {
            AddRangeProperty(TO_PORTS_OPT.ToOptionName(), min_proto, max_proto, '-');
            return this;
        }

        public RedirectTargetBuilder SetRandom()
        {
            AddProperty(RANDOM_OPT.ToOptionName());
            return this;
        }

        public override Target Build()
        {
            return new Target(TargetTypes.REDIRECT, Properties);
        }

        public override NatOptions BuildNative()
        {
            throw new NotImplementedException();
        }
    }
}