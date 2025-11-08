using Application.Enities;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectConventionController : ControllerBase
    {
        private readonly IProjectConventionService _service;

        public ProjectConventionController(IProjectConventionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentConvention()
        {
            try
            {
                var convention = await _service.GetCurrentConventionAsync();
                if (convention == null)
                {
                    return NotFound(new { message = "No project convention configured" });
                }
                return Ok(convention);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateConvention([FromBody] ProjectConventionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _service.UpdateConventionAsync(request);
                return Ok(new 
                { 
                    message = "Project convention updated successfully",
                    convention = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}