using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class ProjectConventionRequest
    {
        [Required]
        [MaxLength(100)]
        public string ExpectedSolutionPrefix { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ExpectedSolutionSuffix { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? AdditionalRules { get; set; }
    }
}