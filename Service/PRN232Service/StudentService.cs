using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using AutoMapper;
using OfficeOpenXml;
using Service.IPRN232Service;
using System.ComponentModel.DataAnnotations;

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

        public async Task<StudentImportResult> ImportStudentsFromExcelAsync(StudentImportRequest request)
        {
            var result = new StudentImportResult();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (request.ExcelFile == null || request.ExcelFile.Length == 0)
            {
                result.Errors.Add("Excel file is required");
                return result;
            }

            try
            {
                using var stream = new MemoryStream();
                await request.ExcelFile.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                
                if (package.Workbook.Worksheets.Count == 0)
                {
                    result.Errors.Add("Excel file contains no worksheets");
                    return result;
                }

                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                {
                    result.Errors.Add("Excel file must contain at least a header row and one data row");
                    return result;
                }

                result.TotalRows = rowCount - 1; // Exclude header row

                // Validate header row
                var expectedHeaders = new[] { "Student Code", "Class", "Full Name", "Email" };
                for (int col = 1; col <= 4; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                    if (!string.Equals(headerValue, expectedHeaders[col - 1], StringComparison.OrdinalIgnoreCase))
                    {
                        result.Errors.Add($"Invalid header at column {col}. Expected '{expectedHeaders[col - 1]}', found '{headerValue}'");
                    }
                }

                if (result.Errors.Any())
                {
                    return result;
                }

                // Process data rows
                var classCache = new Dictionary<string, Class>();
                var studentCache = new Dictionary<string, Student>();

                // Load existing data
                var existingStudents = await _unitOfWork.StudentRepository.GetAllStudentsAsync();
                foreach (var student in existingStudents)
                {
                    studentCache[student.StudentCode] = student;
                }

                var existingClasses = await _unitOfWork.ClassRepository.GetClassesWithDetailsAsync();
                foreach (var cls in existingClasses)
                {
                    var key = $"{cls.ClassName}_{cls.Semester}";
                    classCache[key] = cls;
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var studentCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var className = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var fullName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var email = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                        // Validate required fields
                        if (string.IsNullOrEmpty(studentCode))
                        {
                            result.Errors.Add($"Row {row}: Student Code is required");
                            continue;
                        }

                        if (string.IsNullOrEmpty(className))
                        {
                            result.Errors.Add($"Row {row}: Class is required");
                            continue;
                        }

                        // Validate email format if provided
                        if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                        {
                            result.Errors.Add($"Row {row}: Invalid email format '{email}'");
                            continue;
                        }

                        // Process student
                        Student student;
                        bool isNewStudent = false;
                        bool isUpdated = false;

                        if (studentCache.ContainsKey(studentCode))
                        {
                            // Update existing student
                            student = studentCache[studentCode];
                            var originalFullName = student.FullName;
                            var originalEmail = student.Email;

                            student.FullName = !string.IsNullOrEmpty(fullName) ? fullName : student.FullName;
                            student.Email = !string.IsNullOrEmpty(email) ? email : student.Email;

                            if (originalFullName != student.FullName || originalEmail != student.Email)
                            {
                                _unitOfWork.StudentRepository.Update(student);
                                isUpdated = true;
                                result.UpdatedStudents++;
                            }
                        }
                        else
                        {
                            // Create new student
                            student = new Student
                            {
                                StudentCode = studentCode,
                                FullName = fullName,
                                Email = email
                            };
                            await _unitOfWork.StudentRepository.AddAsync(student);
                            studentCache[studentCode] = student;
                            isNewStudent = true;
                            result.NewStudents++;
                        }

                        // Process class
                        var classKey = $"{className}_{request.DefaultSemester}";
                        Class classEntity;
                        bool isNewClass = false;

                        if (classCache.ContainsKey(classKey))
                        {
                            classEntity = classCache[classKey];
                            result.ExistingClasses++;
                        }
                        else
                        {
                            classEntity = new Class
                            {
                                ClassName = className,
                                Semester = request.DefaultSemester
                            };
                            await _unitOfWork.ClassRepository.AddAsync(classEntity);
                            classCache[classKey] = classEntity;
                            isNewClass = true;
                            result.NewClasses++;

                            result.ImportedClasses.Add(new ImportedClassInfo
                            {
                                ClassName = className,
                                Semester = request.DefaultSemester,
                                Action = "Created",
                                StudentsCount = 1
                            });
                        }

                        // Add student to class if not already enrolled
                        if (!classEntity.Students.Any(s => s.StudentCode == studentCode))
                        {
                            await _unitOfWork.ClassRepository.AddStudentToClassAsync(classEntity.ClassId, student.StudentId);
                        }

                        // Track imported student
                        var importedStudent = result.ImportedStudents.FirstOrDefault(s => s.StudentCode == studentCode);
                        if (importedStudent == null)
                        {
                            importedStudent = new ImportedStudentInfo
                            {
                                StudentCode = studentCode,
                                FullName = student.FullName,
                                Email = student.Email,
                                Action = isNewStudent ? "Created" : (isUpdated ? "Updated" : "Existing")
                            };
                            result.ImportedStudents.Add(importedStudent);
                        }

                        if (!importedStudent.AssignedClasses.Contains(className))
                        {
                            importedStudent.AssignedClasses.Add(className);
                        }

                        // Update class student count
                        var importedClass = result.ImportedClasses.FirstOrDefault(c => c.ClassName == className);
                        if (importedClass != null)
                        {
                            importedClass.StudentsCount++;
                        }
                        else if (!isNewClass)
                        {
                            result.ImportedClasses.Add(new ImportedClassInfo
                            {
                                ClassName = className,
                                Semester = request.DefaultSemester,
                                Action = "Existing",
                                StudentsCount = 1
                            });
                        }

                        result.SuccessfulImports++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {row}: {ex.Message}");
                    }
                }

                // Save all changes
                await _unitOfWork.SaveAsync();
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to process Excel file: {ex.Message}");
                return result;
            }
        }

        private bool IsValidEmail(string email)
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
    }
}