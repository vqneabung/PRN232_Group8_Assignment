using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using AutoMapper;
using Service.IPRN232Service;

namespace Service.PRN232Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClassResponse>> GetAllClassesAsync()
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesWithDetailsAsync();
            return _mapper.Map<List<ClassResponse>>(classes);
        }

        public async Task<ClassResponse?> GetClassByIdAsync(int classId)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetClassWithDetailsAsync(classId);
            return classEntity == null ? null : _mapper.Map<ClassResponse>(classEntity);
        }

        public async Task<List<ClassResponse>> GetClassesByLecturerAsync(int lecturerId)
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesByLecturerAsync(lecturerId);
            return _mapper.Map<List<ClassResponse>>(classes);
        }

        public async Task<List<ClassResponse>> GetClassesByExaminerAsync(int examinerId)
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesByExaminerAsync(examinerId);
            return _mapper.Map<List<ClassResponse>>(classes);
        }

        public async Task<List<ClassResponse>> GetClassesBySemesterAsync(string semester)
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesBySemesterAsync(semester);
            return _mapper.Map<List<ClassResponse>>(classes);
        }

        public async Task<ClassResponse> CreateClassAsync(ClassRequest request)
        {
            // Check if class already exists
            if (await _unitOfWork.ClassRepository.ClassExistsAsync(request.ClassName, request.Semester))
            {
                throw new InvalidOperationException($"Class '{request.ClassName}' already exists for semester '{request.Semester}'");
            }

            var classEntity = _mapper.Map<Class>(request);
            await _unitOfWork.ClassRepository.AddAsync(classEntity);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ClassResponse>(classEntity);
        }

        public async Task<ClassResponse?> UpdateClassAsync(int classId, ClassRequest request)
        {
            var existingClass = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (existingClass == null)
            {
                return null;
            }

            // Check if another class with same name and semester exists (excluding current class)
            var duplicateExists = await _unitOfWork.ClassRepository.ClassExistsAsync(request.ClassName, request.Semester);
            if (duplicateExists && (existingClass.ClassName != request.ClassName || existingClass.Semester != request.Semester))
            {
                throw new InvalidOperationException($"Class '{request.ClassName}' already exists for semester '{request.Semester}'");
            }

            _mapper.Map(request, existingClass);
            existingClass.ClassId = classId; // Ensure ID is preserved

            _unitOfWork.ClassRepository.Update(existingClass);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ClassResponse>(existingClass);
        }

        public async Task<bool> DeleteClassAsync(int classId)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classEntity == null)
            {
                return false;
            }

            _unitOfWork.ClassRepository.Delete(classEntity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<List<StudentResponse>> GetStudentsInClassAsync(int classId)
        {
            var students = await _unitOfWork.ClassRepository.GetStudentsInClassAsync(classId);
            return _mapper.Map<List<StudentResponse>>(students);
        }

        public async Task<bool> AddStudentToClassAsync(int classId, int studentId)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);

            if (classEntity == null || student == null)
            {
                return false;
            }

            await _unitOfWork.ClassRepository.AddStudentToClassAsync(classId, studentId);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int classId, int studentId)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classEntity == null)
            {
                return false;
            }

            await _unitOfWork.ClassRepository.RemoveStudentFromClassAsync(classId, studentId);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> ClassExistsAsync(string className, string semester)
        {
            return await _unitOfWork.ClassRepository.ClassExistsAsync(className, semester);
        }
    }
}