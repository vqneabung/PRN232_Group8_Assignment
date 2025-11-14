using Application.Interface;
using Application.Models;
using Application.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AutoGraderDBContext _context;

        // 🔹 Repositories
        public IRuleRepository RuleRepository { get; }
        public IGenericRepository<Submission> Submissions { get; }
        public IGenericRepository<Violation> Violations { get; }
        public IStudentRepository StudentRepository { get; }
        public IAccountRepository AccountRepository { get; }

        public UnitOfWork(AutoGraderDBContext context)
        {
            _context = context;

            // 🔹 Khởi tạo tất cả repos ở đây để dùng chung cùng DbContext
            RuleRepository = new RuleRepository(_context);
            Submissions = new GenericRepository<Submission>(_context);
            Violations = new GenericRepository<Violation>(_context);
            StudentRepository = new StudentRepository(_context);
            AccountRepository = new AccountRepository(_context);
        }

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}