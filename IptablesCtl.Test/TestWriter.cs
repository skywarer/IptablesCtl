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
            var tcpMatch = new TcpMatchBuilder().SetSrcPort(200,300)
                .SetFlags(new []{"syn","fin","ack"},new []{"syn"})
                .SetOption(16,true).Build();
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
                .SetFlags(new[] {"syn", "fin", "ack"}, new[] {"syn"})
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
                .SetFlags(new[] {"syn", "fin", "ack"}, new[] {"syn"})
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
            var tcpMatch = new TcpMatchBuilder().SetSrcPort(200,300)
                .SetFlags(new []{"syn","fin","ack"},new []{"syn"})
                .SetOption(16,true).Build();
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
    }
}