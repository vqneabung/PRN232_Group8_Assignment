using Microsoft.AspNetCore.Http;

namespace Application.Enities
{
    public class PlagiarismStoreRequest
    {
        public IFormFile File { get; set; }
        public string SubmissionId { get; set; }
    }
}
