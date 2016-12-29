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
        public async Task BasicExample()
        {
            var domainParser = new DomainParser(new WebTldRuleProvider());
            var domainInfo = await domainParser.ParseAsync("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            Assert.Equal("sub.test.co.uk", domainInfo.Hostname);
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("sub", domainInfo.SubDomain);
            Assert.Equal("co.uk", domainInfo.Tld);
            Assert.Equal(TldRuleDivision.ICANN, domainInfo.TldRule.Division);
        }

        [Fact]
        public async Task RuleDivisionExample()
        {
            var provider = new WebTldRuleProvider();
            var rules = await provider.BuildAsync();

            //ICANN and Private rules
            var domainParser = new DomainParser(rules);
            var domainInfo = await domainParser.ParseAsync("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            domainInfo = await domainParser.ParseAsync("www.myblog.blogspot.co.uk");
            Assert.Equal("myblog.blogspot.co.uk", domainInfo.RegistrableDomain);

            //ICANN only rules
            var icannRules = rules.Where(x => x.Division == TldRuleDivision.ICANN);
            domainParser = new DomainParser(icannRules);
            domainInfo = await domainParser.ParseAsync("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            domainInfo = await domainParser.ParseAsync("www.myblog.blogspot.co.uk");
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

            var provider = new WebTldRuleProvider(cachedFile, plublicSuffixUrl, timeToLive);
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
        public async Task FileProviderExample()
        {
            string fileName = "effective_tld_names.dat"; //This file exists in Test project
            //Important: project.json is set to copy this file on build

            var domainParser = new DomainParser(new FileTldRuleProvider(fileName));
            var domainInfo = await domainParser.ParseAsync("sub.test.co.uk");
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
        }
    }
}
