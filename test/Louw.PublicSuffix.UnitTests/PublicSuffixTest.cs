using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace Louw.PublicSuffix.UnitTests
{
    //Comprehensive tests
    //Run tests as specified here:
    //https://raw.githubusercontent.com/publicsuffix/list/master/tests/test_psl.txt

    public class PublicSuffixTest
    {
        private DomainParser _domainParser;

        public PublicSuffixTest()
        {
            string ruleFile = "effective_tld_names.dat";
            Assert.True(File.Exists(ruleFile));

            var domainParser = new DomainParser(new FileTldRuleProvider(ruleFile));

            this._domainParser = domainParser;
        }

        private async Task CheckPublicSuffix(string domain, string expected)
        {
            Assert.NotNull(this._domainParser);

            if (!string.IsNullOrEmpty(domain))
            {
                domain = domain.ToLowerInvariant();
            }

            var domainData = await this._domainParser.ParseAsync(domain);
            if (domainData == null)
            {
                Assert.Null(expected);
            }
            else
            {
                Assert.Equal(expected, domainData.RegistrableDomain);
            }
        }

        [Fact]
        public async Task ComprehensiveCheck()
        {
            // null input.
            await this.CheckPublicSuffix(null, null);

            // Mixed case.
            await this.CheckPublicSuffix("COM", null);
            await this.CheckPublicSuffix("example.COM", "example.com");
            await this.CheckPublicSuffix("WwW.example.COM", "example.com");

            // Leading dot.
            await this.CheckPublicSuffix(".com", null);
            await this.CheckPublicSuffix(".example", null);
            await this.CheckPublicSuffix(".example.com", null);
            await this.CheckPublicSuffix(".example.example", null);

            // Unlisted TLD.
            await this.CheckPublicSuffix("example", null);
            await this.CheckPublicSuffix("example.example", "example.example");
            await this.CheckPublicSuffix("b.example.example", "example.example");
            await this.CheckPublicSuffix("a.b.example.example", "example.example");

            // Listed, but non-Internet, TLD.
            //await this.CheckPublicSuffix("local", null);
            //await this.CheckPublicSuffix("example.local", null);
            //await this.CheckPublicSuffix("b.example.local", null);
            //await this.CheckPublicSuffix("a.b.example.local", null);

            // TLD with only 1 rule.
            await this.CheckPublicSuffix("biz", null);
            await this.CheckPublicSuffix("domain.biz", "domain.biz");
            await this.CheckPublicSuffix("b.domain.biz", "domain.biz");
            await this.CheckPublicSuffix("a.b.domain.biz", "domain.biz");

            // TLD with some 2-level rules.
            await this.CheckPublicSuffix("com", null);
            await this.CheckPublicSuffix("example.com", "example.com");
            await this.CheckPublicSuffix("b.example.com", "example.com");
            await this.CheckPublicSuffix("a.b.example.com", "example.com");
            await this.CheckPublicSuffix("uk.com", null);
            await this.CheckPublicSuffix("example.uk.com", "example.uk.com");
            await this.CheckPublicSuffix("b.example.uk.com", "example.uk.com");
            await this.CheckPublicSuffix("a.b.example.uk.com", "example.uk.com");
            await this.CheckPublicSuffix("test.ac", "test.ac");

            // TLD with only 1 (wildcard) rule.
            await this.CheckPublicSuffix("mm", null);
            await this.CheckPublicSuffix("c.mm", null);
            await this.CheckPublicSuffix("b.c.mm", "b.c.mm");
            await this.CheckPublicSuffix("a.b.c.mm", "b.c.mm");

            // More complex TLD.
            await this.CheckPublicSuffix("jp", null);
            await this.CheckPublicSuffix("test.jp", "test.jp");
            await this.CheckPublicSuffix("www.test.jp", "test.jp");
            await this.CheckPublicSuffix("ac.jp", null);
            await this.CheckPublicSuffix("test.ac.jp", "test.ac.jp");
            await this.CheckPublicSuffix("www.test.ac.jp", "test.ac.jp");
            await this.CheckPublicSuffix("kyoto.jp", null);
            await this.CheckPublicSuffix("test.kyoto.jp", "test.kyoto.jp");
            await this.CheckPublicSuffix("ide.kyoto.jp", null);
            await this.CheckPublicSuffix("b.ide.kyoto.jp", "b.ide.kyoto.jp");
            await this.CheckPublicSuffix("a.b.ide.kyoto.jp", "b.ide.kyoto.jp");
            await this.CheckPublicSuffix("c.kobe.jp", null);
            await this.CheckPublicSuffix("b.c.kobe.jp", "b.c.kobe.jp");
            await this.CheckPublicSuffix("a.b.c.kobe.jp", "b.c.kobe.jp");
            await this.CheckPublicSuffix("city.kobe.jp", "city.kobe.jp");
            await this.CheckPublicSuffix("www.city.kobe.jp", "city.kobe.jp");

            // TLD with a wildcard rule and exceptions.
            await this.CheckPublicSuffix("ck", null);
            await this.CheckPublicSuffix("test.ck", null);
            await this.CheckPublicSuffix("b.test.ck", "b.test.ck");
            await this.CheckPublicSuffix("a.b.test.ck", "b.test.ck");
            await this.CheckPublicSuffix("www.ck", "www.ck");
            await this.CheckPublicSuffix("www.www.ck", "www.ck");

            // US K12.
            await this.CheckPublicSuffix("us", null);
            await this.CheckPublicSuffix("test.us", "test.us");
            await this.CheckPublicSuffix("www.test.us", "test.us");
            await this.CheckPublicSuffix("ak.us", null);
            await this.CheckPublicSuffix("test.ak.us", "test.ak.us");
            await this.CheckPublicSuffix("www.test.ak.us", "test.ak.us");
            await this.CheckPublicSuffix("k12.ak.us", null);
            await this.CheckPublicSuffix("test.k12.ak.us", "test.k12.ak.us");
            await this.CheckPublicSuffix("www.test.k12.ak.us", "test.k12.ak.us");
        }

        [Fact]
        public async Task IdnDomainCheck()
        {
            // IDN labels.
            await this.CheckPublicSuffix("食狮.com.cn", "食狮.com.cn");
            await this.CheckPublicSuffix("食狮.公司.cn", "食狮.公司.cn");
            await this.CheckPublicSuffix("www.食狮.公司.cn", "食狮.公司.cn");
            await this.CheckPublicSuffix("shishi.公司.cn", "shishi.公司.cn");
            await this.CheckPublicSuffix("公司.cn", null);
            await this.CheckPublicSuffix("食狮.中国", "食狮.中国");
            await this.CheckPublicSuffix("www.食狮.中国", "食狮.中国");
            await this.CheckPublicSuffix("shishi.中国", "shishi.中国");
            await this.CheckPublicSuffix("中国", null);

            // Same as above, but punycoded.
            await this.CheckPublicSuffix("xn--85x722f.com.cn", "xn--85x722f.com.cn");
            await this.CheckPublicSuffix("xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
            await this.CheckPublicSuffix("www.xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
            await this.CheckPublicSuffix("shishi.xn--55qx5d.cn", "shishi.xn--55qx5d.cn");
            await this.CheckPublicSuffix("xn--55qx5d.cn", null);
            await this.CheckPublicSuffix("xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
            await this.CheckPublicSuffix("www.xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
            await this.CheckPublicSuffix("shishi.xn--fiqs8s", "shishi.xn--fiqs8s");
            await this.CheckPublicSuffix("xn--fiqs8s", null);
        }

        [Fact]
        public async Task TreeSplitCheck()
        {
            //Extra tests (Added to avoid regression bugs)
            await this.CheckPublicSuffix("co.ke", null);
            await this.CheckPublicSuffix("blogspot.co.ke", null);
            await this.CheckPublicSuffix("web.co.ke", "web.co.ke");
            await this.CheckPublicSuffix("a.b.web.co.ke", "web.co.ke");
            await this.CheckPublicSuffix("blogspot.co.ke", null);
            await this.CheckPublicSuffix("web.blogspot.co.ke", "web.blogspot.co.ke");
            await this.CheckPublicSuffix("a.b.web.blogspot.co.ke", "web.blogspot.co.ke");
        }
    }
}
