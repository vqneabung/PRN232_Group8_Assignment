using Microsoft.AspNetCore.Http;

namespace Application.Enities
{
    public class PlagiarismCheckRequest
    {
        public IFormFile File { get; set; }
        public string SubmissionId { get; set; }
        public double Threshold { get; set; } = 0.85;
    }
}
