using System.Collections;
using System.Threading.Tasks;
using Xunit;

namespace Louw.PublicSuffix.UnitTests
{
    public class DomainParserTest
    {
        [Fact]
        public async Task CheckDomainName1()
        {
            var domainParser = new DomainParser(new TldRule[] 
            {
                new TldRule("com")
            });

            var domainInfo = await domainParser.ParseAsync("test.com");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("com", domainInfo.Tld);
            Assert.Equal("test.com", domainInfo.RegistrableDomain);
            Assert.Equal(null, domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }

        [Fact]
        public async Task CheckDomainName2()
        {
            var domainParser = new DomainParser(new TldRule[] 
            {
                new TldRule("uk"),
                new TldRule("co.uk")
            });

            var domainInfo = await domainParser.ParseAsync("test.co.uk");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("co.uk", domainInfo.Tld);
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            Assert.Equal(null, domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }

        [Fact]
        public async Task CheckDomainName3()
        {
            var domainParser = new DomainParser(new TldRule[]
            {
                new TldRule("uk"),
                new TldRule("co.uk")
            });

            var domainInfo = await domainParser.ParseAsync("sub.test.co.uk");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("co.uk", domainInfo.Tld);
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            Assert.Equal("sub", domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }

        [Fact]
        public async Task CheckDomainName4()
        {
            var domainParser = new DomainParser(new TldRule[]
            {
                new TldRule("uk"),
                new TldRule("co.uk")
            });

            //Check if we can handle full URL
            var domainInfo = await domainParser.ParseAsync("http://sub.test.co.uk/path?query#fragement");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("co.uk", domainInfo.Tld);
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            Assert.Equal("sub", domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }
    }
}
