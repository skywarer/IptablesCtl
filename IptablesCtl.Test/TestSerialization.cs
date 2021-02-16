using System.Linq;
using System;
using Xunit;
using IptablesCtl.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IptablesCtl.Test
{
    public class TestSerialization
    {
        public TestSerialization()
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
        public void OptionsSerializeTest()
        {
            Options opt = new Options(new Dictionary<string, string>() { { "key", "value" } });
            var optJson = System.Text.Json.JsonSerializer.Serialize(opt);
            Console.WriteLine(optJson);
            var dOpt = System.Text.Json.JsonSerializer.Deserialize<Options>(optJson);
            Assert.Equal(opt["key"], dOpt["key"]);
        }

        [Fact]
        public void MatchSerializeTest()
        {
            Match match = new Match("match", true, new Dictionary<string, string>() { { "key", "value" } });
            var matchJson = System.Text.Json.JsonSerializer.Serialize(match);
            Console.WriteLine(matchJson);
            var dMatch = System.Text.Json.JsonSerializer.Deserialize<Match>(matchJson);
            Assert.Equal(match["key"], dMatch["key"]);
            Assert.Equal(match.Name, dMatch.Name);
            Assert.Equal(match.NeedKey, dMatch.NeedKey);
        }

        [Fact]
        public void TargetSerializeTest()
        {
            Target target = new Target("target", new Dictionary<string, string>() { { "key", "value" } });
            var targetJson = System.Text.Json.JsonSerializer.Serialize(target);
            Console.WriteLine(targetJson);
            var dTarget = System.Text.Json.JsonSerializer.Deserialize<Target>(targetJson);
            Assert.Equal(target["key"], dTarget["key"]);
            Assert.Equal(target.Name, dTarget.Name);
        }

        [Fact]
        public void RuleSerializeTest()
        {
            "iptables -A INPUT -p tcp --sport 1000 --dport 1002 -j ACCEPT".Bash();
            using (var wr = new IO.IptWrapper())
            {
                var rules = wr.GetRules(IO.Chains.INPUT);
                Assert.NotEmpty(rules);
                var rule = rules.First();
                var ruleJson = System.Text.Json.JsonSerializer.Serialize(rule);
                System.Console.WriteLine(rule);
                System.Console.WriteLine(ruleJson);
                var dRule = System.Text.Json.JsonSerializer.Deserialize<Rule>(ruleJson);
                Assert.Equal(rule.Target.Name,dRule.Target.Name);
                Assert.Equal(rule.Matches.Count,dRule.Matches.Count);
            }
            "iptables -D INPUT -p tcp --sport 1000 --dport 1002 -j ACCEPT".Bash();
        }
    }
}