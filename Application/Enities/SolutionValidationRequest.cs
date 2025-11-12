using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class SolutionValidationRequest
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public string SubmissionPath { get; set; } = string.Empty;
    }
}