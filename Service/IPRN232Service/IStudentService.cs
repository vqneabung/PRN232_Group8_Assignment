using Application.Enities;

namespace Service.IPRN232Service
{
    public interface IStudentService
    {
        Task<List<StudentResponse>> GetAllStudentsAsync();
        Task<StudentResponse?> GetStudentByIdAsync(int id);
        Task<StudentResponse?> GetStudentByCodeAsync(string studentCode);
        Task<StudentResponse> CreateStudentAsync(StudentRequest request);
        Task<StudentResponse?> UpdateStudentAsync(int id, StudentRequest request);
        Task<bool> DeleteStudentAsync(int id);
        Task<int> ImportStudentsFromExcelAsync(ImportStudentsRequest request);
    }
}