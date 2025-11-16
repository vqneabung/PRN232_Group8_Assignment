using Application.Interface;
using Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repos
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly AutoGraderDBContext _context;

        public UserRepository(AutoGraderDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllsWithRolesAsync()
        {
            return await _context.Users.Include(u => u.Role).ToListAsync();
        }
    }
}
