using System.Linq;
using Xunit;
using IptablesCtl.Models;
using IptablesCtl.Models.Builders;

namespace IptablesCtl.Test
{
    public class TestReader
    {
        public TestReader()
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
        public void GetBytesTest()
        {
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.FORWARD);
                Assert.NotEmpty(rules);
                Assert.Equal("tcp", rules.First()[RuleBuilder.PROTOCOL_OPT]);
                System.Console.WriteLine(rules.First());
                foreach(var r in rules)
                {
                    System.Console.WriteLine(r.Bytes);
                }
            }
        }

        [Fact]
        public void ProtocolMatchTest()
        {
            // tcp
            "iptables -A INPUT -p tcp -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("tcp", rules.First()[RuleBuilder.PROTOCOL_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT -p tcp -j ACCEPT".Bash();
            "iptables -A INPUT ! -p tcp -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("tcp", rules.First()[RuleBuilder.PROTOCOL_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT ! -p tcp -j ACCEPT".Bash();
            // udp
            "iptables -A INPUT -p udp -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("udp", rules.First()[RuleBuilder.PROTOCOL_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT -p udp -j ACCEPT".Bash();
        }

        [Fact]
        public void SourceMatchTest()
        {
            // tcp
            "iptables -A INPUT -s 192.168.1.1 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Contains("192.168.1.1", rules.First()[RuleBuilder.SOURCE_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT -s 192.168.1.1 -j ACCEPT".Bash();
            "iptables -A INPUT ! -s 192.168.1.1/24 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Contains("192.168.1", rules.First()[RuleBuilder.SOURCE_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT ! -s 192.168.1.1/24 -j ACCEPT".Bash();
        }

        [Fact]
        public void DestinationMatchTest()
        {
            // tcp
            "iptables -A INPUT -d 192.168.1.1 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Contains("192.168.1.1", rules.First()[RuleBuilder.DESTINATION_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT -d 192.168.1.1 -j ACCEPT".Bash();
            "iptables -A INPUT ! -d 192.168.1.1/24 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Contains("192.168.1", rules.First()[RuleBuilder.DESTINATION_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT ! -d 192.168.1.1/24 -j ACCEPT".Bash();
        }

        [Fact]
        public void InIfaceMatchTest()
        {
            // tcp
            "iptables -A INPUT -i eno10 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("eno10", rules.First()[RuleBuilder.IN_INTERFACE_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT -i eno10 -j ACCEPT".Bash();
            "iptables -A INPUT -i enf+ -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("enf+", rules.First()[RuleBuilder.IN_INTERFACE_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT -i enf+ -j ACCEPT".Bash();
            "iptables -A INPUT ! -i eno10 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                Assert.Contains("eno10", rules.First()[RuleBuilder.IN_INTERFACE_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D INPUT ! -i eno10 -j ACCEPT".Bash();
        }

        [Fact]
        public void OutIfaceMatchTest()
        {
            // tcp
            "iptables -A OUTPUT -o eno10 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("eno10", rules.First()[RuleBuilder.OUT_INTERFACE_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D OUTPUT -o eno10 -j ACCEPT".Bash();
            "iptables -A OUTPUT -o enf+ -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                Assert.Equal("enf+", rules.First()[RuleBuilder.OUT_INTERFACE_OPT]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D OUTPUT -o enf+ -j ACCEPT".Bash();
            "iptables -A OUTPUT ! -o eno10 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                Assert.Contains("eno10", rules.First()[RuleBuilder.OUT_INTERFACE_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rules.First());
            }
            "iptables -D OUTPUT ! -o eno10 -j ACCEPT".Bash();
        }

        [Fact]
        public void FragmentMatchTest()
        {
            "iptables -A OUTPUT -f -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                Assert.True(string.IsNullOrEmpty(rules.First()[RuleBuilder.FRAGMENT_OPT]));
                System.Console.WriteLine(rules.First());
            }
            "iptables -D OUTPUT -f -j ACCEPT".Bash();
            "iptables -A OUTPUT ! -f -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                Assert.True(string.IsNullOrEmpty(rules.First()[RuleBuilder.FRAGMENT_OPT.ToOptionName(true)]));
                System.Console.WriteLine(rules.First());
            }
            "iptables -D OUTPUT ! -f -j ACCEPT".Bash();
        }

        [Fact]
        public void TcpMatchTest()
        {
            // Add test for port range
            // sport/dport
            "iptables -A INPUT -p tcp --sport 1000 --dport 1002 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Equal("1000", match[TcpMatchBuilder.SPORT_OPT]);
                Assert.Equal("1002", match[TcpMatchBuilder.DPORT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --sport 1000 --dport 1002 -j ACCEPT".Bash();
            // sport/dport
            "iptables -A INPUT -p tcp --sport 1000:1003 --dport 1002:1007 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Equal("1000:1003", match[TcpMatchBuilder.SPORT_OPT]);
                Assert.Equal("1002:1007", match[TcpMatchBuilder.DPORT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --sport 1000:1003 --dport 1002:1007 -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp ! --sport 1000 ! --dport 1002 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Equal("1000", match[TcpMatchBuilder.SPORT_OPT.ToOptionName(true)]);
                Assert.Equal("1002", match[TcpMatchBuilder.DPORT_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp ! --sport 1000 ! --dport 1002 -j ACCEPT".Bash();
            //tcp-flags
            "iptables -A INPUT -p tcp --tcp-flags syn,fin,ack syn -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("SYN", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                Assert.Contains("FIN", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --tcp-flags syn,fin,ack syn -j ACCEPT".Bash();

            "iptables -A INPUT -p tcp ! --tcp-flags syn,fin,ack syn -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("SYN", match[TcpMatchBuilder.TCP_FLAGS_OPT.ToOptionName(true)]);
                Assert.Contains("FIN", match[TcpMatchBuilder.TCP_FLAGS_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp ! --tcp-flags syn,fin,ack syn -j ACCEPT".Bash();

            "iptables -A INPUT -p tcp --tcp-flags all none -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("ALL", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                Assert.Contains("NONE", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --tcp-flags all none -j ACCEPT".Bash();

            "iptables -A INPUT -p tcp --tcp-flags none all -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("ALL", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                Assert.Contains("NONE", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --tcp-flags none all -j ACCEPT".Bash();

            "iptables -A INPUT -p tcp ! --tcp-flags all none -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("ALL", match[TcpMatchBuilder.TCP_FLAGS_OPT.ToOptionName(true)]);
                Assert.Contains("NONE", match[TcpMatchBuilder.TCP_FLAGS_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp ! --tcp-flags all none -j ACCEPT".Bash();
            //syn
            "iptables -A INPUT -p tcp --syn -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("SYN", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                Assert.Contains("FIN", match[TcpMatchBuilder.TCP_FLAGS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --syn -j ACCEPT".Bash();

            "iptables -A INPUT -p tcp ! --syn -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TCP)).First();
                Assert.Contains("SYN", match[TcpMatchBuilder.TCP_FLAGS_OPT.ToOptionName(true)]);
                Assert.Contains("FIN", match[TcpMatchBuilder.TCP_FLAGS_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp ! --syn -j ACCEPT".Bash();
            // option
            "iptables -A INPUT -p tcp --tcp-option 16 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(Models.MatchTypes.TCP)).First();
                Assert.Equal("16", match[TcpMatchBuilder.TCP_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp --tcp-option 16 -j ACCEPT".Bash();

            "iptables -A INPUT -p tcp ! --tcp-option 16 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(Models.MatchTypes.TCP)).First();
                Assert.Equal("16", match[TcpMatchBuilder.TCP_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp ! --tcp-option 16 -j ACCEPT".Bash();
        }

        [Fact]
        public void UdpMatchTest()
        {
            // sport/dport
            "iptables -A INPUT -p udp --sport 1000 --dport 1002 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.UDP)).First();
                Assert.Equal("1000", match[UdpMatchBuilder.SPORT_OPT]);
                Assert.Equal("1002", match[UdpMatchBuilder.DPORT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p udp --sport 1000 --dport 1002 -j ACCEPT".Bash();

            "iptables -A INPUT -p udp ! --sport 1000 ! --dport 1002 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.UDP)).First();
                Assert.Equal("1000", match[UdpMatchBuilder.SPORT_OPT.ToOptionName(true)]);
                Assert.Equal("1002", match[UdpMatchBuilder.DPORT_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p udp ! --sport 1000 ! --dport 1002 -j ACCEPT".Bash();
        }

        [Fact]
        public void IcmpMatchTest()
        {
            // icmp-type
            "iptables -A INPUT -p icmp --icmp-type 18 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.ICMP)).First();
                Assert.Equal("address-mask-reply", match[IcmpMatchBuilder.TYPE_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p icmp --icmp-type 18 -j ACCEPT".Bash();

            "iptables -A INPUT -p icmp --icmp-type timestamp-reply -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.ICMP)).First();
                Assert.Equal("timestamp-reply", match[IcmpMatchBuilder.TYPE_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p icmp --icmp-type timestamp-reply -j ACCEPT".Bash();

            "iptables -A INPUT -p icmp --icmp-type any -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.ICMP)).First();
                Assert.Equal("any", match[IcmpMatchBuilder.TYPE_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p icmp --icmp-type any -j ACCEPT".Bash();

            "iptables -A INPUT -p icmp -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.Empty(rule.Matches);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p icmp -j ACCEPT".Bash();
            "iptables -A INPUT -p icmp ! --icmp-type 3 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.ICMP)).First();
                Assert.Equal("network-unreachable", match[IcmpMatchBuilder.TYPE_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p icmp ! --icmp-type 18 -j ACCEPT".Bash();
        }

        [Fact]
        public void LimitMatchTest()
        {
            // sport/dport
            "iptables -A INPUT -m limit -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.LIMIT)).First();
                Assert.Equal("5", match[LimitMatchBuilder.LIMIT_BURST_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m limit -j ACCEPT".Bash();
            "iptables -A INPUT -m limit --limit 10/m -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.LIMIT)).First();
                Assert.Equal("10/m", match[LimitMatchBuilder.LIMIT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m limit --limit 10/m -j ACCEPT".Bash();
        }

        [Fact]
        public void MacMatchTest()
        {
            // sport/dport
            "iptables -A INPUT -m mac --mac-source 01:02:0F:A4:34:01 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MAC)).First();
                Assert.Equal("01:02:0F:A4:34:01", match[MacMatchBuilder.MAC_SOURCE_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m mac --mac-source 01:02:0F:A4:34:01 -j ACCEPT".Bash();
            "iptables -A INPUT -m mac ! --mac-source 01:02:0F:A4:34:01 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MAC)).First();
                Assert.Equal("01:02:0F:A4:34:01", match[MacMatchBuilder.MAC_SOURCE_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m mac ! --mac-source 01:02:0F:A4:34:01 -j ACCEPT".Bash();
        }

        [Fact]
        public void MarkMatchTest()
        {
            // sport/dport
            "iptables -A INPUT -m mark --mark 8 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MARK)).First();
                Assert.Equal("8", match[MarkMatchBuilder.MARK_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m mark --mark 8 -j ACCEPT".Bash();
            "iptables -A INPUT -m mark --mark 8/10 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MARK)).First();
                Assert.Equal("8/10", match[MarkMatchBuilder.MARK_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m mark --mark 8/10 -j ACCEPT".Bash();
            "iptables -A INPUT -m mark ! --mark 8 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MARK)).First();
                Assert.Equal("8", match[MarkMatchBuilder.MARK_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m mark ! --mark 8 -j ACCEPT".Bash();
        }

        [Fact]
        public void MultiportMatchTest()
        {
            // sport
            "iptables -A INPUT -p tcp -m multiport --source-port 22,53,80,110,500:510 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MULTIPORT)).First();
                Assert.Contains("22", match[MultiportMatchBuilder.SOURCE_PORT_OPT]);
                Assert.Contains("80", match[MultiportMatchBuilder.SOURCE_PORT_OPT]);
                Assert.Contains("500:510", match[MultiportMatchBuilder.SOURCE_PORT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m multiport --source-port 22,53,80,110,500:510 -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp -m multiport ! --source-port 22,53,80,110 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MULTIPORT)).First();
                Assert.Contains("22", match[MultiportMatchBuilder.SOURCE_PORT_OPT.ToOptionName(true)]);
                Assert.Contains("80", match[MultiportMatchBuilder.SOURCE_PORT_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m multiport ! --source-port 22,53,80,110 -j ACCEPT".Bash();
            // dport
            "iptables -A INPUT -p tcp -m multiport --destination-port 22,53,80,110 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MULTIPORT)).First();
                Assert.Contains("22", match[MultiportMatchBuilder.DESTINATION_PORT_OPT]);
                Assert.Contains("80", match[MultiportMatchBuilder.DESTINATION_PORT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m multiport --destination-port 22,53,80,110 -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp -m multiport ! --destination-port 22,53,80,110 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MULTIPORT)).First();
                Assert.Contains("22", match[MultiportMatchBuilder.DESTINATION_PORT_OPT.ToOptionName(true)]);
                Assert.Contains("80", match[MultiportMatchBuilder.DESTINATION_PORT_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m multiport ! --destination-port 22,53,80,110 -j ACCEPT".Bash();
            // port
            "iptables -A INPUT -p tcp -m multiport --port 22,53,80,110 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MULTIPORT)).First();
                Assert.Contains("22", match[MultiportMatchBuilder.PORT_OPT]);
                Assert.Contains("80", match[MultiportMatchBuilder.PORT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m multiport --port 22,53,80,110 -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp -m multiport ! --port 22,53,80,110 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.MULTIPORT)).First();
                Assert.Contains("22", match[MultiportMatchBuilder.PORT_OPT.ToOptionName(true)]);
                Assert.Contains("80", match[MultiportMatchBuilder.PORT_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m multiport ! --port 22,53,80,110 -j ACCEPT".Bash();
        }

        [Fact]
        public void OwnerMatchTest()
        {
            // uid-owner
            "iptables -A OUTPUT -m owner --uid-owner 500 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal("500", match[OwnerMatchBuilder.UID_OWNER_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner --uid-owner 500 -j ACCEPT".Bash();
            "iptables -A OUTPUT -m owner --uid-owner 500-700 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal("500-700", match[OwnerMatchBuilder.UID_OWNER_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner --uid-owner 500-700 -j ACCEPT".Bash();
            "iptables -A OUTPUT -m owner ! --uid-owner 500 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal("500", match[OwnerMatchBuilder.UID_OWNER_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner ! --uid-owner 500 -j ACCEPT".Bash();
            // gid-owner
            "iptables -A OUTPUT -m owner --gid-owner 5 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal("5", match[OwnerMatchBuilder.GID_OWNER_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner --gid-owner 5 -j ACCEPT".Bash();
            "iptables -A OUTPUT -m owner --gid-owner 5-7 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal("5-7", match[OwnerMatchBuilder.GID_OWNER_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner --gid-owner 5-7 -j ACCEPT".Bash();
            "iptables -A OUTPUT -m owner ! --gid-owner 5 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal("5", match[OwnerMatchBuilder.GID_OWNER_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner ! --gid-owner 5 -j ACCEPT".Bash();
            // socket-exists
            "iptables -A OUTPUT -m owner --socket-exists -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal(string.Empty, match[OwnerMatchBuilder.SOCKET_EXSTS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner --socket-exists -j ACCEPT".Bash();
            "iptables -A OUTPUT -m owner ! --socket-exists -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.OWNER)).First();
                Assert.Equal(string.Empty, match[OwnerMatchBuilder.SOCKET_EXSTS_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D OUTPUT -m owner ! --socket-exists -j ACCEPT".Bash();
        }

        [Fact]
        public void TosMatchTest()
        {
            // sport/dport
            "iptables -A INPUT -p tcp -m tos --tos 10 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TOS)).First();
                Assert.Equal("10", match[TosMatchBuilder.TOS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m tos --tos 10 -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp -m tos --tos 10/2 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TOS)).First();
                Assert.Equal("10/2", match[TosMatchBuilder.TOS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m tos --tos 10/2 -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp -m tos --tos Minimize-Delay -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TOS)).First();
                Assert.Equal("16/63", match[TosMatchBuilder.TOS_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m tos --tos Minimize-Delay -j ACCEPT".Bash();
            "iptables -A INPUT -p tcp -m tos ! --tos 0x16 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TOS)).First();
                Assert.Equal("22", match[TosMatchBuilder.TOS_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -p tcp -m tos ! --tos 0x16 -j ACCEPT".Bash();
        }

        [Fact]
        public void TtlMatchTest()
        {
            // sport/dport
            "iptables -A INPUT -m ttl --ttl-eq 60 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TTL)).First();
                Assert.Equal("60", match[TtlMatchBuilder.TTL_EQ_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m ttl --ttl-eq 60 -j ACCEPT".Bash();
            "iptables -A INPUT -m ttl ! --ttl-eq 60 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TTL)).First();
                Assert.Equal("60", match[TtlMatchBuilder.TTL_EQ_OPT.ToOptionName(true)]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m ttl ! --ttl-eq 60 -j ACCEPT".Bash();
            "iptables -A INPUT -m ttl --ttl-gt 60 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TTL)).First();
                Assert.Equal("60", match[TtlMatchBuilder.TTL_GT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m ttl --ttl-gt 60 -j ACCEPT".Bash();
            "iptables -A INPUT -m ttl --ttl-lt 60 -j ACCEPT".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotEmpty(rule.Matches);
                var match = rule.Matches.Where(m => m.Name.Equals(MatchTypes.TTL)).First();
                Assert.Equal("60", match[TtlMatchBuilder.TTL_LT_OPT]);
                System.Console.WriteLine(rule);
            }
            "iptables -D INPUT -m ttl --ttl-lt 60 -j ACCEPT".Bash();
        }

        [Theory]
        [InlineData(TargetTypes.ACCEPT)]
        [InlineData(TargetTypes.DROP)]
        [InlineData(TargetTypes.RETURN)]
        public void VerdictTargetTest(string verdict)
        {
            $"iptables -A INPUT -j {verdict}".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(verdict, rule.Target.Name);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j {verdict}".Bash();
        }

        [Fact]
        public void DNatTargetTest()
        {

            $"iptables -t nat -A OUTPUT -o eno6 -j DNAT --to-destination 192.168.1.1-192.168.1.10".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.DNAT, rule.Target.Name);
                Assert.Equal("192.168.1.1-192.168.1.10", rule.Target[DNatTargetBuilder.TO_DESTINATION_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -o eno6 -j DNAT --to-destination 192.168.1.1-192.168.1.10".Bash();

            $"iptables -t nat -A OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1-192.168.1.10:200-300".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.DNAT, rule.Target.Name);
                Assert.Equal("192.168.1.1-192.168.1.10:200-300", rule.Target[DNatTargetBuilder.TO_DESTINATION_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1-192.168.1.10:200-300".Bash();
            $"iptables -t nat -A OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1:89".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.DNAT, rule.Target.Name);
                Assert.Equal("192.168.1.1:89", rule.Target[DNatTargetBuilder.TO_DESTINATION_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1:89".Bash();
            $"iptables -t nat -A OUTPUT -p tcp --dport 10000 -j DNAT --to-destination :89".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.DNAT, rule.Target.Name);
                Assert.Equal(":89", rule.Target[DNatTargetBuilder.TO_DESTINATION_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p tcp --dport 10000 -j DNAT --to-destination :89".Bash();
            $"iptables -t nat -A OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1 --random".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.DNAT, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[DNatTargetBuilder.RANDOM_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1 --random".Bash();
            $"iptables -t nat -A OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1 --persistent".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.DNAT, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[DNatTargetBuilder.PERSISTENT_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p tcp --dport 10000 -j DNAT --to-destination 192.168.1.1 --persistent".Bash();
        }

        [Fact]
        public void SNatTargetTest()
        {

            $"iptables -t nat -A INPUT -i eno6 -j SNAT --to-source 192.168.1.1-192.168.1.10".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal("192.168.1.1-192.168.1.10", rule.Target[SNatTargetBuilder.TO_SOURCE_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -i eno6 -j SNAT --to-source 192.168.1.1-192.168.1.10".Bash();

            $"iptables -t nat -A INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1-192.168.1.10:200-300".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal("192.168.1.1-192.168.1.10:200-300", rule.Target[SNatTargetBuilder.TO_SOURCE_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1-192.168.1.10:200-300".Bash();
            $"iptables -t nat -A INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1:89".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal("192.168.1.1:89", rule.Target[SNatTargetBuilder.TO_SOURCE_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1:89".Bash();
            $"iptables -t nat -A INPUT -p tcp --sport 10000 -j SNAT --to-source :89".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal(":89", rule.Target[SNatTargetBuilder.TO_SOURCE_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -p tcp --sport 10000 -j SNAT --to-source :89".Bash();
            $"iptables -t nat -A INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1 --random".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[SNatTargetBuilder.RANDOM_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1 --random".Bash();
            $"iptables -t nat -A INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1 --random-fully".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[SNatTargetBuilder.RANDOM_FULLY_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1 --random-fully".Bash();
            $"iptables -t nat -A INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1 --persistent".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.SNAT, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[SNatTargetBuilder.PERSISTENT_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D INPUT -p tcp --sport 10000 -j SNAT --to-source 192.168.1.1 --persistent".Bash();
        }
        [Fact]
        public void LogTargetTest()
        {
            $"iptables -A INPUT -j LOG --log-level debug".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.LOG, rule.Target.Name);
                Assert.Equal($"{Native.LogLevels.DEBUG}", rule.Target[LogTargetBuilder.LOG_LEVEL_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j LOG --log-level debug".Bash();
            $"iptables -A INPUT -j LOG --log-prefix log_test".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.LOG, rule.Target.Name);
                Assert.Equal("log_test", rule.Target[LogTargetBuilder.LOG_PREFIX_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j LOG --log-prefix log_test".Bash();
            $"iptables -A INPUT -j LOG --log-tcp-sequence".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.LOG, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[LogTargetBuilder.LOG_TCP_SEQUENCE_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j LOG --log-tcp-sequence".Bash();
            $"iptables -A INPUT -j LOG --log-tcp-options".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.LOG, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[LogTargetBuilder.LOG_TCP_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j LOG --log-tcp-options".Bash();
            $"iptables -A INPUT -j LOG --log-ip-options".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.LOG, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[LogTargetBuilder.LOG_IP_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j LOG --log-ip-options".Bash();
            $"iptables -A INPUT -j LOG --log-uid".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.LOG, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[LogTargetBuilder.LOG_UID_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D INPUT -j LOG --log-uid".Bash();
        }

        [Fact]
        public void MasqueradeTargetTest()
        {
            $"iptables -t nat -A POSTROUTING -p TCP --sport 1000 -j MASQUERADE --to-ports 1024-31000".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.POSTROUTING);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.MASQUERADE, rule.Target.Name);
                Assert.Equal("1024-31000", rule.Target[MasqueradeTargetBuilder.TO_PORTS_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D POSTROUTING -p TCP --sport 1000 -j MASQUERADE --to-ports 1024-31000".Bash();
            $"iptables -t nat -A POSTROUTING -p TCP --sport 1000 -j MASQUERADE --to-ports 1024".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.POSTROUTING);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.MASQUERADE, rule.Target.Name);
                Assert.Equal("1024", rule.Target[MasqueradeTargetBuilder.TO_PORTS_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D POSTROUTING -p TCP --sport 1000 -j MASQUERADE --to-ports 1024".Bash();
            $"iptables -t nat -A POSTROUTING -p TCP --sport 1000 -j MASQUERADE --random".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.POSTROUTING);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.MASQUERADE, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[MasqueradeTargetBuilder.RANDOM_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D POSTROUTING -p TCP --sport 1000 -j MASQUERADE --random".Bash();
        }

        [Fact]
        public void RedirectTargetTest()
        {
            $"iptables -t nat -A OUTPUT -p TCP --dport 1000 -j REDIRECT --to-ports 1024-31000".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.REDIRECT, rule.Target.Name);
                Assert.Equal("1024-31000", rule.Target[RedirectTargetBuilder.TO_PORTS_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p TCP --dport 1000 -j REDIRECT --to-ports 1024-31000".Bash();
            $"iptables -t nat -A OUTPUT -p TCP --dport 1000 -j REDIRECT --to-ports 1024".Bash();
            using (var wr = new IO.IptTransaction(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.REDIRECT, rule.Target.Name);
                Assert.Equal("1024", rule.Target[RedirectTargetBuilder.TO_PORTS_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p TCP --dport 1000 -j REDIRECT --to-ports 1024".Bash();
            /* not work
            $"iptables -t nat -A OUTPUT -p TCP -o eno12 -j REDIRECT --random".Bash();
            using (var wr = new IO.IptWrapper(IO.Tables.NAT))
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.REDIRECT, rule.Target.Name);
                Assert.Equal(string.Empty, rule.Target[RedirectTarget.RANDOM_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -t nat -D OUTPUT -p TCP -o eno12 -j REDIRECT --random".Bash();
            */
        }

        [Fact]
        public void RejectTargetTest()
        {
            $"iptables -A OUTPUT -p TCP --dport 1000 -j REJECT --reject-with tcp-reset".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.REJECT, rule.Target.Name);
                Assert.Equal("tcp-reset", rule.Target[RejectTargetBuilder.REJECT_WITH_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D OUTPUT -p TCP --dport 1000 -j REJECT --reject-with tcp-reset".Bash();
            $"iptables -A OUTPUT -p TCP --dport 1000 -j REJECT --reject-with icmp-host-unreachable".Bash();
            using (var wr = new IO.IptTransaction())
            {
                var rules = wr.GetRules(IO.Chains.OUTPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                Assert.NotNull(rule.Target);
                Assert.Equal(TargetTypes.REJECT, rule.Target.Name);
                Assert.Equal("icmp-host-unreachable", rule.Target[RejectTargetBuilder.REJECT_WITH_OPT]);
                System.Console.WriteLine(rule);
            }
            $"iptables -D OUTPUT -p TCP --dport 1000 -j REJECT --reject-with icmp-host-unreachable".Bash();
        }
    }
}
