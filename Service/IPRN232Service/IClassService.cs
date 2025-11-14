using Application.Enities;

namespace Service.IPRN232Service
{
    public interface IClassService
    {
        Task<List<ClassResponse>> GetAllClassesAsync();
        Task<ClassResponse?> GetClassByIdAsync(int classId);
        Task<List<ClassResponse>> GetClassesByLecturerAsync(int lecturerId);
        Task<List<ClassResponse>> GetClassesByExaminerAsync(int examinerId);
        Task<List<ClassResponse>> GetClassesBySemesterAsync(string semester);
        Task<ClassResponse> CreateClassAsync(ClassRequest request);
        Task<ClassResponse?> UpdateClassAsync(int classId, ClassRequest request);
        Task<bool> DeleteClassAsync(int classId);
        Task<List<StudentResponse>> GetStudentsInClassAsync(int classId);
        Task<bool> AddStudentToClassAsync(int classId, int studentId);
        Task<bool> RemoveStudentFromClassAsync(int classId, int studentId);
        Task<bool> ClassExistsAsync(string className, string semester);
    }
}