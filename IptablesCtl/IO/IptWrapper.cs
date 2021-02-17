#define DEBUG
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using IptablesCtl.Native;
using IptablesCtl.Models;
using IptablesCtl.Models.Builders;
namespace IptablesCtl.IO
{
    public class IptWrapper : IDisposable
    {
        /// <summary>
        /// Current Table
        /// </summary>
        public string Table { get; }
        IntPtr _handle;
        public IptWrapper(string tableName = Tables.FILTER)
        {
            Table = tableName;
            _handle = Libiptc4.iptc_init(Table);
        }

        /// <summary>
        /// Get chains for current Table
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<string> GetChains()
        {
            List<string> chains = new List<string>(10);
            for (var chainPtr = Libiptc4.iptc_first_chain(_handle);
                chainPtr != IntPtr.Zero;
                chainPtr = Libiptc4.iptc_next_chain(_handle))
            {
                var chain = (string)Marshal.PtrToStringAnsi(chainPtr);
                chains.Add(chain);
            }
            return new ReadOnlyCollection<string>(chains);
        }

        /// <summary>
        /// Get rules for chain name
        /// </summary>
        /// <param name="chain">chain name</param>
        /// <returns></returns>
        public IReadOnlyCollection<Rule> GetRules(string chain)
        {
            List<Rule> rules = new List<Rule>(10);
            for (var rulePtr = Libiptc4.iptc_first_rule(chain, _handle);
                    rulePtr != IntPtr.Zero;
                    rulePtr = Libiptc4.iptc_next_rule(rulePtr, _handle))
            {
                var iptEntry = Marshal.PtrToStructure<IptEntry>(rulePtr);
                int offset = Sizes.Align(Sizes.IptEntryLen);
                List<Match> matches = new List<Match>();
                // read matches
                while (iptEntry.target_offset > offset)
                {
                    offset += ReadMatch(rulePtr + offset, out Match match);
                    matches.Add(match);
                }
                // read targets
                offset = iptEntry.target_offset;
                Target target = null;
                if (iptEntry.next_offset > offset)
                {
                    offset += ReadTarget(rulePtr + offset, out target);
                }
                var rule = new RuleBuilder(iptEntry, matches, target).Build();
                rules.Add(rule);
            }
            return new ReadOnlyCollection<Rule>(rules);
        }

        /// <summary>
        /// Type of match option by name selector
        /// </summary>
        /// <param name="name">name of match</param>
        /// <returns></returns>
        Type GetMatchOptionsTypeBase(string name) => name.ToLower() switch
        {
            MatchTypes.TCP => typeof(TcpOptions),
            MatchTypes.UDP => typeof(UdpOptions),
            MatchTypes.ICMP => typeof(IcmpOptions),
            MatchTypes.LIMIT => typeof(RateInfoOptions),
            MatchTypes.MAC => typeof(MacOptions),
            MatchTypes.MARK => typeof(MarkOptions),
            MatchTypes.MULTIPORT => typeof(MultiportOptions),
            MatchTypes.OWNER => typeof(OwnerOptions),
            MatchTypes.TOS => typeof(TosOptions),
            MatchTypes.TTL => typeof(TtlOptions),
            _ => GetMatchOptionsType(name)
        };

        /// <summary>
        /// Type of match option by name selector
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual Type GetMatchOptionsType(string name)
        {
            return null;
        }

        /// <summary>
        /// Match options deserialize 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, string> GetMatchOptions(Header header, object options)
        {
            return null;
        }

        /// <summary>
        /// Deserialize match options for not base types
        /// </summary>
        /// <param name="header"></param>
        /// <param name="optPoint"></param>
        /// <returns></returns>
        /// <exception cref="IptException"></exception>
        Match GetUnknowMatch(Header header, IntPtr optPoint)
        {
            var optType = GetMatchOptionsType(header.name);
            if (optType != null)
            {
                if (!optType.IsValueType)
                {
                    throw new IptException($"options type {optType.Name} must be struct)");
                }
                var optSize = header.size - Options.HeaderLen;
                if (optSize > Marshal.SizeOf(optType))
                {
                    throw new IptException($"size of {optType.Name} greater then option size ({optSize}b)");
                }
                var opt = Marshal.PtrToStructure(optPoint, optType);
                var prop = GetMatchOptions(header, opt);
                return new Match(header.name, true, prop);
            }
            else
            {
                return new Match(header.name);
            }
        }

        /// <summary>
        /// Deserialize match from unmanaged
        /// </summary>
        /// <param name="point"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        private int ReadMatch(IntPtr point, out Match match)
        {
            var header = Marshal.PtrToStructure<Header>(point);
            int offset = header.size;
            var optPoint = point + Sizes.Align(Sizes.HeaderLen);
            match = header.name.ToLower() switch
            {
                MatchTypes.TCP => new TcpMatchBuilder(Marshal.PtrToStructure<TcpOptions>(optPoint)).Build(),
                MatchTypes.UDP => new UdpMatchBuilder(Marshal.PtrToStructure<UdpOptions>(optPoint)).Build(),
                MatchTypes.ICMP => new IcmpMatchBuilder(Marshal.PtrToStructure<IcmpOptions>(optPoint)).Build(),
                MatchTypes.LIMIT => new LimitMatchBuilder(Marshal.PtrToStructure<RateInfoOptions>(optPoint)).Build(),
                MatchTypes.MAC => new MacMatchBuilder(Marshal.PtrToStructure<MacOptions>(optPoint)).Build(),
                MatchTypes.MARK => new MarkMatchBuilder(Marshal.PtrToStructure<MarkOptions>(optPoint)).Build(),
                MatchTypes.MULTIPORT => new MultiportMatchBuilder(Marshal.PtrToStructure<MultiportOptions>(optPoint)).Build(),
                MatchTypes.OWNER => new OwnerMatchBuilder(Marshal.PtrToStructure<OwnerOptions>(optPoint)).Build(),
                MatchTypes.TOS => new TosMatchBuilder(Marshal.PtrToStructure<TosOptions>(optPoint)).Build(),
                MatchTypes.TTL => new TtlMatchBuilder(Marshal.PtrToStructure<TtlOptions>(optPoint)).Build(),
                _ => GetUnknowMatch(header, optPoint)
            };
            return offset;
        }

        /// <summary>
        /// Cast verdict to standart target
        /// </summary>
        /// <param name="verdict"></param>
        /// <returns></returns>
        private Target StandartTarget(int verdict) => verdict switch
        {
            StandardTarget.NF_ACCEPT => new Target(TargetTypes.ACCEPT),
            StandardTarget.NF_DROP => new Target(TargetTypes.DROP),
            StandardTarget.NF_QUEUE => new Target(TargetTypes.QUEUE),
            StandardTarget.NF_REPEAT => new Target(TargetTypes.RETURN),//x_tables.h::XT_RETURN
            _ => new Target("unknown verdict")
        };

        /*from x_tables.c::verdict_ok*/
        private int VerdictTrsfm(int verdict) => verdict switch
        {
            var vrd when vrd > 0 => vrd,
            var vrd when vrd < 0 => (-vrd - 1),
            _ => 0xFF
        };

        /// <summary>
        /// Type of target option by name selector
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Type GetTargetOptionsTypeBase(string name) => name.ToUpper() switch
        {
            TargetTypes.DNAT => typeof(NatOptions),
            TargetTypes.SNAT => typeof(NatOptions),
            TargetTypes.LOG => typeof(LogOptions),
            TargetTypes.MASQUERADE => typeof(NatOptions),
            TargetTypes.REDIRECT => typeof(NatOptions),
            TargetTypes.REJECT => typeof(RejectOptions),
            TargetTypes.ACCEPT => typeof(int),
            TargetTypes.DROP => typeof(int),
            TargetTypes.QUEUE => typeof(int),
            TargetTypes.RETURN => typeof(int),
            _ => GetTargetOptionsType(name)
        };

        /// <summary>
        /// Type of target option by name selector
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual Type GetTargetOptionsType(string name)
        {
            return null;
        }

        /// <summary>
        /// Target options deserialize
        /// </summary>
        /// <param name="header"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected virtual IDictionary<string, string> GetTargetOptions(Header header, object options)
        {
            return null;
        }

        /// <summary>
        /// Deserialize match options for not base types
        /// </summary>
        /// <param name="header"></param>
        /// <param name="optPoint"></param>
        /// <returns></returns>
        /// <exception cref="IptException"></exception>
        Target GetUnknowTarget(Header header, IntPtr optPoint)
        {
            var optType = GetTargetOptionsType(header.name);
            if (optType != null)
            {
                if (!optType.IsValueType)
                {
                    throw new IptException($"options type {optType.Name} must be struct)");
                }
                var optSize = header.size - Options.HeaderLen;
                if (optSize > Marshal.SizeOf(optType))
                {
                    throw new IptException($"size of {optType.Name} greater then option size ({optSize}b)");
                }
                var opt = Marshal.PtrToStructure(optPoint, optType);
                var prop = GetTargetOptions(header, opt);
                return new Target(header.name, prop);
            }
            else
            {
                return new Target(header.name);
            }
        }

        /// <summary>
        /// Deserialize target from unmanaged
        /// </summary>
        /// <param name="point"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private int ReadTarget(IntPtr point, out Target target)
        {
            var header = Marshal.PtrToStructure<Header>(point);
            int offset = header.size;
            var optPoint = point + Sizes.Align(Sizes.HeaderLen);
            target = header.name.ToUpper() switch
            {
                TargetTypes.DNAT => new DNatTargetBuilder(Marshal.PtrToStructure<NatOptions>(optPoint)).Build(),
                TargetTypes.SNAT => new SNatTargetBuilder(Marshal.PtrToStructure<NatOptions>(optPoint)).Build(),
                TargetTypes.LOG => new LogTargetBuilder(Marshal.PtrToStructure<LogOptions>(optPoint)).Build(),
                TargetTypes.MASQUERADE => new MasqueradeTargetBuilder(Marshal.PtrToStructure<NatOptions>(optPoint)).Build(),
                TargetTypes.REDIRECT => new RedirectTargetBuilder(Marshal.PtrToStructure<NatOptions>(optPoint)).Build(),
                TargetTypes.REJECT => new RejectTargetBuilder(Marshal.PtrToStructure<RejectOptions>(optPoint)).Build(),
                var name when string.IsNullOrEmpty(name) => StandartTarget(VerdictTrsfm(Marshal.PtrToStructure<int>(optPoint))),
                _ => GetUnknowTarget(header, optPoint)
            };
            return offset;
        }

        /// <summary>
        /// Create unmanaged struct for rule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns>If success point retured: need Marshal.FreeHGlobal(point)</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        IntPtr RuleToIntPtr(Rule rule)
        {
            if (rule.Target == null || string.IsNullOrEmpty(rule.Target.Name)) throw new ArgumentNullException(nameof(rule.Target));
            var entry = new RuleBuilder(rule).BuildNative();
            // get offsets
            entry.target_offset = (ushort)Sizes.Align(Marshal.SizeOf<IptEntry>()
                                                      + rule.Matches.Sum(m => Sizes.HeaderLen + Marshal.SizeOf(GetMatchOptionsTypeBase(m.Name))));
            entry.next_offset = (ushort)(entry.target_offset + Sizes.HeaderLen
                                                             + Sizes.Align(Marshal.SizeOf(GetTargetOptionsTypeBase(rule.Target.Name))));
            // all size
            int size = entry.next_offset;
            IntPtr point = Marshal.AllocHGlobal(size);
            try
            {
                // write entry
                Marshal.StructureToPtr<IptEntry>(entry, point, false);
                int offset = Sizes.Align(Sizes.IptEntryLen);
                // write matches
                foreach (var m in rule.Matches)
                {
                    offset += WriteMatch(point + offset, m);
                }
                // write target
                offset += WriteTarget(point + offset, rule.Target);
                if (offset != size)
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }
            catch (Exception)
            {
                Marshal.FreeHGlobal(point);
                throw;
            }
            return point;
        }

        void CommitResultOrThrowException(int result)
        {
            bool success = result == 1;
            if (!success)
            {
                int errno = Marshal.GetLastWin32Error();
                string errStr = Marshal.PtrToStringAnsi(Libiptc4.iptc_strerror(errno));
                throw new IptException($"{errStr} (errcode: {errno})");
            }
            success &= Libiptc4.iptc_commit(_handle) == 1;
            if (!success)
            {
                int errno = Marshal.GetLastWin32Error();// it`s works
                string errStr = Marshal.PtrToStringAnsi(Libiptc4.iptc_strerror(errno));
                throw new IptException($"{errStr} (errcode: {errno})");
            }
        }

        /// <summary>
        /// Append rule to chain
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="rule"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AppendRule(string chain, Rule rule)
        {
            IntPtr point = RuleToIntPtr(rule); ;
            try
            {
                int result = Libiptc4.iptc_append_entry(chain, point, _handle);
                CommitResultOrThrowException(result);
            }
            finally
            {
                Marshal.FreeHGlobal(point);
            }
        }

        /// <summary>
        /// Serialize match options to struct
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public virtual object SetMatchOptions(Match match)
        {
            return null;
        }

        /// <summary>
        /// Write match to memory
        /// </summary>
        /// <param name="point"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        /// <exception cref="IptException"></exception>
        int WriteMatch(IntPtr point, Match match)
        {
            //header
            Header header = new Header();
            header.name = match.Name;
            header.revision = match.Revision;
            var optType = GetMatchOptionsTypeBase(match.Name);
            var optSize = Marshal.SizeOf(optType);
            var allSize = Sizes.Align(Options.HeaderLen + optSize);
            if (allSize > ushort.MaxValue)
            {
                throw new IptException($"size of {optType.Name} greater then max size ({ushort.MaxValue}b)");
            }
            header.size = (ushort)allSize;
            Marshal.StructureToPtr(header, point, true);
            //options
            var optPoint = point + Sizes.Align(Sizes.HeaderLen);
            switch (match.Name.ToLower())
            {
                case MatchTypes.TCP:
                    Marshal.StructureToPtr(new TcpMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.UDP:
                    Marshal.StructureToPtr(new UdpMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.ICMP:
                    Marshal.StructureToPtr(new IcmpMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.LIMIT:
                    Marshal.StructureToPtr(new LimitMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.MAC:
                    Marshal.StructureToPtr(new MacMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.MARK:
                    Marshal.StructureToPtr(new MarkMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.MULTIPORT:
                    Marshal.StructureToPtr(new MultiportMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.OWNER:
                    Marshal.StructureToPtr(new OwnerMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.TOS:
                    Marshal.StructureToPtr(new TosMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                case MatchTypes.TTL:
                    Marshal.StructureToPtr(new TtlMatchBuilder(match).BuildNative(), optPoint, true);
                    break;
                default:
                    object opt = SetMatchOptions(match);
                    if (opt != null)
                    {
                        if (Marshal.SizeOf(opt) != optSize)
                        {
                            throw new IptException($"unexpected size of {nameof(opt)}");
                        }
                        Marshal.StructureToPtr(opt, optPoint, true);
                    }
                    break;
            };
            return header.size;
        }

        /// <summary>
        /// Serialize target options to struct 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual object SetTargetOptions(Target target)
        {
            return null;
        }

        /// <summary>
        /// Write target options to memory
        /// </summary>
        /// <param name="point"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="IptException"></exception>
        int WriteTarget(IntPtr point, Target target)
        {
            //header
            Header header = new Header();
            header.name = target.Name;
            var optType = GetTargetOptionsTypeBase(target.Name);
            var optSize = Marshal.SizeOf(optType);
            var allSize = Sizes.Align(Options.HeaderLen + optSize);
            if (allSize > ushort.MaxValue)
            {
                throw new IptException($"size of {optType.Name} greater then max size ({ushort.MaxValue}b)");
            }
            header.size = (ushort)allSize;
            Marshal.StructureToPtr(header, point, true);
            //options
            var optPoint = point + Sizes.Align(Sizes.HeaderLen);
            object options = null;// ACCEPT and DROP not need options 
            switch (target.Name)
            {
                case TargetTypes.SNAT:
                    options = new SNatTargetBuilder(target).BuildNative();
                    break;
                case TargetTypes.DNAT:
                    options = new DNatTargetBuilder(target).BuildNative();
                    break;
                case TargetTypes.MASQUERADE:
                    options = new MasqueradeTargetBuilder(target).BuildNative();
                    break;
                case TargetTypes.LOG:
                    options = new LogTargetBuilder(target).BuildNative();
                    break;
                case TargetTypes.REDIRECT:
                    options = new RedirectTargetBuilder(target).BuildNative();
                    break;
                case TargetTypes.REJECT:
                    options = new RejectTargetBuilder(target).BuildNative();
                    break;
                default:
                    options = SetTargetOptions(target);
                    break;                    
            };
            if (options != null)
            {
                if (Marshal.SizeOf(options) != optSize)
                {
                    throw new IptException($"unexpected size of {nameof(options)}");
                }
                Marshal.StructureToPtr(options, optPoint, true);
            }
            return header.size;
        }

        /// <summary>
        /// Replace rule with new
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="number">start with 1</param>
        /// <param name="rule"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ReplaceRule(string chain, uint number, Rule rule)
        {
            IntPtr point = RuleToIntPtr(rule);
            try
            {
                int result = Libiptc4.iptc_replace_entry(chain, point, number - 1, _handle);
                CommitResultOrThrowException(result);
            }
            finally
            {
                Marshal.FreeHGlobal(point);
            }
        }

        /// <summary>
        /// Delete from chain 
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="number">start with 1</param>
        /// <exception cref="IptException"></exception>
        public void DeleteRule(string chain, uint number)
        {
            // documentation https://www.opennet.ru/docs/HOWTO/Querying-libiptc-HOWTO/ 
            // wrong with num start 1 !!!!
            int result = Libiptc4.iptc_delete_num_entry(chain, number - 1, _handle);
            CommitResultOrThrowException(result);
        }

        public void Dispose()
        {
            Libiptc4.iptc_free(_handle);
        }
    }

}