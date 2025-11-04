using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IRuleRepository : IGenericRepository<Rule>
    {
        Task<IEnumerable<Rule>> GetRulesByIdsAsync(List<int> ruleIds);
    }

}
    