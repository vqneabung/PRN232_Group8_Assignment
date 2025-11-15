using Application.Models;

namespace Application.Interface
{
    public interface IClassRepository : IGenericRepository<Class>
    {
        Task<IEnumerable<Class>> GetClassesWithDetailsAsync();
        Task<Class?> GetClassWithDetailsAsync(int classId);
        Task<IEnumerable<Class>> GetClassesByLecturerAsync(int lecturerId);
        Task<IEnumerable<Class>> GetClassesByExaminerAsync(int examinerId);
        Task<IEnumerable<Class>> GetClassesBySemesterAsync(string semester);
        Task<bool> ClassExistsAsync(string className, string semester);
        Task<IEnumerable<Student>> GetStudentsInClassAsync(int classId);
        Task AddStudentToClassAsync(int classId, int studentId);
        Task RemoveStudentFromClassAsync(int classId, int studentId);
    }
}