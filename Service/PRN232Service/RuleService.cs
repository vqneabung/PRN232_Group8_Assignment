using Application.Enities;
using Application.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Service.IPRN232Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.PRN232Service
{
    public class RuleService : IRuleService
    {
        private readonly AutoGraderDBContext _context;
        private readonly IMapper _mapper;

        public RuleService(AutoGraderDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RuleResponse>> GetAllAsync()
        {
            var rules = await _context.Rules.ToListAsync();
            return _mapper.Map<List<RuleResponse>>(rules);
        }

        public async Task<RuleResponse?> GetByIdAsync(int id)
        {
            var rule = await _context.Rules.FindAsync(id);
            return rule == null ? null : _mapper.Map<RuleResponse>(rule);
        }

        public async Task<RuleResponse> CreateAsync(RuleRequest request)
        {
            var rule = _mapper.Map<Rule>(request);
            _context.Rules.Add(rule);
            await _context.SaveChangesAsync();
            return _mapper.Map<RuleResponse>(rule);
        }

        public async Task<RuleResponse?> UpdateAsync(int id, RuleRequest request)
        {
            var existing = await _context.Rules.FindAsync(id);
            if (existing == null) return null;

            _mapper.Map(request, existing);
            await _context.SaveChangesAsync();

            return _mapper.Map<RuleResponse>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rule = await _context.Rules.FindAsync(id);
            if (rule == null) return false;

            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
