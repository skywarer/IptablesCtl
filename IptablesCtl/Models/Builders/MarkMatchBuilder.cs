using System;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{
    public class MarkMatchBuilder : OptionsBuilder<MarkOptions, Match>
    {
        public const string MARK_OPT = "--mark";

        public MarkMatchBuilder()
        {

        }

        public MarkMatchBuilder(MarkOptions options)
        {
            SetOptions(options);
        }

        public MarkMatchBuilder(Match match) : base(match)
        {

        }

        public override void SetOptions(MarkOptions options)
        {
            SetMark(options.mark,options.mask < uint.MaxValue ? options.mask : 0,(options.invert & MarkOptions.XT_MARK_INV) > 0);
        }

        public MarkMatchBuilder SetMark(uint mark, uint mask = 0, bool invert = false)
        {
            AddMaskedProperty(MARK_OPT.ToOptionName(invert), mark, mask, '/');
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.MARK, true, Properties);
        }

        public override MarkOptions BuildNative()
        {
            throw new NotImplementedException();
        }

    }
}