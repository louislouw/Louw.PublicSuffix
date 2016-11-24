using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Louw.PublicSuffix
{
    public class CachedTldRuleProvider : ITldRuleProvider
    {
        private readonly string _fileName;
        private readonly string _fileUrl;
        private readonly TimeSpan _timeToLive;

        public CachedTldRuleProvider(string fileName = null, string fileUrl = null, TimeSpan? timeToLive = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = Path.Combine(Path.GetTempPath(), "public_suffix_list.dat");

            if (string.IsNullOrEmpty(fileUrl))
                fileUrl = @"https://publicsuffix.org/list/public_suffix_list.dat";

            if (!timeToLive.HasValue)
                timeToLive = TimeSpan.FromDays(1);

            _fileName = fileName;
            _fileUrl = fileUrl;
            _timeToLive = timeToLive.Value;
        }

        public async Task Refresh()
        {
            var ruleData = await FetchFromWeb(_fileUrl);
            if (!string.IsNullOrEmpty(ruleData))
                File.WriteAllText(_fileName, ruleData);
        }

        public async Task<IEnumerable<TldRule>> BuildAsync()
        {
            bool mustRefresh = !File.Exists(_fileName)
                || (File.GetLastWriteTimeUtc(_fileName) < DateTime.UtcNow.Subtract(_timeToLive));

            if (mustRefresh)
            {
                //TODO: Improvement - Continue even if refresh of file failed (if cached copy exists)
                await Refresh();
            }

            var parser = new TldRuleParser();
            var ruleData = await FetchFromFile(_fileName);

            var rules = parser.ParseRules(ruleData);
            return rules;
        }

        private async Task<string> FetchFromWeb(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
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
