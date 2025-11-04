using Application.Interface;
using Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repos
{
    public class RuleRepository : GenericRepository<Rule>, IRuleRepository
    {
        private readonly AutoGraderDBContext _context;

        public RuleRepository(AutoGraderDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Rule>> GetRulesByIdsAsync(List<int> ruleIds)
        {
            return await _context.Rules
                .Where(r => ruleIds.Contains(r.RuleId))
                .ToListAsync();
        }
    }
}
