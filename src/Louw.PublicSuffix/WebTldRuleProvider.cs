using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Louw.PublicSuffix
{
    [Obsolete("Use WebTldRuleProvider instead")]
    public class CachedTldRuleProvider : WebTldRuleProvider
    {
        public CachedTldRuleProvider(string fileName = null, string fileUrl = null, TimeSpan? timeToLive = null) : base(fileName,fileUrl,timeToLive)
        {

        }
    }

    public class WebTldRuleProvider : ITldRuleProvider
    {
        private readonly string _fileName;
        private readonly string _fileUrl;
        private readonly TimeSpan _timeToLive;

        /// <summary>
        /// Constructs CachedTldRuleProvider class.
        /// </summary>
        /// <param name="fileName">Filename where data is cached. If empty, temporary filename generated.</param>
        /// <param name="fileUrl">URL where Publix Suffix rules can be downloaded. (Default: https://publicsuffix.org/list/public_suffix_list.dat)</param>
        /// <param name="timeToLive">TimeToLive - file data is refreshed from specified URL if file older than TTL. (Default: 1day)</param>
        public WebTldRuleProvider(string fileName = null, string fileUrl = null, TimeSpan? timeToLive = null)
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

        /// <summary>
        /// Load data from URL and store it at File cache location (overwrites existing cached file)
        /// </summary>
        /// <returns>Returns Task that can be awaited</returns>
        public async Task Refresh()
        {
            var ruleData = await FetchFromWeb(_fileUrl).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(ruleData))
                File.WriteAllText(_fileName, ruleData);
        }

        /// <summary>
        /// Check if cached file should be refreshed
        /// </summary>
        /// <returns>True if cache file does not exist, or if cached file is older than specified TimeToLive</returns>
        public bool MustRefresh()
        {
            bool mustRefresh = !File.Exists(_fileName)
                || (File.GetLastWriteTimeUtc(_fileName) < DateTime.UtcNow.Subtract(_timeToLive));
            return mustRefresh;
        }

        public async Task<IEnumerable<TldRule>> BuildAsync()
        {
            if (MustRefresh())
            {
                //TODO: Improvement - Continue even if refresh of file failed (if cached copy exists)
                await Refresh().ConfigureAwait(false);
            }

            var parser = new TldRuleParser();
            var ruleData = await FetchFromFile(_fileName).ConfigureAwait(false);

            var rules = parser.ParseRules(ruleData);
            return rules;
        }

        private async Task<string> FetchFromWeb(string url)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task<string> FetchFromFile(string fileName)
        {
            if (!File.Exists(_fileName))
                throw new FileNotFoundException("Rule file does not exist");

            using (var reader = File.OpenText(fileName))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }
    }
}
