using Application.Enities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IPRN232Service
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAlls();

    }
}
