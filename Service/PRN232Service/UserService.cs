using Application.Enities;
using Application.UnitOfWork;
using Service.IPRN232Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.PRN232Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserResponse>> GetAlls()
        {
            var users = await _unitOfWork.UserRepository.GetAllsWithRolesAsync();
            var userList = users.Select(user => new UserResponse
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                RoleName = user.Role.RoleName,
                UserName = user.UserName,
            }).ToList();

            return userList;
        }
    }
}
