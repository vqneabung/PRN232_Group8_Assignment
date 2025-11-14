using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.Enities;
using Microsoft.AspNetCore.Authorization;
using Service.IPRN232Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _accountService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _accountService.RegisterAsync(request);
                return CreatedAtAction(nameof(GetCurrentUser), null, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a username is available
        /// </summary>
        /// <param name="userName">Username to check</param>
        /// <returns>True if username is available, false if taken</returns>
        [HttpGet("check-username/{userName}")]
        public async Task<IActionResult> CheckUsername(string userName)
        {
            try
            {
                var exists = await _accountService.UserExistsAsync(userName);
                return Ok(new { available = !exists, message = exists ? "Username is already taken" : "Username is available" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { error = "Invalid token claims" });
                }

                return Ok(new
                {
                    userId = int.Parse(userId),
                    userName,
                    role = userRole,
                    isAuthenticated = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
