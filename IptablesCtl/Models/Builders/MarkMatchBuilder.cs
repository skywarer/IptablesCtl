using System;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{
    public class MarkMatchBuilder : OptionsBuilder<MarkOptions, Match>
    {
        public const string MARK_OPT = "--mark";
        public const byte Revision = 1;

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
            SetMark(options.mark, options.mask, (options.invert & MarkOptions.XT_MARK_INV) > 0);
        }

        public MarkMatchBuilder SetMark(uint mark, uint mask = 63, bool invert = false)
        {
            // 63 is default mask
            if (mask < 63)
            {
                AddMaskedProperty(MARK_OPT.ToOptionName(invert), mark, mask, '/');
            }
            else
            {
                AddProperty(MARK_OPT.ToOptionName(invert), mark);
            }
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.MARK, true, Properties, Revision);
        }

        public override MarkOptions BuildNative()
        {
            var match = Build();
            MarkOptions opt = MarkOptions.Default();
            if (match.TryGetOption(MARK_OPT, out var options))
            {
                var masked = options.Value.ToMaskedProperty('/');
                opt.mark = uint.Parse(masked.Value);
                if (!string.IsNullOrEmpty(masked.Mask))
                {
                    opt.mask = uint.Parse(masked.Mask);
                }
                if (options.Inverted) opt.invert |= MarkOptions.XT_MARK_INV;
            }
            return opt;
        }

    }
}