using System.Linq;
using Xunit;
using IptablesCtl.Models;
using IptablesCtl.Models.Builders;

namespace IptablesCtl.Test
{
    public class TestProperties
    {

        [Fact]
        public void ToIpProtoRange()
        {
            Assert.Equal("192.156.2.3".ParseIpv4(),"192.156.2.3:20".ToIpProtoRange().minIp);
            Assert.Equal(uint.MaxValue,":20".ToIpProtoRange().maxIp);
            Assert.Equal(ushort.MaxValue,"192.156.2.3-192.156.2.33".ToIpProtoRange().maxP);
        }

        [Fact]
         public void TestMaskedProperties()
        {
            Assert.Equal("192.156.2.3", "192.156.2.3".ToMaskedProperty('/').Value);
            Assert.Equal("192.156.2.3", "192.156.2.3/".ToMaskedProperty('/').Value);
            Assert.Equal("192.156.2.3", "192.156.2.3/2".ToMaskedProperty('/').Value);
            Assert.Equal("192.156.2.3", "192.156.2.3/55".ToMaskedProperty('/', "55").Value);
            Assert.Equal("55", "192.156.2.3/55".ToMaskedProperty('/', "55").Mask);
            Assert.Equal("55", "192.156.2.3/55".ToMaskedProperty('/').Mask);
            Assert.Null("192.156.2.3".ToMaskedProperty('/').Mask);
        }

        [Fact]
        public void TestRangeProperties()
        {
            Assert.Equal("33", "33:10".ToRangeProperty(':').Left);
            Assert.Equal("33", "33:".ToRangeProperty(':').Left);
            Assert.Equal("33", "33".ToRangeProperty(':').Left);
            Assert.Equal("10", "33:10".ToRangeProperty(':').Rigt);
            Assert.Equal("10", "10".ToRangeProperty(':').Rigt);
            Assert.Equal("33", "33:".ToRangeProperty(':').Rigt);
        }

        [Fact]
        public void TestIpv4Properties()
        {
            Assert.Equal(45L, "0.0.0.45".ParseIpv4());
            Assert.Equal(45L | 8L << 8, "0.0.8.45".ParseIpv4());
            Assert.Equal(45L | 8L << 24, "8.0.0.45".ParseIpv4());
            Assert.Equal(8L << 24, "8.0.0".ParseIpv4());
        }

        [Fact]
        public void TestMacProperties()
        {
            Assert.Equal(new byte[]{1,2,15,164,52,1}, "01:02:0F:A4:34:01".ParseMacaddr());
        }

        [Fact]
        public void TestMultiportProperties()
        {
            Assert.Equal(new ushort[]{12,23,55,77,90,0,0,0,0,0}, "12,23,55:77,90".ParseMultiports(10));
            Assert.Equal(new byte[]{0,0,1,0,0,0,0,0,0,0}, "12,23,55:77,90".ParseMultiportsFlag(10));
        }

        [Fact]
        public void TestParseIpProtoRange()
        {
            uint minIp = 2344, maxIp = 5589;
            ushort minP = 45, maxP = 876;
            var minIpStr = Options.ToIp4String(minIp);
            var maxIpStr = Options.ToIp4String(maxIp);
            var src = $"{minIpStr}-{maxIpStr}:{minP}-{maxP}".ToIpProtoRange();
            Assert.Equal(minIp, src.minIp);
            Assert.Equal(maxIp, src.maxIp);
            Assert.Equal(minP, src.minP);
            Assert.Equal(maxP, src.maxP);
            src = $"{maxIpStr}:{minP}-{maxP}".ToIpProtoRange();
            Assert.Equal(maxIp, src.minIp);
            Assert.Equal(maxIp, src.maxIp);
            Assert.Equal(minP, src.minP);
            Assert.Equal(maxP, src.maxP);
            src = $":{minP}-{maxP}".ToIpProtoRange();
            Assert.Equal(0L, src.minIp);
            Assert.Equal(0L, src.maxIp);
            Assert.Equal(minP, src.minP);
            Assert.Equal(maxP, src.maxP);
            src = $"{minIpStr}-{maxIpStr}:{minP}".ToIpProtoRange();
            Assert.Equal(minIp, src.minIp);
            Assert.Equal(maxIp, src.maxIp);
            Assert.Equal(minP, src.minP);
            Assert.Equal(minP, src.maxP);
        }
        [Fact]
        public void TestOptions()
        {
            var rule = new RuleBuilder()
                .SetIp4Src("192.168.3.2/23")
                .SetIp4Dst("192.168.3")
                .SetInInterface("eno8")
                .SetOutInterface("eno45", true, true)
                .SetProto("tCp")
                .Accept();
            Assert.True(rule.ContainsOption(RuleBuilder.OUT_INTERFACE_OPT));
            if (rule.TryGetOption(RuleBuilder.OUT_INTERFACE_OPT, out var inf))
            {
                Assert.Equal("eno45+",inf.Value);
            }
        }
    }
}