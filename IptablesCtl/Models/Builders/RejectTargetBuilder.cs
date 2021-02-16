using System;
using System.Linq;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class RejectTargetBuilder : OptionsBuilder<RejectOptions, Target>
    {
        public const string REJECT_WITH_OPT = "--reject-with";

        public static readonly (string key, RejectWith value)[] reject_with = {
            ("icmp-net-unreachable", RejectWith.IPT_ICMP_NET_UNREACHABLE),
            ("icmp-host-unreachable", RejectWith.IPT_ICMP_HOST_UNREACHABLE),
            ("icmp-port-unreachable", RejectWith.IPT_ICMP_PORT_UNREACHABLE),
            ("icmp-proto-unreachable", RejectWith.IPT_ICMP_PROT_UNREACHABLE),
            ("icmp-net-prohibited", RejectWith.IPT_ICMP_NET_PROHIBITED),
            ("icmp-admin-prohibited", RejectWith.IPT_ICMP_ADMIN_PROHIBITED),
            ("tcp-reset", RejectWith.IPT_TCP_RESET)
        };

        public RejectTargetBuilder() { }

        public RejectTargetBuilder(RejectOptions options)
        {
            SetOptions(options);
        }
        public RejectTargetBuilder(Target target) : base(target)
        {

        }

        public override void SetOptions(RejectOptions options)
        {
            SetRejectWith(options.with);
        }

        public RejectTargetBuilder SetRejectWith(RejectWith rw)
        {
            var withKey = reject_with.FirstOrDefault(w => w.value == rw).key;
            AddProperty(REJECT_WITH_OPT.ToOptionName(), withKey);
            return this;
        }

        public override Target Build()
        {
            return new Target(TargetTypes.REJECT, Properties);
        }

        public override RejectOptions BuildNative()
        {
            throw new NotImplementedException();
        }
    }
}