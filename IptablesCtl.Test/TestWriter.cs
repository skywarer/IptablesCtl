using System;
using System.Linq;
using Xunit;
using IptablesCtl.IO;
using IptablesCtl.Models;
using IptablesCtl.Models.Builders;

namespace IptablesCtl.Test
{
    public class TestWriter
    {
        public TestWriter()
        {
            "iptables -F INPUT".Bash();
            "iptables -F OUTPUT".Bash();
            "iptables -F FORWARD".Bash();

            "iptables -t nat -F INPUT".Bash();
            "iptables -t nat -F OUTPUT".Bash();
            "iptables -t nat -F FORWARD".Bash();
            "iptables -t nat -F POSTROUTING".Bash();
        }

        [Fact]
        public void BuildAcceptRule()
        {
            var builder = new RuleBuilder();
            builder.SetIp4Src("192.168.3.2/23");
            var rule = builder.Accept();
            Assert.Equal("192.168.3.2/23", rule[RuleBuilder.SOURCE_OPT]);
        }

        [Fact]
        public void WriteAcceptRule()
        {
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.3.2/23")
                .SetIp4Dst("192.168.3")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.FILTER))
            {
                wr.AppendRule(Chains.INPUT, rule);
                var rules = wr.GetRules(Chains.INPUT);
                rule = rules.First();
                System.Console.WriteLine(rule);
                Assert.NotEmpty(rules);
                Assert.Equal("192.168.3.2/23", rule[RuleBuilder.SOURCE_OPT]);
                Assert.Equal("192.168.3.0/32", rule[RuleBuilder.DESTINATION_OPT]);
                Assert.Equal("eno8", rule[RuleBuilder.IN_INTERFACE_OPT]);
                Assert.Equal("eno45+", rule[RuleBuilder.OUT_INTERFACE_OPT.ToOptionName(true)]);
                Assert.Equal("tcp", rule[RuleBuilder.PROTOCOL_OPT]);
                Assert.Equal(TargetTypes.ACCEPT, rule.Target.Name);
            }
        }

        [Fact]
        public void WriteSNatTarget()
        {
            var snatTarget = new SNatTargetBuilder().SetSource("192.168.1.1", "192.168.1.10", 200, 300).Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.3.2/23")
                .SetIp4Dst("192.168.3/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .SetTarget(snatTarget)
                .Build();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.AppendRule(Chains.POSTROUTING, rule);
                var rules = wr.GetRules(Chains.POSTROUTING);
                rule = rules.First();
                var target = rule.Target;
                System.Console.WriteLine(rule);
                Assert.NotEmpty(rules);
                Assert.Equal("192.168.1.1-192.168.1.10:200-300", target[SNatTargetBuilder.TO_SOURCE_OPT]);
                Assert.Equal(TargetTypes.SNAT, target.Name);
            }
        }

        [Fact]
        public void WriteDNatTarget()
        {
            var dnatTarget = new DNatTargetBuilder().SetDestination("192.168.1.1", "192.168.1.10", 200, 300)
                .SetRandom().Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.3.2/23")
                .SetIp4Dst("192.168.3/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .SetTarget(dnatTarget)
                .Build();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.AppendRule(Chains.PREROUTING, rule);
                var rules = wr.GetRules(Chains.PREROUTING);
                rule = rules.First();
                var target = rule.Target;
                System.Console.WriteLine(rule);
                Assert.NotEmpty(rules);
                Assert.Equal("192.168.1.1-192.168.1.10:200-300", target[DNatTargetBuilder.TO_DESTINATION_OPT]);
                Assert.NotNull(target[DNatTargetBuilder.RANDOM_OPT]);
                Assert.Equal(TargetTypes.DNAT, target.Name);
            }
        }

        [Fact]
        public void WriteMasqueradeTarget()
        {
            var msqrdTarget = new MasqueradeTargetBuilder().SetPorts(200, 300)
                .SetRandom().Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.3.2/23")
                .SetIp4Dst("192.168.3/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .SetTarget(msqrdTarget)
                .Build();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.AppendRule(Chains.POSTROUTING, rule);
                var rules = wr.GetRules(Chains.POSTROUTING);
                rule = rules.First();
                var target = rule.Target;
                System.Console.WriteLine(rule);
                Assert.NotEmpty(rules);
                Assert.Equal("200-300", target[MasqueradeTargetBuilder.TO_PORTS_OPT]);
                Assert.NotNull(target[MasqueradeTargetBuilder.RANDOM_OPT]);
                Assert.Equal(TargetTypes.MASQUERADE, target.Name);
            }
        }

        [Fact]
        public void WriteTcpMatch()
        {
            var tcpMatch = new TcpMatchBuilder().SetSrcPort(200, 300)
                .SetFlags(new[] { "syn", "fin", "ack" }, new[] { "syn" })
                .SetOption(16, true).Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.5.2/23")
                .SetIp4Dst("192.168.5/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .AddMatch(tcpMatch)
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.AppendRule(Chains.POSTROUTING, rule);
                var rules = wr.GetRules(Chains.POSTROUTING);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("200:300", match[TcpMatchBuilder.SPORT_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }
        }


        [Fact]
        public void ReplaceTcpMatch()
        {
            var tcpMatch = new TcpMatchBuilder().SetSrcPort(200, 300)
                .SetFlags(new[] { "syn", "fin", "ack" }, new[] { "syn" })
                .SetOption(16, true).Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.5.2/23")
                .SetIp4Dst("192.168.5/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .AddMatch(tcpMatch)
                .Accept();
            var tcpMatch2 = new TcpMatchBuilder().SetSrcPort(500, 600)
                .SetFlags(new[] { "syn", "fin", "ack" }, new[] { "syn" })
                .SetOption(16, true).Build();
            var rule2 = new RuleBuilder()
                .SetIp4Src("192.168.7.2/23")
                .SetIp4Dst("192.168.3/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .AddMatch(tcpMatch2)
                .Accept();
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.AppendRule(Chains.POSTROUTING, rule);
                var rules = wr.GetRules(Chains.POSTROUTING);
                rule = rules.First();
                var match = rule.Matches.First();
                Assert.Equal("200:300", match[TcpMatchBuilder.SPORT_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.ReplaceRule(Chains.POSTROUTING, 1, rule2);
                var rules = wr.GetRules(Chains.POSTROUTING);
                rule2 = rules.First();
                var match = rule2.Matches.First();
                Assert.Equal("500:600", match[TcpMatchBuilder.SPORT_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }
        }

        [Fact]
        public void DeleteTcpMatch()
        {
            var tcpMatch = new TcpMatchBuilder().SetSrcPort(200, 300)
                .SetFlags(new[] { "syn", "fin", "ack" }, new[] { "syn" })
                .SetOption(16, true).Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.5.2/23")
                .SetIp4Dst("192.168.5/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .AddMatch(tcpMatch)
                .Accept();
            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.AppendRule(Chains.POSTROUTING, rule);
                var rules = wr.GetRules(Chains.POSTROUTING);
                rule = rules.First();
                var match = rule.Matches.First();
                Assert.Equal("200:300", match[TcpMatchBuilder.SPORT_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }

            using (var wr = new IptWrapper(Tables.NAT))
            {
                wr.DeleteRule(Chains.POSTROUTING, 1);
                var rules = wr.GetRules(Chains.POSTROUTING);
                Assert.Empty(rules);
            }
        }

        [Fact]
        public void WriteUdpMatch()
        {
            var udpMatch = new UdpMatchBuilder().SetSrcPort(200, 300)
            .SetDstPort(400, 500).Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.5.2/23")
                .SetIp4Dst("192.168.5/24")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("udp")
                .AddMatch(udpMatch)
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.FILTER))
            {
                wr.AppendRule(Chains.FORWARD, rule);
                var rules = wr.GetRules(Chains.FORWARD);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("200:300", match[UdpMatchBuilder.SPORT_OPT]);
                Assert.Equal("400:500", match[UdpMatchBuilder.DPORT_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }
        }

        [Fact]
        public void WriteIcmpMatch()
        {
            var icmpMatch = new IcmpMatchBuilder().SetIcmpType(3,11).Build();
            var rule = new RuleBuilder()
                .SetProto("icmp")
                .AddMatch(icmpMatch)
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.FILTER))
            {
                wr.AppendRule(Chains.FORWARD, rule);
                var rules = wr.GetRules(Chains.FORWARD);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("TOS-network-unreachable", match[IcmpMatchBuilder.TYPE_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteLimitMatch()
        {
            var limitMatch = new LimitMatchBuilder().SetLimit("20/m").Build();
            var rule = new RuleBuilder()
                .SetProto("icmp")
                .AddMatch(limitMatch)
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.FILTER))
            {
                wr.AppendRule(Chains.FORWARD, rule);
                var rules = wr.GetRules(Chains.FORWARD);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("20/m", match[LimitMatchBuilder.LIMIT_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteMacMatch()
        {
            var macMatch = new MacMatchBuilder().SetMacaddress("01:02:0F:A4:34:01").Build();
            var rule = new RuleBuilder()
                .SetProto("icmp")
                .AddMatch(macMatch)
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.FILTER))
            {
                wr.AppendRule(Chains.FORWARD, rule);
                var rules = wr.GetRules(Chains.FORWARD);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("01:02:0F:A4:34:01", match[MacMatchBuilder.MAC_SOURCE_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteMarkMatch()
        {
            var markMatch = new MarkMatchBuilder().SetMark(8,63).Build();
            var rule = new RuleBuilder()
                .AddMatch(markMatch)                
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper(Tables.MANGLE))
            {
                wr.AppendRule(Chains.INPUT, rule);
                var rules = wr.GetRules(Chains.INPUT);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("8/63", match[MarkMatchBuilder.MARK_OPT]);
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteMultiportMatch()
        {
            var multiportMatch = new MultiportMatchBuilder()
            .SetDstPorts("12,23,55:77,90").Build();
            var rule = new RuleBuilder()
                .SetProto("tcp")
                .AddMatch(multiportMatch)                
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper())
            {
                wr.AppendRule(Chains.INPUT, rule);
                var rules = wr.GetRules(Chains.INPUT);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("12,23,55:77,90", match[MultiportMatchBuilder.DESTINATION_PORT_OPT]);                
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteOwnerMatch()
        {
            var multiportMatch = new OwnerMatchBuilder()
            .SetUid(500,700)
            .SetGid(5,5)
            .SetSocketExists()
            .Build();
            var rule = new RuleBuilder()
                .AddMatch(multiportMatch)                
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper())
            {
                wr.AppendRule(Chains.OUTPUT, rule);
                var rules = wr.GetRules(Chains.OUTPUT);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("500-700", match[OwnerMatchBuilder.UID_OWNER_OPT]);  
                Assert.Equal("5", match[OwnerMatchBuilder.GID_OWNER_OPT]);  
                Assert.Equal(string.Empty, match[OwnerMatchBuilder.SOCKET_EXSTS_OPT]);            
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteTosMatch()
        {
            var tosMatch = new TosMatchBuilder()
            .SetTos(10,2)
            .Build();
            var rule = new RuleBuilder()
                .AddMatch(tosMatch)                
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper())
            {
                wr.AppendRule(Chains.OUTPUT, rule);
                var rules = wr.GetRules(Chains.OUTPUT);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("10/2", match[TosMatchBuilder.TOS_OPT]);         
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

        [Fact]
        public void WriteTtlMatch()
        {
            var tosMatch = new TtlMatchBuilder()
            .SetTtlGreatThan(60)
            .Build();
            var rule = new RuleBuilder()
                .AddMatch(tosMatch)                
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptWrapper())
            {
                wr.AppendRule(Chains.OUTPUT, rule);
                var rules = wr.GetRules(Chains.OUTPUT);
                rule = rules.First();
                System.Console.WriteLine(rule);
                var match = rule.Matches.First();
                Assert.Equal("60", match[TtlMatchBuilder.TTL_GT_OPT]);         
                var target = rule.Target;
                Assert.NotEmpty(rules);
                Assert.Equal(TargetTypes.ACCEPT, target.Name);
            }            
        }

    }
}