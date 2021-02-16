using System;
using System.Linq;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{

    public sealed class TosMatchBuilder : OptionsBuilder<TosOptions, Match>
    {
        public const string TOS_OPT = "--tos";
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
            SetTos(options.value, options.mask < byte.MaxValue ? options.mask : 0, (options.invert & TosOptions.XT_TOS_INV) > 0);
        }

        public TosMatchBuilder SetTos(byte value, byte mask, bool invert)
        {
            AddMaskedProperty(TOS_OPT.ToOptionName(invert), value, mask, '/');
            return this;
        }

        public override Match Build()
        {
            return new Match(MatchTypes.TOS, true, Properties);
        }

        public override TosOptions BuildNative()
        {
            throw new NotImplementedException();
        }
    }
}