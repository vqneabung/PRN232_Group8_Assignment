using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using AutoMapper;
using Service.IPRN232Service;

namespace Service.PRN232Service
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<StudentResponse>> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.StudentRepository.GetAllStudentsAsync();
            return _mapper.Map<List<StudentResponse>>(students);
        }

        public async Task<StudentResponse?> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
            return student == null ? null : _mapper.Map<StudentResponse>(student);
        }

        public async Task<StudentResponse?> GetStudentByCodeAsync(string studentCode)
        {
            var student = await _unitOfWork.StudentRepository.GetByStudentCodeAsync(studentCode);
            return student == null ? null : _mapper.Map<StudentResponse>(student);
        }

        public async Task<StudentResponse> CreateStudentAsync(StudentRequest request)
        {
            var student = _mapper.Map<Student>(request);
            await _unitOfWork.StudentRepository.AddAsync(student);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<StudentResponse>(student);
        }

        public async Task<StudentResponse?> UpdateStudentAsync(int id, StudentRequest request)
        {
            var existingStudent = await _unitOfWork.StudentRepository.GetByIdAsync(id);
            if (existingStudent == null)
            {
                return null;
            }

            // Map request data to existing student
            _mapper.Map(request, existingStudent);
            existingStudent.StudentId = id; // Ensure ID is preserved
            
            _unitOfWork.StudentRepository.Update(existingStudent);
            await _unitOfWork.SaveAsync();
            
            return _mapper.Map<StudentResponse>(existingStudent);
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return false;
            }

            _unitOfWork.StudentRepository.Delete(student);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}