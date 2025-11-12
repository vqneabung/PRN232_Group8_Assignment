using Application.Interface;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Repos
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly AutoGraderDBContext _context;

        public StudentRepository(AutoGraderDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Student?> GetByStudentCodeAsync(string studentCode)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.StudentCode == studentCode);
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }
    }
}