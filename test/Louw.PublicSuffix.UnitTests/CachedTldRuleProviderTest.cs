using Xunit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Louw.PublicSuffix.UnitTests
{
    public class CachedTldRuleProviderTest
    {
        [Fact]
        public async Task CheckCachedRules()
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), "UnitTestPublicSuffixList.dat");
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }

            ITldRuleProvider provider = new CachedTldRuleProvider(fileName: tmpFile);
            var rules = await provider.BuildAsync();
            Assert.True(File.Exists(tmpFile));
            Assert.NotNull(rules);
            var ruleList = rules.ToList();
            Assert.True(ruleList.Count > 100); //Expecting lots of rules

            //Spot checks (If test fails here, verify that rules still exist on:
            //https://publicsuffix.org/list/public_suffix_list.dat
            var spotChecks = new string[] { "com", "*.bd", "blogspot.com" };
            var lookup = ruleList.ToDictionary(x => x.Name, x => x.Name);
            Assert.True(spotChecks.All(x => lookup.ContainsKey(x)));

            //Verify cache
            var fileDateBefore = File.GetLastWriteTimeUtc(tmpFile);
            var cachedRules = await provider.BuildAsync();
            var fileDateAfter = File.GetLastWriteTimeUtc(tmpFile);
            Assert.Equal(ruleList.Count, cachedRules.Count());
            Assert.Equal(fileDateBefore, fileDateAfter);
            Assert.True(cachedRules.All(x => lookup.ContainsKey(x.Name)));

            //Cleanup
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }
}
