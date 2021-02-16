using System;
using IptablesCtl.Native;
using System.Text.RegularExpressions;
namespace IptablesCtl.Models.Builders
{
    public sealed class LimitMatchBuilder : OptionsBuilder<RateInfoOptions, Match>
    {
        public const string LIMIT_OPT = "--limit";
        public const string LIMIT_BURST_OPT = "--limit-burst";
        const uint DAY_RANGE = 24 * 60 * 60 * RateInfoOptions.XT_LIMIT_SCALE;
        const uint HOUR_RANGE = 60 * 60 * RateInfoOptions.XT_LIMIT_SCALE;
        const uint MINUTE_RANGE = 60 * RateInfoOptions.XT_LIMIT_SCALE;
        const uint SECOND_RANGE = RateInfoOptions.XT_LIMIT_SCALE;
        public static string ToHumanRate(uint rate) => rate switch
        {
            var r when r <= SECOND_RANGE => $"{SECOND_RANGE / r}/s",
            var r when r > SECOND_RANGE && r <= MINUTE_RANGE => $"{MINUTE_RANGE / r}/m",
            var r when r > MINUTE_RANGE && r <= HOUR_RANGE => $"{HOUR_RANGE / r}/h",
            var r when r > HOUR_RANGE && r <= DAY_RANGE => $"{DAY_RANGE / r}/d",
            _ => $"{rate}"
        };

        static Regex limitRegex = new Regex(@"(?<count>[1-9]\d*)\/(?<range>(?:second|minute|hour|day|s|m|h|d))");

        public LimitMatchBuilder()
        {

        }

        public LimitMatchBuilder(RateInfoOptions options)
        {
            SetOptions(options);
        }

        public LimitMatchBuilder(Match match) : base(match)
        {

        }

        public LimitMatchBuilder SetLimit(uint avg)
        {
            SetLimit(ToHumanRate(avg));
            return this;
        }

        public LimitMatchBuilder SetLimit(string limit)
        {
            if (!limitRegex.IsMatch(limit)) throw new FormatException($"limit:{limit}");
            AddProperty(LIMIT_OPT.ToOptionName(), limit);
            return this;
        }

        public LimitMatchBuilder SetLimitBurst(uint burst)
        {
            AddProperty(LIMIT_BURST_OPT.ToOptionName(), burst);
            return this;
        }

        public override void SetOptions(RateInfoOptions options)
        {
            SetLimit(options.avg);
            SetLimitBurst(options.burst);
        }

        public override Match Build()
        {
            return new Match(MatchTypes.LIMIT, true, Properties);
        }

        public override RateInfoOptions BuildNative()
        {
            var match = Build();
            RateInfoOptions opt = new RateInfoOptions();

            if (match.TryGetOption(LIMIT_OPT, out var options))
            {
                var limitMatch = limitRegex.Match(options.Value);
                if (limitMatch.Success)
                {
                    var count = uint.Parse(limitMatch.Groups["count"].Value);
                    var range = limitMatch.Groups["range"].Value;
                    var numerator = range.ToLower() switch
                    {
                        "s" => SECOND_RANGE,
                        "m" => MINUTE_RANGE,
                        "h" => HOUR_RANGE,
                        "d" => DAY_RANGE,
                        _ => throw new FormatException($"rate limit {options.Value}"),
                    };
                    opt.avg = numerator / count;
                }
                else
                {
                    throw new FormatException($"rate limit {options.Value}");
                }
            }
            else
            {
                opt.avg = RateInfoOptions.LIMIT_DEF;
            }
            if (match.TryGetOption(LIMIT_BURST_OPT, out options))
            {
                opt.burst = uint.Parse(options.Value);
            }
            else
            {
                opt.burst = RateInfoOptions.LIMIT_BURST_DEF;
            }
            return opt;
        }
    }
}