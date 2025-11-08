using System;

namespace Application.Enities
{
    public class ProjectConventionResponse
    {
        public int Id { get; set; }
        public string ExpectedSolutionPrefix { get; set; } = string.Empty;
        public string ExpectedSolutionSuffix { get; set; } = string.Empty;
        public string? AdditionalRules { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}