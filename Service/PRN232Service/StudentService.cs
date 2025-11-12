using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using AutoMapper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
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

        public async Task<int> ImportStudentsFromExcelAsync(ImportStudentsRequest request)
        {
            if (request.ExcelFile == null || request.ExcelFile.Length == 0)
            {
                throw new ArgumentException("Excel file is required");
            }

            // Validate file extension
            var fileExtension = Path.GetExtension(request.ExcelFile.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                throw new ArgumentException("Only Excel files (.xlsx, .xls) are supported");
            }

            var importedCount = 0;
            var errors = new List<string>();

            using var stream = new MemoryStream();
            await request.ExcelFile.CopyToAsync(stream);
            stream.Position = 0;

            IWorkbook workbook;
            try
            {
                // Try to create workbook based on file extension
                if (fileExtension == ".xlsx")
                {
                    workbook = new XSSFWorkbook(stream);
                }
                else
                {
                    workbook = new HSSFWorkbook(stream);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid Excel file format: {ex.Message}");
            }

            using (workbook)
            {
                ISheet worksheet = workbook.GetSheetAt(0);
                if (worksheet == null)
                {
                    throw new ArgumentException("No worksheet found in the Excel file");
                }

                var rowCount = worksheet.LastRowNum + 1;
                if (rowCount < 2) // At least header + 1 data row
                {
                    throw new ArgumentException("Excel file must contain at least one data row besides the header");
                }

                // Get all existing students to check for duplicates
                var existingStudents = await _unitOfWork.StudentRepository.GetAllStudentsAsync();
                var existingStudentCodes = existingStudents.ToDictionary(s => s.StudentCode.ToUpper(), s => s);

                var studentsToAdd = new List<Student>();
                var studentsToUpdate = new List<Student>();

                // Process rows (starting from row 1, assuming row 0 is header)
                for (int rowIndex = 1; rowIndex < rowCount; rowIndex++)
                {
                    IRow row = worksheet.GetRow(rowIndex);
                    if (row == null) continue;

                    try
                    {
                        var studentCode = GetCellStringValue(row.GetCell(0))?.Trim();
                        var fullName = GetCellStringValue(row.GetCell(1))?.Trim();
                        var email = GetCellStringValue(row.GetCell(2))?.Trim();

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(studentCode))
                        {
                            errors.Add($"Row {rowIndex + 1}: Student Code is required");
                            continue;
                        }

                        if (studentCode.Length > 20)
                        {
                            errors.Add($"Row {rowIndex + 1}: Student Code must be 20 characters or less");
                            continue;
                        }

                        // Validate email format if provided
                        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
                        {
                            errors.Add($"Row {rowIndex + 1}: Invalid email format");
                            continue;
                        }

                        // Validate field lengths
                        if (!string.IsNullOrWhiteSpace(fullName) && fullName.Length > 255)
                        {
                            fullName = fullName.Substring(0, 255);
                        }

                        if (!string.IsNullOrWhiteSpace(email) && email.Length > 255)
                        {
                            errors.Add($"Row {rowIndex + 1}: Email must be 255 characters or less");
                            continue;
                        }

                        var upperStudentCode = studentCode.ToUpper();
                        var existingStudent = existingStudentCodes.ContainsKey(upperStudentCode) 
                            ? existingStudentCodes[upperStudentCode] 
                            : null;

                        if (existingStudent != null)
                        {
                            if (request.SkipDuplicates && !request.UpdateExisting)
                            {
                                continue; // Skip this student
                            }
                            else if (request.UpdateExisting)
                            {
                                // Update existing student
                                existingStudent.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName;
                                existingStudent.Email = string.IsNullOrWhiteSpace(email) ? null : email;
                                studentsToUpdate.Add(existingStudent);
                            }
                        }
                        else
                        {
                            // Create new student
                            var newStudent = new Student
                            {
                                StudentCode = studentCode,
                                FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                                Email = string.IsNullOrWhiteSpace(email) ? null : email
                            };
                            studentsToAdd.Add(newStudent);
                            existingStudentCodes[upperStudentCode] = newStudent; // Prevent duplicates within the same import
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {rowIndex + 1}: {ex.Message}");
                    }
                }

                // Throw exception if there are validation errors
                if (errors.Any())
                {
                    throw new ValidationException($"Import failed with errors: {string.Join("; ", errors)}");
                }

                // Save to database
                foreach (var student in studentsToAdd)
                {
                    await _unitOfWork.StudentRepository.AddAsync(student);
                    importedCount++;
                }

                foreach (var student in studentsToUpdate)
                {
                    _unitOfWork.StudentRepository.Update(student);
                    importedCount++;
                }

                await _unitOfWork.SaveAsync();
                return importedCount;
            }
        }

        private static string? GetCellStringValue(ICell? cell)
        {
            if (cell == null) return null;

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Formula => cell.StringCellValue,
                _ => cell.ToString()
            };
        }

        private static bool IsValidEmail(string email)
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
    }
}