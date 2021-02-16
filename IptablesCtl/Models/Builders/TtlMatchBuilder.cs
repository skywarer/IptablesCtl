using System;
using System.Linq;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{
    public class TtlMatchBuilder : OptionsBuilder<TtlOptions, Match>
    {
        public const string TTL_EQ_OPT = "--ttl-eq";
        public const string TTL_GT_OPT = "--ttl-gt";
        public const string TTL_LT_OPT = "--ttl-lt";

        public TtlMatchBuilder() { }
        public TtlMatchBuilder(TtlOptions options)
        {
            SetOptions(options);
        }
        public TtlMatchBuilder(Match match) : base(match)
        {

        }

        public override void SetOptions(TtlOptions options)
        {
            switch (options.mode)
            {
                case TtlOptions.IPT_TTL_EQ:
                    SetTtlEqual(options.ttl);
                    break;
                case TtlOptions.IPT_TTL_NE:
                    SetTtlEqual(options.ttl, true);
                    break;
                case TtlOptions.IPT_TTL_GT:
                    SetTtlGreatThan(options.ttl);
                    break;
                case TtlOptions.IPT_TTL_LT:
                    SetTtlLessThan(options.ttl);
                    break;
            }
        }

        public TtlMatchBuilder SetTtlEqual(byte ttl, bool invert = false)
        {
            AddProperty(TTL_EQ_OPT.ToOptionName(invert), ttl);
            return this;
        }
        public TtlMatchBuilder SetTtlGreatThan(byte ttl)
        {
            AddProperty(TTL_GT_OPT.ToOptionName(), ttl);
            return this;
        }

        public TtlMatchBuilder SetTtlLessThan(byte ttl)
        {
            AddProperty(TTL_LT_OPT.ToOptionName(), ttl);
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.TTL, true, Properties);
        }
        public override TtlOptions BuildNative()
        {
            throw new NotImplementedException();
        }

    }
}