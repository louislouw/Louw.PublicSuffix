using System.Collections;
using Xunit;

namespace Louw.PublicSuffix.UnitTests
{
    public class DomainParserTest
    {
        [Fact]
        public void CheckDomainName1()
        {
            var domainParser = new DomainParser(new TldRule[] 
            {
                new TldRule("com")
            });

            var domainInfo = domainParser.Get("test.com");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("com", domainInfo.Tld);
            Assert.Equal("test.com", domainInfo.RegistrableDomain);
            Assert.Equal(null, domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }

        [Fact]
        public void CheckDomainName2()
        {
            var domainParser = new DomainParser(new TldRule[] 
            {
                new TldRule("uk"),
                new TldRule("co.uk")
            });

            var domainInfo = domainParser.Get("test.co.uk");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("co.uk", domainInfo.Tld);
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            Assert.Equal(null, domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }

        [Fact]
        public void CheckDomainName3()
        {
            var domainParser = new DomainParser(new TldRule[]
            {
                new TldRule("uk"),
                new TldRule("co.uk")
            });

            var domainInfo = domainParser.Get("sub.test.co.uk");
            Assert.Equal("test", domainInfo.Domain);
            Assert.Equal("co.uk", domainInfo.Tld);
            Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
            Assert.Equal("sub", domainInfo.SubDomain);
            Assert.NotNull(domainInfo.TldRule);
        }
    }
}
