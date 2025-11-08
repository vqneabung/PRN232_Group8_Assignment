using Application.Interface;
using Application.Models;

namespace Application.Interface
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student?> GetByStudentCodeAsync(string studentCode);
        Task<List<Student>> GetAllStudentsAsync();
    }
}