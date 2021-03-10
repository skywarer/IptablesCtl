using System;
using System.Linq;
using Xunit;
using IptablesCtl.IO;
using IptablesCtl.IO.Extentions;
using IptablesCtl.Models;
using IptablesCtl.Models.Builders;
using IptablesCtl.Models.Builders.Extentions;

namespace IptablesCtl.Test
{
    public class TestExtended
    {
        public TestExtended()
        {
            "iptables -F INPUT".Bash();
            "iptables -F OUTPUT".Bash();
            "iptables -F FORWARD".Bash();

            "iptables -t nat -F INPUT".Bash();
            "iptables -t nat -F OUTPUT".Bash();
            "iptables -t nat -F PREROUTING".Bash();
            "iptables -t nat -F POSTROUTING".Bash();

            "iptables -t mangle -F INPUT".Bash();
        }

        [Fact]
        public void WriteCommentRule()
        {
            var commentMatch = new CommentMatchBuilder()
                .SetComment("test comment").Build();
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.3.2/23")
                .SetProto("tCp")
                .AddMatch(commentMatch)
                .Accept();
            System.Console.WriteLine(rule);
            using (var wr = new IptExtended())
            {
                wr.AppendRule(Chains.INPUT,rule);
                wr.Commit(); 
                var rules = wr.GetRules(Chains.INPUT);
                rule = rules.First();    
                System.Console.WriteLine(rule);     
                Assert.NotEmpty(rules);   
                var match = rule.Matches.First();   
                Assert.Equal("test comment", match[CommentMatchBuilder.COMMENT_OPT]);
            }
        }
    }
}