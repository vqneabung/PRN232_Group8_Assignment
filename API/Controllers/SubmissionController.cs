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
    }
}