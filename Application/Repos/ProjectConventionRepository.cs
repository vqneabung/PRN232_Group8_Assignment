using Application.Interface;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Repos
{
    public class ProjectConventionRepository : GenericRepository<ProjectConvention>, IProjectConventionRepository
    {
        private readonly AutoGraderDBContext _context;

        public ProjectConventionRepository(AutoGraderDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ProjectConvention?> GetCurrentConventionAsync()
        {
            return await _context.ProjectConventions
                .OrderByDescending(pc => pc.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ProjectConvention> UpdateConventionAsync(ProjectConvention convention)
        {
            var existing = await GetCurrentConventionAsync();
            if (existing != null)
            {
                existing.ExpectedSolutionPrefix = convention.ExpectedSolutionPrefix;
                existing.ExpectedSolutionSuffix = convention.ExpectedSolutionSuffix;
                existing.AdditionalRules = convention.AdditionalRules;
                existing.UpdatedAt = DateTime.Now;
                
                _context.ProjectConventions.Update(existing);
                return existing;
            }
            else
            {
                convention.CreatedAt = DateTime.Now;
                _context.ProjectConventions.Add(convention);
                return convention;
            }
        }
    }
}