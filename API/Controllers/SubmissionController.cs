using Application.Enities;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _service;
        private readonly IProjectConventionService _conventionService;

        public SubmissionController(ISubmissionService service, IProjectConventionService conventionService)
        {
            _service = service;
            _conventionService = conventionService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadZip([FromForm] FileUploadRequest request)
        {
            try
            {
                var result = await _service.HandleSubmissionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("validate")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ValidateSolutionNaming([FromForm] SolutionFileValidationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _conventionService.ValidateSolutionNamingFromFileAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}