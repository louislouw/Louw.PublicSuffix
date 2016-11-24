using Xunit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Louw.PublicSuffix.UnitTests
{
    public class FileTldRuleProviderTest
    {
        [Fact]
        public async Task CheckFileRules()
        {
            string dataFile = Path.Combine("effective_tld_names.dat");

            ITldRuleProvider provider = new FileTldRuleProvider(dataFile);
            var rules = await provider.BuildAsync();
            Assert.NotNull(rules);
            var ruleList = rules.ToList();
            Assert.True(ruleList.Count > 100); //Expecting lots of rules

            //Spot checks
            var spotChecks = new string[] { "com", "*.bd", "blogspot.com" };
            var lookup = ruleList.ToDictionary(x => x.Name, x => x.Name);
            Assert.True(spotChecks.All(x => lookup.ContainsKey(x)));
        }

        [Fact]
        public async Task CheckFileNotValid()
        {
            string dataFile = Path.Combine("this_is_not_valid.dat");
            ITldRuleProvider provider = new FileTldRuleProvider(dataFile);

            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.BuildAsync());
        }
    }
}
