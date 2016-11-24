using System.Linq;
using Xunit;

namespace Louw.PublicSuffix.UnitTest
{
    public class TldRuleParserTest
    {
        [Fact]
        public void ParseValidData1()
        {
            var lines = new string[] { "com", "uk", "co.uk" };

            var ruleParser = new TldRuleParser();
            var tldRules = ruleParser.ParseRules(lines).ToList();

            Assert.Equal("com", tldRules[0].Name);
            Assert.Equal("uk", tldRules[1].Name);
            Assert.Equal("co.uk", tldRules[2].Name);
        }

        [Fact]
        public void ParseValidData2()
        {
            var lines = new string[] { "com", "//this is a example comment", "uk", "co.uk" };

            var ruleParser = new TldRuleParser();
            var tldRules = ruleParser.ParseRules(lines).ToList();

            Assert.Equal("com", tldRules[0].Name);
            Assert.Equal("uk", tldRules[1].Name);
            Assert.Equal("co.uk", tldRules[2].Name);
        }

        [Fact]
        public void ParseValidData3()
        {
            var lines = new string[] 
            {
                "example.above",
                "// ===BEGIN ICANN DOMAINS===",
                "uk", "co.uk",
                "// ===END ICANN DOMAINS===",
                "example.between",
                "// ===BEGIN PRIVATE DOMAINS===",
                "blogspot.com","no-ip.co.uk",
                "// ===END PRIVATE DOMAINS===",
                "example.after"
            };

            var ruleParser = new TldRuleParser();
            var tldRules = ruleParser.ParseRules(lines).ToList();

            Assert.Equal("example.above", tldRules[0].Name);
            Assert.Equal(TldRuleDivision.Unknown, tldRules[0].Division);

            Assert.Equal("uk", tldRules[1].Name);
            Assert.Equal(TldRuleDivision.ICANN, tldRules[1].Division);
            Assert.Equal("co.uk", tldRules[2].Name);
            Assert.Equal(TldRuleDivision.ICANN, tldRules[2].Division);

            Assert.Equal("example.between", tldRules[3].Name);
            Assert.Equal(TldRuleDivision.Unknown, tldRules[3].Division);

            Assert.Equal("blogspot.com", tldRules[4].Name);
            Assert.Equal(TldRuleDivision.Private, tldRules[4].Division);
            Assert.Equal("no-ip.co.uk", tldRules[5].Name);
            Assert.Equal(TldRuleDivision.Private, tldRules[5].Division);

            Assert.Equal("example.after", tldRules[6].Name);
            Assert.Equal(TldRuleDivision.Unknown, tldRules[6].Division);
        }
    }
}
