using Application.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IPRN232Service
{
    public interface IPlagiarismService
    {
        Task<PlagiarismCheckResult> CheckPlagiarismAsync(string zipFilePath, string submissionId, double threshold = 0.85);
        Task<StoreSubmissionResult> StoreSubmissionAsync(string zipFilePath, string submissionId);
        Task<bool> IsServiceAvailableAsync();
    }
}
