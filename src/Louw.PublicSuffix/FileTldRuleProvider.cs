using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Louw.PublicSuffix
{
    public class FileTldRuleProvider : ITldRuleProvider
    {
        private readonly string _fileName;

        public FileTldRuleProvider(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("fileName may not be empty.", "fileName");

            _fileName = fileName;
        }

        public async Task<IEnumerable<TldRule>> BuildAsync()
        {
            var ruleData = await FetchFromFile(_fileName);

            var parser = new TldRuleParser();
            var rules = parser.ParseRules(ruleData);
            return rules;
        }

        private async Task<string> FetchFromFile(string fileName)
        {
            if (!File.Exists(_fileName))
                throw new FileNotFoundException("Rule file does not exist");

            using (var reader = File.OpenText(fileName))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
