using Application.Enities;
using Application.Helper;
using Service.IPRN232Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.PRN232Service
{
    public class PlagiarismService : IPlagiarismService
    {
        private readonly PlagiarismCheckClient _client;

        public PlagiarismService(PlagiarismCheckClient client)
        {
            _client = client;
        }

        public async Task<PlagiarismCheckResult> CheckPlagiarismAsync(string zipFilePath, string submissionId, double threshold = 0.85)
        {
            return await _client.CheckPlagiarismAsync(zipFilePath, submissionId, threshold);
        }

        public async Task<StoreSubmissionResult> StoreSubmissionAsync(string zipFilePath, string submissionId)
        {
            return await _client.StoreSubmissionAsync(zipFilePath, submissionId);
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            return await _client.IsServiceHealthyAsync();
        }
    }
}
