using Xunit;

namespace Louw.PublicSuffix.UnitTests
{
    public class DomainNameTest
    {
        [Fact]
        public void CheckDomainName1()
        {
            var domainParser = new DomainParser();
            domainParser.AddRule(new TldRule("com"));

            var domainName = domainParser.Get("test.com");
            Assert.Equal("test", domainName.Domain);
            Assert.Equal("com", domainName.TLD);
            Assert.Equal("test.com", domainName.RegistrableDomain);
            Assert.Equal(null, domainName.SubDomain);
        }

        [Fact]
        public void CheckDomainName2()
        {
            var domainParser = new DomainParser();
            domainParser.AddRule(new TldRule("uk"));
            domainParser.AddRule(new TldRule("co.uk"));

            var domainName = domainParser.Get("test.co.uk");
            Assert.Equal("test", domainName.Domain);
            Assert.Equal("co.uk", domainName.TLD);
            Assert.Equal("test.co.uk", domainName.RegistrableDomain);
            Assert.Equal(null, domainName.SubDomain);
        }

        [Fact]
        public void CheckDomainName3()
        {
            var domainParser = new DomainParser();
            domainParser.AddRule(new TldRule("uk"));
            domainParser.AddRule(new TldRule("co.uk"));

            var domainName = domainParser.Get("sub.test.co.uk");
            Assert.Equal("test", domainName.Domain);
            Assert.Equal("co.uk", domainName.TLD);
            Assert.Equal("test.co.uk", domainName.RegistrableDomain);
            Assert.Equal("sub", domainName.SubDomain);
        }
    }
}
