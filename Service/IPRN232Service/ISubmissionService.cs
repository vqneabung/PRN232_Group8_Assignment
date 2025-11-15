using Application.Enities;
using Application.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IPRN232Service
{
    public interface ISubmissionService
    {
        Task<object> HandleSubmissionAsync(FileUploadRequest request);
        Task<object?> GetSubmissionByIdAsync(int submissionId);
        Task<List<object>> GetSubmissionsByStudentIdAsync(int studentId);
        Task<List<object>> GetAllSubmissionsAsync();
        Task<bool> DeleteSubmissionAsync(int submissionId);
    }
}
