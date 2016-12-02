using System;
using System.Collections.Generic;
using System.Linq;

namespace Louw.PublicSuffix
{
    public class DomainParser
    {
        private readonly object _lockObject = new object();
        private DomainDataStructure _domainDataStructure = null;
        private readonly ITldRuleProvider _ruleProvider; 

        public DomainParser(IEnumerable<TldRule> rules)
        {
            if (rules == null)
                throw new ArgumentNullException("rules");
            
            this.AddRules(rules);
        }

        public DomainParser(ITldRuleProvider ruleProvider)
        {
            if (ruleProvider == null)
                throw new ArgumentNullException("ruleProvider");

            _ruleProvider = ruleProvider;
        }

        public DomainInfo Get(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return null;
            }

            if(_domainDataStructure==null)
            {
                //Gotta keep it thread safe (as this object is expected to be immutable)
                lock (_lockObject)
                {
                    if (_domainDataStructure == null)
                    {
                        BuildRules(); //Use ITldRuleProvider to build rules
                    }
                }
            }

            //We use Uri methods to normalize host (So Punycode is converted to UTF-8
            if (!domain.Contains("://")) domain = string.Concat("https://", domain);
            Uri uri;
            if (!Uri.TryCreate(domain, UriKind.RelativeOrAbsolute, out uri))
            {
                return null;
            }
            string normalizedDomain = uri.Host;
            string normalizedHost = uri.GetComponents(UriComponents.NormalizedHost, UriFormat.UriEscaped); //Normalize Punycode

            var parts = normalizedHost
                .Split('.')
                .Reverse()
                .ToList();

            if (parts.Count == 0 || parts.Any(x => x.Equals("")))
            {
                return null;
            }

            var structure = this._domainDataStructure;
            var matches = new List<TldRule>();
            FindMatches(parts, structure, matches);

            //Sort so exceptions are first, then by biggest label count (with wildcards at bottom) 
            var sortedMatches = matches.OrderByDescending(x => x.Type == TldRuleType.WildcardException?1:0)
                .ThenByDescending(x => x.LabelCount)
                .ThenByDescending(x => x.Name);

            var winningRule = sortedMatches.FirstOrDefault();

            if (winningRule == null)
            {
                winningRule = new TldRule("*");
            }

            //Domain is TLD
            if (parts.Count == winningRule.LabelCount)
            {
                return null;
            }

            var domainName = new DomainInfo(normalizedDomain, winningRule);
            return domainName;
        }

        private void FindMatches(IEnumerable<string> parts, DomainDataStructure structure, List<TldRule> matches)
        {
            if (structure.TldRule != null)
            {
                matches.Add(structure.TldRule);
            }

            var part = parts.FirstOrDefault();
            if (string.IsNullOrEmpty(part))
            {
                return;
            }

            DomainDataStructure foundStructure;
            if (structure.Nested.TryGetValue(part, out foundStructure))
            {
                FindMatches(parts.Skip(1), foundStructure, matches);
            }

            if (structure.Nested.TryGetValue("*", out foundStructure))
            {
                FindMatches(parts.Skip(1), foundStructure, matches);
            }
        }

        private void BuildRules()
        {
            System.Diagnostics.Debug.Assert(_ruleProvider != null);
            var rules = _ruleProvider.BuildAsync().Result;
            AddRules(rules);
        }

        private void AddRules(IEnumerable<TldRule> tldRules)
        {
            System.Diagnostics.Debug.Assert(_domainDataStructure == null); //We can only load rules once
            _domainDataStructure = new DomainDataStructure("*", new TldRule("*"));

            foreach (var tldRule in tldRules)
            {
                this.AddRule(tldRule);
            }
        }

        private void AddRule(TldRule tldRule)
        {
            var structure = this._domainDataStructure;
            var domainPart = string.Empty;

            var parts = tldRule.Name.Split('.').Reverse().ToList();
            for (var i = 0; i < parts.Count; i++)
            {
                domainPart = parts[i];

                if (parts.Count - 1 > i)
                {
                    //Check if domain exists
                    if (!structure.Nested.ContainsKey(domainPart))
                    {
                        structure.Nested.Add(domainPart, new DomainDataStructure(domainPart));
                    }

                    structure = structure.Nested[domainPart];
                    continue;
                }

                //Check if domain exists
                if (structure.Nested.ContainsKey(domainPart))
                {
                    structure.Nested[domainPart].TldRule = tldRule;
                    continue;
                }

                structure.Nested.Add(domainPart, new DomainDataStructure(domainPart, tldRule));
            }
        }
    }
}
