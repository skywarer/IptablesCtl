using System;
using System.Linq;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{

    public sealed class TosMatchBuilder : OptionsBuilder<TosOptions, Match>
    {
        public const string TOS_OPT = "--tos";
        public const byte Revision = 1;
        public static readonly (byte code, string name)[] TOS_NAMES = {
            (0x10, "minimize-delay"),
            (0x08, "maximize-throughput"),
            (0x04, "maximize-reliability"),
            (0x02, "minimize-cost"),
            (0x00, "normal-service")
        };

        public TosMatchBuilder() { }
        public TosMatchBuilder(TosOptions options)
        {
            SetOptions(options);
        }
        public TosMatchBuilder(Match match) : base(match)
        {

        }

        public override void SetOptions(TosOptions options)
        {
            SetTos(options.value, options.mask, (options.invert & TosOptions.XT_TOS_INV) > 0);
        }

        public TosMatchBuilder SetTos(byte value, byte mask = byte.MaxValue, bool invert = false)
        {
            if (mask < byte.MaxValue)
            {
                AddMaskedProperty(TOS_OPT.ToOptionName(invert), value, mask, '/');
            }
            else
            {
                AddProperty(TOS_OPT.ToOptionName(invert), value);
            }
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.TOS, true, Properties, Revision);
        }

        public override TosOptions BuildNative()
        {
            var match = Build();
            TosOptions opt = TosOptions.Default();
            if (match.TryGetOption(TOS_OPT, out var options))
            {
                var masked = options.Value.ToMaskedProperty('/');
                opt.value = byte.Parse(masked.Value);
                if (!string.IsNullOrEmpty(masked.Mask))
                {
                    opt.mask = byte.Parse(masked.Mask);
                }
                if (options.Inverted) opt.invert |= TosOptions.XT_TOS_INV;
            }
            return opt;
        }
    }
}