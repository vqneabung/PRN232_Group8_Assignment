using Application.Enities;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IPRN232Service
{
    public interface IRuleService
    {
        Task<List<RuleResponse>> GetAllAsync();
        Task<RuleResponse?> GetByIdAsync(int id);

        Task<RuleResponse> CreateAsync(RuleRequest request);

        Task<RuleResponse?> UpdateAsync(int id, RuleRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
