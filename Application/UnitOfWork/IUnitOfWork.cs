using Application.Interface;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Submission> Submissions { get; }
        IGenericRepository<Violation> Violations { get; }

        IRuleRepository RuleRepository { get; }
        IStudentRepository StudentRepository { get; }

        IAccountRepository AccountRepository { get; }

        Task<int> SaveAsync();
    }
}
