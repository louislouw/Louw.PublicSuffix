using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Louw.PublicSuffix.UnitTests
{
    public class Examples
    {
        [Fact]
        public void BasicExample()
        {
            var domainParser = new DomainParser(new CachedTldRuleProvider());
            var domainInfo = domainParser.Get("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
        }

        [Fact]
        public async Task RuleDivisionExample()
        {
            var provider = new CachedTldRuleProvider();
            var rules = await provider.BuildAsync();

            //ICANN and Private rules
            var domainParser = new DomainParser(rules);
            var domainInfo = domainParser.Get("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            domainInfo = domainParser.Get("www.myblog.blogspot.co.uk");
            Assert.Equal("myblog.blogspot.co.uk", domainInfo.RegistrableDomain);

            //ICANN only rules
            var icannRules = rules.Where(x => x.Division == TldRuleDivision.ICANN);
            domainParser = new DomainParser(icannRules);
            domainInfo = domainParser.Get("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            domainInfo = domainParser.Get("www.myblog.blogspot.co.uk");
            Assert.Equal("blogspot.co.uk", domainInfo.RegistrableDomain); //Now TLD returned
        }

        [Fact]
        public async Task CachedProviderExample()
        {
            //Cache Public Suffix data to file, refresh after TTL expires
            //All params are optional (in which cases it uses same values as below as defaults)
            var cachedFile = Path.Combine(Path.GetTempPath(), "public_suffix_list.dat");
            var plublicSuffixUrl = @"https://publicsuffix.org/list/public_suffix_list.dat";
            var timeToLive = TimeSpan.FromDays(1);

            var provider = new CachedTldRuleProvider(cachedFile, plublicSuffixUrl, timeToLive);
            var rules = await provider.BuildAsync();
            Assert.NotEmpty(rules);
            Assert.False(provider.MustRefresh());

            //Lets manually force refresh of cache
            File.Delete(cachedFile);
            Assert.True(provider.MustRefresh());
            await provider.Refresh();
            Assert.False(provider.MustRefresh());
        }

        [Fact]
        public void FileProviderExample()
        {
            string fileName = "effective_tld_names.dat"; //This file exists in Test project
            //Important: project.json is set to copy this file on build

            var domainParser = new DomainParser(new FileTldRuleProvider(fileName));
            var domainInfo = domainParser.Get("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
        }
    }
}
