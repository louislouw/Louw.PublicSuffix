using System.Linq;

namespace Louw.PublicSuffix
{
    public class DomainInfo
    {
        public string Domain { get; private set; }
        public string Tld { get; private set; }
        public string SubDomain { get; private set; }
        public string RegistrableDomain { get; private set; }
        public string Hostname { get; private set; }
        public TldRule TldRule { get; private set; }

        public DomainInfo(string domain, TldRule tldRule)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return;
            }

            if (tldRule == null)
            {
                return;
            }

            var domainParts = domain.Split('.').Reverse().ToList();
            var ruleParts = tldRule.Name.Split('.').Skip(tldRule.Type == TldRuleType.WildcardException ? 1 : 0).Reverse().ToList();
            var tld = string.Join(".", domainParts.Take(ruleParts.Count).Reverse());
            var registrableDomain = string.Join(".", domainParts.Take(ruleParts.Count + 1).Reverse());

            if (domain.Equals(tld))
            {
                return;
            }

            this.TldRule = tldRule;
            this.Hostname = domain;
            this.Tld = tld;
            this.RegistrableDomain = registrableDomain;

            this.Domain = domainParts.Skip(ruleParts.Count).FirstOrDefault();
            var subDomain = string.Join(".", domainParts.Skip(ruleParts.Count + 1).Reverse());
            this.SubDomain = string.IsNullOrEmpty(subDomain) ? null : subDomain;
        }
    }
}
