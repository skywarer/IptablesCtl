using System.Linq;
using System;
using IptablesCtl.Native;

namespace IptablesCtl.Models.Builders
{
    public sealed class LogTargetBuilder : OptionsBuilder<LogOptions, Target>
    {
        public const string LOG_LEVEL_OPT = "--log-level";
        public const string LOG_PREFIX_OPT = "--log-prefix";
        public const string LOG_TCP_SEQUENCE_OPT = "--log-tcp-sequence";
        public const string LOG_TCP_OPT = "--log-tcp-options";
        public const string LOG_IP_OPT = "--log-ip-options";
        public const string LOG_UID_OPT = "--log-uid";

        public readonly (byte code, string name)[] LOG_TYPES =
        {
            (0, "EMERGENCY"),
            (1, "ALERT"),
            (2, "CRITICAL"),
            (3, "ERROR"),
            (4, "WARNING"),
            (5, "NOTICE"),
            (6, "INFORMATIONAL"),
            (7, "DEBUG")
        };

        public LogTargetBuilder() { }
        public LogTargetBuilder(LogOptions options)
        {
            SetOptions(options);
        }
        public LogTargetBuilder(Target target) : base(target)
        {

        }

        public override void SetOptions(LogOptions options)
        {
            SetLogLevel(options.level);
            if (!string.IsNullOrEmpty(options.prefix))
            {
                SetPrefix(options.prefix);
            }
            if ((options.logflags & LogOptions.IPT_LOG_TCPSEQ) > 0)
            {
                SetLogTcpSequence();
            }
            if ((options.logflags & LogOptions.IPT_LOG_TCPOPT) > 0)
            {
                SetLogTcp();
            }
            if ((options.logflags & LogOptions.IPT_LOG_IPOPT) > 0)
            {
                SetLogIp();
            }
            if ((options.logflags & LogOptions.IPT_LOG_UID) > 0)
            {
                SetLogUid();
            }
        }

        public LogTargetBuilder SetLogLevel(byte code)
        {
            var name = LOG_TYPES.FirstOrDefault(l => l.code == code).name;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException($"level:{code}");
            AddProperty(LOG_LEVEL_OPT.ToOptionName(), name);
            return this;
        }

        public LogTargetBuilder SetPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentNullException($"prefix");
            AddProperty(LOG_PREFIX_OPT.ToOptionName(), prefix);
            return this;
        }

        public LogTargetBuilder SetLogTcpSequence()
        {
            AddProperty(LOG_TCP_SEQUENCE_OPT.ToOptionName());
            return this;
        }
        public LogTargetBuilder SetLogTcp()
        {
            AddProperty(LOG_TCP_OPT.ToOptionName());
            return this;
        }
        public LogTargetBuilder SetLogIp()
        {
            AddProperty(LOG_IP_OPT.ToOptionName());
            return this;
        }
        public LogTargetBuilder SetLogUid()
        {
            AddProperty(LOG_UID_OPT.ToOptionName());
            return this;
        }
        public override Target Build()
        {
            return new Target(TargetTypes.LOG, Properties);
        }

        public override LogOptions BuildNative()
        {
            Target target = Build();
            LogOptions opt = new LogOptions();
            if (target.TryGetOption(LOG_LEVEL_OPT, out var option))
            {
                opt.level = LOG_TYPES.FirstOrDefault(l => 
                    StringComparer.OrdinalIgnoreCase.Equals(l.name,option.Value)).code;
            }
            if (target.TryGetOption(LOG_PREFIX_OPT, out var prefix))
            {
                opt.prefix = prefix.Value;
            }
            if (target.ContainsOption(LOG_TCP_SEQUENCE_OPT))
            {
                opt.logflags |= LogOptions.IPT_LOG_TCPSEQ;
            }
            if (target.ContainsOption(LOG_TCP_OPT))
            {
                opt.logflags |= LogOptions.IPT_LOG_TCPOPT;
            }
            if (target.ContainsOption(LOG_IP_OPT))
            {
                opt.logflags |= LogOptions.IPT_LOG_IPOPT;
            }
            if (target.ContainsOption(LOG_UID_OPT))
            {
                opt.logflags |= LogOptions.IPT_LOG_UID;
            }
            return opt;
        }
    }
}