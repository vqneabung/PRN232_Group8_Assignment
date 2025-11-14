using Application.Interface;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Repos
{
    internal class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly AutoGraderDBContext _context;

        public ClassRepository(AutoGraderDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Class>> GetClassesWithDetailsAsync()
        {
            return await _context.Classes
                .Include(c => c.LecturerNavigation)
                .Include(c => c.ExaminerNavigation)
                .Include(c => c.Students)
                .ToListAsync();
        }

        public async Task<Class?> GetClassWithDetailsAsync(int classId)
        {
            return await _context.Classes
                .Include(c => c.LecturerNavigation)
                .Include(c => c.ExaminerNavigation)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == classId);
        }

        public async Task<IEnumerable<Class>> GetClassesByLecturerAsync(int lecturerId)
        {
            return await _context.Classes
                .Include(c => c.LecturerNavigation)
                .Include(c => c.ExaminerNavigation)
                .Include(c => c.Students)
                .Where(c => c.Lecturer == lecturerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetClassesByExaminerAsync(int examinerId)
        {
            return await _context.Classes
                .Include(c => c.LecturerNavigation)
                .Include(c => c.ExaminerNavigation)
                .Include(c => c.Students)
                .Where(c => c.Examiner == examinerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetClassesBySemesterAsync(string semester)
        {
            return await _context.Classes
                .Include(c => c.LecturerNavigation)
                .Include(c => c.ExaminerNavigation)
                .Include(c => c.Students)
                .Where(c => c.Semester == semester)
                .ToListAsync();
        }

        public async Task<bool> ClassExistsAsync(string className, string semester)
        {
            return await _context.Classes
                .AnyAsync(c => c.ClassName == className && c.Semester == semester);
        }

        public async Task<IEnumerable<Student>> GetStudentsInClassAsync(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            return classEntity?.Students ?? new List<Student>();
        }

        public async Task AddStudentToClassAsync(int classId, int studentId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var student = await _context.Students.FindAsync(studentId);

            if (classEntity != null && student != null && !classEntity.Students.Contains(student))
            {
                classEntity.Students.Add(student);
            }
        }

        public async Task RemoveStudentFromClassAsync(int classId, int studentId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var student = classEntity?.Students.FirstOrDefault(s => s.StudentId == studentId);

            if (classEntity != null && student != null)
            {
                classEntity.Students.Remove(student);
            }
        }
    }
}