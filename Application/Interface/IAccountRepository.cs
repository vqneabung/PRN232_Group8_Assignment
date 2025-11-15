using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interface
{
    public interface IAccountRepository : IGenericRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<bool> UserExistsAsync(string userName);
        Task<Role?> GetRoleByIdAsync(int roleId);
    }
}
