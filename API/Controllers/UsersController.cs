using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAlls();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("lecturers")]
        public async Task<IActionResult> GetAllLecturers()
        {
            try
            {
                var users = await _userService.GetAlls();
                var lecturers = users.Where(u => u.RoleId == 4);
                return Ok(lecturers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }

        [HttpGet("lecturers/{id}")]
        public async Task<IActionResult> GetLecturerById(int id)
        {
            try
            {
                var users = await _userService.GetAlls();
                var lecturers = users.Where(u => u.RoleId == 4 && u.UserId == id);
                return Ok(lecturers.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }
    }
}
