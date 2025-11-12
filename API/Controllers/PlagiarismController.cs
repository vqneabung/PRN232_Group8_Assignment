using Application.Enities;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlagiarismController : ControllerBase
    {
        private readonly IPlagiarismService _plagiarismService;

        public PlagiarismController(IPlagiarismService plagiarismService)
        {
            _plagiarismService = plagiarismService;
        }

        [HttpGet("health")]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var isAvailable = await _plagiarismService.IsServiceAvailableAsync();
                return Ok(new
                {
                    available = isAvailable,
                    message = isAvailable ? "Plagiarism service is running" : "Plagiarism service is not available"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("check")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CheckPlagiarism([FromForm] IFormFile file, [FromForm] string submissionId, [FromForm] double threshold = 0.85)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "File không hợp lệ" });

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
                
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = await _plagiarismService.CheckPlagiarismAsync(tempPath, submissionId, threshold);

                try { System.IO.File.Delete(tempPath); } catch { }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("store")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> StoreSubmission([FromForm] IFormFile file, [FromForm] string submissionId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "File không hợp lệ" });

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
                
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = await _plagiarismService.StoreSubmissionAsync(tempPath, submissionId);

                try { System.IO.File.Delete(tempPath); } catch { }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
