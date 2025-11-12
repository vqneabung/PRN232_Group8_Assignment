using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class SolutionFileValidationRequest
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public IFormFile SubmissionFile { get; set; } = null!;
    }
}