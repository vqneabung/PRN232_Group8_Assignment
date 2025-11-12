using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enities
{
    public class PlagiarismCheckResult
    {
        public bool IsPlagiarized { get; set; }
        public double SimilarityScore { get; set; }
        public string? MatchedSubmissionId { get; set; }
        public List<MatchedFileDetail>? MatchedFiles { get; set; }
        public int TotalFilesChecked { get; set; }
        public string? Message { get; set; }
    }

    public class MatchedFileDetail
    {
        public string CurrentFile { get; set; } = string.Empty;
        public string MatchedFile { get; set; } = string.Empty;
        public double Similarity { get; set; }
        public string MatchedSubmissionId { get; set; } = string.Empty;
    }
}
