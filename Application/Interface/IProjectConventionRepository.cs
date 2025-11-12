using Application.Interface;
using Application.Models;

namespace Application.Interface
{
    public interface IProjectConventionRepository : IGenericRepository<ProjectConvention>
    {
        Task<ProjectConvention?> GetCurrentConventionAsync();
        Task<ProjectConvention> UpdateConventionAsync(ProjectConvention convention);
    }
}