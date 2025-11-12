using Application.Enities;

namespace Service.IPRN232Service
{
    public interface IProjectConventionService
    {
        Task<ProjectConventionResponse?> GetCurrentConventionAsync();
        Task<ProjectConventionResponse> UpdateConventionAsync(ProjectConventionRequest request);
        Task<SolutionValidationResponse> ValidateSolutionNamingAsync(SolutionValidationRequest request);
        Task<SolutionValidationResponse> ValidateSolutionNamingFromFileAsync(SolutionFileValidationRequest request);
    }
}