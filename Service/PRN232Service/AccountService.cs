using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using Service.IPRN232Service;
using System.Security.Cryptography;
using System.Text;

namespace Service.PRN232Service
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AccountService(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.AccountRepository.GetByUserNameAsync(request.UserName);
            
            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            var token = _tokenService.GenerateAccessToken(user);
            
            return new AuthResponse
            {
                Token = token,
                UserName = user.UserName,
                Role = user.Role.RoleName,
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _unitOfWork.AccountRepository.UserExistsAsync(request.UserName))
            {
                throw new InvalidOperationException("Username already exists");
            }

            var role = await _unitOfWork.AccountRepository.GetRoleByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new InvalidOperationException("Invalid role");
            }

            var hashedPassword = HashPassword(request.Password);

            var user = new User
            {
                UserName = request.UserName,
                Password = hashedPassword,
                RoleId = request.RoleId,
                Role = role
            };

            await _unitOfWork.AccountRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();

            var token = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                Token = token,
                UserName = user.UserName,
                Role = user.Role.RoleName,
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _unitOfWork.AccountRepository.UserExistsAsync(userName);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}
