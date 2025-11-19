using Application.Enities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _service;

        public SubmissionController(ISubmissionService service)
        {
            _service = service;
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadZip([FromForm] FileUploadRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _service.HandleSubmissionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllSubmissions()
        {
            try
            {
                var submissions = await _service.GetAllSubmissionsAsync();
                return Ok(submissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubmission(int id)
        {
            try
            {
                var submission = await _service.GetSubmissionByIdAsync(id);
                if (submission == null)
                {
                    return NotFound(new { message = "Submission not found" });
                }
                return Ok(submission);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("by-student/{studentId}")]
        public async Task<IActionResult> GetSubmissionsByStudent(int studentId)
        {
            try
            {
                var submissions = await _service.GetSubmissionsByStudentIdAsync(studentId);
                return Ok(submissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            try
            {
                var deleted = await _service.DeleteSubmissionAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Submission not found" });
                }
                return Ok(new { message = "Submission deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("statistics/student/{studentId}")]
        public async Task<IActionResult> GetStudentSubmissionStatistics(int studentId)
        {
            try
            {
                var submissions = await _service.GetSubmissionsByStudentIdAsync(studentId);
                
                var statistics = new
                {
                    TotalSubmissions = submissions.Count,
                    SubmissionsWithViolations = submissions.Count(s => 
                    {
                        dynamic submission = s;
                        return submission.HasViolations;
                    }),
                    SubmissionsWithoutViolations = submissions.Count(s => 
                    {
                        dynamic submission = s;
                        return !submission.HasViolations;
                    }),
                    MostRecentSubmission = submissions.FirstOrDefault()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all submissions from students in a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>List of submissions from all students in the class</returns>
        [HttpGet("by-class/{classId}")]
        public async Task<IActionResult> GetSubmissionsByClass(int classId)
        {
            try
            {
                var submissions = await _service.GetSubmissionsByClassIdAsync(classId);
                return Ok(submissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Process batch grading from a ZIP/RAR file containing multiple student submissions
        /// Expected structure: 
        /// - Filename contains class code in parentheses: (SE1751)
        /// - Inside: StudentFolders like "AnhNASE183208" 
        /// - Each folder contains: 0/solution.zip
        /// </summary>
        /// <param name="request">Batch grading request with archive file and rule IDs</param>
        /// <returns>Batch grading results with statistics and individual student results</returns>
        [HttpPost("batch-grading")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ProcessBatchGrading([FromForm] BatchGradingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate file
                if (request.ArchiveFile != null)
                {
                    var allowedExtensions = new[] { ".zip", ".rar", ".7z" };
                    var fileExtension = Path.GetExtension(request.ArchiveFile.FileName).ToLowerInvariant();
    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { error = "Only archive files (.zip, .rar, .7z) are allowed" });
                    }

                    // Check file size (max 500MB for batch)
                    if (request.ArchiveFile.Length > 500 * 1024 * 1024)
                    {
                        return BadRequest(new { error = "File size must be less than 500MB" });
                    }
                }

                var result = await _service.ProcessBatchGradingAsync(request);

                // Return appropriate response based on results
                if (result.FailedGradings == result.TotalStudentFolders && result.TotalStudentFolders > 0)
                {
                    return BadRequest(new
                    {
                        message = "Batch grading completed with all failures",
                        result
                    });
                }

                return Ok(new
                {
                    message = $"Batch grading completed. {result.SuccessfulGradings}/{result.TotalStudentFolders} students graded successfully.",
                    summary = new
                    {
                        result.ClassName,
                        result.Semester,
                        result.ClassId,
                        result.TotalStudentFolders,
                        result.SuccessfulGradings,
                        result.FailedGradings,
                        result.NewStudentsCreated,
                        result.ExistingStudentsFound,
                        result.ClassCreated,
                        result.ProcessedAt,
                        ErrorCount = result.Errors.Count
                    },
                    studentResults = result.StudentResults,
                    errors = result.Errors.Any() ? result.Errors : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}