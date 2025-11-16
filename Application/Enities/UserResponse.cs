using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enities
{
    public class UserResponse
    {
        public int UserId { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public string UserName { get; set; } = null!;

    }
}
