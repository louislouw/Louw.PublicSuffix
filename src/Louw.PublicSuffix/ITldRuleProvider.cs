using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Louw.PublicSuffix
{
    public interface ITldRuleProvider
    {
        /// <summary>
        /// Builds the list of TldRules
        /// </summary>
        /// <returns>List of TldRules</returns>
        Task<IEnumerable<TldRule>> BuildAsync();
    }
}
