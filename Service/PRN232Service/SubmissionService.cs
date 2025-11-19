using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Service.IPRN232Service;
using System.IO.Compression;
using System.Text.RegularExpressions;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Service.PRN232Service
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubmissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<object> HandleSubmissionAsync(FileUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                throw new Exception("File không hợp lệ");

            // Validate student exists
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
                throw new Exception("Student không tồn tại");

            var ruleIds = (request.RuleIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var val) ? val : 0)
                .Where(id => id > 0)
                .ToList();

            if (!ruleIds.Any())
                throw new Exception("Vui lòng chọn ít nhất một rule để kiểm tra");

            var selectedRules = await _unitOfWork.RuleRepository.GetRulesByIdsAsync(ruleIds);

            var tempFolder = Path.Combine(Path.GetTempPath(), "Submission_" + Guid.NewGuid());
            Directory.CreateDirectory(tempFolder);

            string zipPath = Path.Combine(tempFolder, request.File.FileName);
            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var submission = new Submission
            {
                ZipFileName = request.File.FileName,
                UploadedAt = DateTime.Now,
                CheckedAt = DateTime.Now,
                StudentId = request.StudentId
            };
            await _unitOfWork.Submissions.AddAsync(submission);
            await _unitOfWork.SaveAsync();

            string extractPath = Path.Combine(tempFolder, "Extracted");
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            var violations = new List<Violation>();
            var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string content = "";

                try { content = await File.ReadAllTextAsync(filePath); } catch { }

                foreach (var rule in selectedRules)
                {
                    bool match = Regex.IsMatch(fileName, rule.Pattern, RegexOptions.IgnoreCase)
                        || (!string.IsNullOrEmpty(content) && Regex.IsMatch(content, rule.Pattern, RegexOptions.IgnoreCase));

                    if (match)
                    {
                        violations.Add(new Violation
                        {
                            SubmissionId = submission.SubmissionId,
                            RuleId = rule.RuleId,
                            FilePath = filePath.Replace(extractPath, "").TrimStart('\\', '/'),
                            Message = rule.Description
                        });
                    }
                }
            }

            foreach (var v in violations)
                await _unitOfWork.Violations.AddAsync(v);
            await _unitOfWork.SaveAsync();

            submission.Violations = violations;
            try { Directory.Delete(tempFolder, true); } catch { }

            return new
            {
                message = "File được xử lý thành công",
                submissionId = submission.SubmissionId,
                submission.ZipFileName,
                submission.UploadedAt,
                submission.CheckedAt,
                StudentId = submission.StudentId,
                StudentInfo = new
                {
                    student.StudentId,
                    student.StudentCode,
                    student.FullName,
                    student.Email
                },
                ViolationCount = violations.Count,
                Violations = violations.Select(v => new
                {
                    v.FilePath,
                    v.Message,
                    Rule = selectedRules.FirstOrDefault(r => r.RuleId == v.RuleId)
                })
            };
        }

        public async Task<object?> GetSubmissionByIdAsync(int submissionId)
        {
            var submissions = await _unitOfWork.Submissions.GetAllAsync();
            var submission = submissions.Cast<Submission>()
                .FirstOrDefault(s => s.SubmissionId == submissionId);

            if (submission == null)
                return null;

            // Load related data
            var student = submission.StudentId.HasValue
                ? await _unitOfWork.StudentRepository.GetByIdAsync(submission.StudentId.Value)
                : null;

            var violations = await _unitOfWork.Violations.GetAllAsync();
            var submissionViolations = violations.Cast<Violation>()
                .Where(v => v.SubmissionId == submissionId)
                .ToList();

            return new
            {
                submission.SubmissionId,
                submission.ZipFileName,
                submission.UploadedAt,
                submission.CheckedAt,
                StudentId = submission.StudentId,
                StudentInfo = student != null ? new
                {
                    student.StudentId,
                    student.StudentCode,
                    student.FullName,
                    student.Email
                } : null,
                ViolationCount = submissionViolations.Count,
                Violations = submissionViolations.Select(v => new
                {
                    v.FilePath,
                    v.Message,
                    v.RuleId
                })
            };
        }

        public async Task<List<object>> GetSubmissionsByStudentIdAsync(int studentId)
        {
            var submissions = await _unitOfWork.Submissions.GetAllAsync();
            var studentSubmissions = submissions.Cast<Submission>()
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.UploadedAt)
                .ToList();

            var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
            var allViolations = await _unitOfWork.Violations.GetAllAsync();

            return studentSubmissions.Select(submission =>
            {
                var violations = allViolations.Cast<Violation>()
                    .Where(v => v.SubmissionId == submission.SubmissionId)
                    .ToList();

                return new
                {
                    submission.SubmissionId,
                    submission.ZipFileName,
                    submission.UploadedAt,
                    submission.CheckedAt,
                    StudentId = submission.StudentId,
                    StudentInfo = student != null ? new
                    {
                        student.StudentId,
                        student.StudentCode,
                        student.FullName,
                        student.Email
                    } : null,
                    ViolationCount = violations.Count,
                    HasViolations = violations.Any()
                };
            }).Cast<object>().ToList();
        }

        /// <summary>
        /// Get all submissions from students in a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>List of submissions from all students in the class</returns>
        public async Task<List<object>> GetSubmissionsByClassIdAsync(int classId)
        {
            // Get all students in the class
            var classEntity = await _unitOfWork.ClassRepository.GetClassWithDetailsAsync(classId);
            if (classEntity == null)
                return new List<object>();

            var studentIds = classEntity.Students.Select(s => s.StudentId).ToList();

            if (!studentIds.Any())
                return new List<object>();

            // Get all submissions
            var submissions = await _unitOfWork.Submissions.GetAllAsync();
            var classSubmissions = submissions.Cast<Submission>()
                .Where(s => s.StudentId.HasValue && studentIds.Contains(s.StudentId.Value))
                .OrderByDescending(s => s.UploadedAt)
                .ToList();

            var allViolations = await _unitOfWork.Violations.GetAllAsync();

            var result = new List<object>();

            foreach (var submission in classSubmissions)
            {
                var student = submission.StudentId.HasValue
                    ? await _unitOfWork.StudentRepository.GetByIdAsync(submission.StudentId.Value)
                    : null;

                var violations = allViolations.Cast<Violation>()
                    .Where(v => v.SubmissionId == submission.SubmissionId)
                    .ToList();

                result.Add(new
                {
                    submission.SubmissionId,
                    submission.ZipFileName,
                    submission.UploadedAt,
                    submission.CheckedAt,
                    StudentId = submission.StudentId,
                    StudentInfo = student != null ? new
                    {
                        student.StudentId,
                        student.StudentCode,
                        student.FullName,
                        student.Email
                    } : null,
                    ViolationCount = violations.Count,
                    HasViolations = violations.Any()
                });
            }

            return result;
        }

        public async Task<List<object>> GetAllSubmissionsAsync()
        {
            var submissions = await _unitOfWork.Submissions.GetAllAsync();
            var allViolations = await _unitOfWork.Violations.GetAllAsync();

            var submissionList = submissions.Cast<Submission>()
                .OrderByDescending(s => s.UploadedAt)
                .ToList();

            var result = new List<object>();

            foreach (var submission in submissionList)
            {
                var student = submission.StudentId.HasValue
                    ? await _unitOfWork.StudentRepository.GetByIdAsync(submission.StudentId.Value)
                    : null;

                var violations = allViolations.Cast<Violation>()
                    .Where(v => v.SubmissionId == submission.SubmissionId)
                    .ToList();

                result.Add(new
                {
                    submission.SubmissionId,
                    submission.ZipFileName,
                    submission.UploadedAt,
                    submission.CheckedAt,
                    StudentId = submission.StudentId,
                    StudentInfo = student != null ? new
                    {
                        student.StudentId,
                        student.StudentCode,
                        student.FullName,
                        student.Email
                    } : null,
                    ViolationCount = violations.Count,
                    HasViolations = violations.Any()
                });
            }

            return result;
        }

        public async Task<bool> DeleteSubmissionAsync(int submissionId)
        {
            var submissions = await _unitOfWork.Submissions.GetAllAsync();
            var submission = submissions.Cast<Submission>()
                .FirstOrDefault(s => s.SubmissionId == submissionId);

            if (submission == null)
                return false;

            // Delete related violations first
            var violations = await _unitOfWork.Violations.GetAllAsync();
            var submissionViolations = violations.Cast<Violation>()
                .Where(v => v.SubmissionId == submissionId)
                .ToList();

            foreach (var violation in submissionViolations)
            {
                _unitOfWork.Violations.Delete(violation);
            }

            _unitOfWork.Submissions.Delete(submission);
            await _unitOfWork.SaveAsync();
            return true;
        }

        /// <summary>
        /// Process batch grading from a ZIP/RAR file containing multiple student submissions
        /// </summary>
        public async Task<BatchGradingResult> ProcessBatchGradingAsync(BatchGradingRequest request)
        {
            var result = new BatchGradingResult();

            try
            {
                // Validate file
                if (request.ArchiveFile == null || request.ArchiveFile.Length == 0)
                    throw new Exception("Archive file is required");

                // Extract class code from filename (e.g., "PRN232_SU25_PE_Block10w_PhuongLHK_(SE1751).rar" -> "SE1751")
                var fileName = Path.GetFileNameWithoutExtension(request.ArchiveFile.FileName);
                var classCode = ExtractClassCode(fileName);

                if (string.IsNullOrEmpty(classCode))
                {
                    result.Errors.Add("Could not extract class code from filename. Expected format: filename_(CLASSCODE).ext");
                    return result;
                }

                result.ClassName = classCode;
                result.Semester = request.DefaultSemester;

                // Parse rule IDs
                var ruleIds = (request.RuleIds ?? "")
                 .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(id => int.TryParse(id, out var val) ? val : 0)
             .Where(id => id > 0)
                .ToList();

                if (!ruleIds.Any())
                {
                    result.Errors.Add("At least one rule must be selected");
                    return result;
                }

                var selectedRules = await _unitOfWork.RuleRepository.GetRulesByIdsAsync(ruleIds);

                // Create temp folder for extraction
                var tempRootFolder = Path.Combine(Path.GetTempPath(), "BatchGrading_" + Guid.NewGuid());
                Directory.CreateDirectory(tempRootFolder);

                try
                {
                    // Extract main archive
                    var archivePath = Path.Combine(tempRootFolder, request.ArchiveFile.FileName);
                    using (var stream = new FileStream(archivePath, FileMode.Create))
                    {
                        await request.ArchiveFile.CopyToAsync(stream);
                    }

                    var extractPath = Path.Combine(tempRootFolder, "Extracted");
                    // Use universal extraction method to support ZIP, RAR, 7Z
                    ExtractArchive(archivePath, extractPath);

                    // Find or create class
                    var classEntity = await FindOrCreateClass(classCode, request.DefaultSemester, request.CreateClassIfNotExists);
                    if (classEntity == null)
                    {
                        result.Errors.Add($"Class '{classCode}' not found and CreateClassIfNotExists is false");
                        return result;
                    }

                    result.ClassId = classEntity.ClassId;
                    result.ClassCreated = classEntity.ClassId > 0;

                    // Find student folders
                    var studentFolders = FindStudentFolders(extractPath);
                    result.TotalStudentFolders = studentFolders.Count;

                    // Process each student
                    foreach (var studentFolder in studentFolders)
                    {
                        var studentResult = await ProcessStudentSubmission(
                 studentFolder,
                       classEntity,
                      selectedRules,
                  request.CreateStudentsIfNotExist
                           );

                        result.StudentResults.Add(studentResult);

                        if (studentResult.Success)
                        {
                            result.SuccessfulGradings++;
                            if (studentResult.IsNewStudent)
                                result.NewStudentsCreated++;
                            else
                                result.ExistingStudentsFound++;
                        }
                        else
                        {
                            result.FailedGradings++;
                            result.Errors.Add($"{studentResult.FolderName}: {studentResult.ErrorMessage}");
                        }
                    }

                    await _unitOfWork.SaveAsync();
                }
                finally
                {
                    // Cleanup temp folder
                    try { Directory.Delete(tempRootFolder, true); } catch { }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Batch grading failed: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Extract class code from filename (e.g., "(SE1751)" -> "SE1751")
        /// </summary>
        private string ExtractClassCode(string fileName)
        {
            var match = Regex.Match(fileName, @"\(([A-Z]{2}\d+)\)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// Find or create class entity
        /// </summary>
        private async Task<Class?> FindOrCreateClass(string className, string semester, bool createIfNotExists)
        {
            var classes = await _unitOfWork.ClassRepository.GetClassesWithDetailsAsync();
            var classEntity = classes.FirstOrDefault(c =>
           c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase) &&
            c.Semester.Equals(semester, StringComparison.OrdinalIgnoreCase));

            if (classEntity == null && createIfNotExists)
            {
                classEntity = new Class
                {
                    ClassName = className,
                    Semester = semester
                };
                await _unitOfWork.ClassRepository.AddAsync(classEntity);
                await _unitOfWork.SaveAsync();
            }

            return classEntity;
        }

        /// <summary>
        /// Find student folders in extracted archive
        /// </summary>
        private List<string> FindStudentFolders(string rootPath)
        {
            var studentFolders = new List<string>();

            // Look for folders that match student pattern (e.g., "AnhNASE183208")
            var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

            foreach (var dir in directories)
            {
                var folderName = Path.GetFileName(dir);
                // Check if folder name matches pattern: Name + Code (SE/QE/HE + numbers)
                if (Regex.IsMatch(folderName, @"[A-Z]{2}\d{6}", RegexOptions.IgnoreCase))
                {
                    studentFolders.Add(dir);
                }
            }

            // If no matches found at all levels, check first level directories
            if (!studentFolders.Any())
            {
                var firstLevelDirs = Directory.GetDirectories(rootPath);
                foreach (var dir in firstLevelDirs)
                {
                    // Check subdirectories
                    var subDirs = Directory.GetDirectories(dir);
                    foreach (var subDir in subDirs)
                    {
                        var folderName = Path.GetFileName(subDir);
                        if (Regex.IsMatch(folderName, @"[A-Z]{2}\d{6}", RegexOptions.IgnoreCase))
                        {
                            studentFolders.Add(subDir);
                        }
                    }
                }
            }

            return studentFolders;
        }

        /// <summary>
        /// Process individual student submission
        /// </summary>
        private async Task<StudentGradingResult> ProcessStudentSubmission(
                 string studentFolderPath,
                 Class classEntity,
           IEnumerable<Rule> rules,
         bool createStudentIfNotExists)
        {
            var result = new StudentGradingResult
            {
                FolderName = Path.GetFileName(studentFolderPath)
            };

            try
            {
                // Parse student info from folder name (e.g., "AnhNASE183208" -> Name: "AnhNA", Code: "SE183208")
                var (studentName, studentCode) = ParseStudentInfo(result.FolderName);
                result.StudentName = studentName;
                result.StudentCode = studentCode;

                if (string.IsNullOrEmpty(studentCode))
                {
                    result.Success = false;
                    result.ErrorMessage = "Could not parse student code from folder name";
                    return result;
                }

                // Find or create student
                var student = await _unitOfWork.StudentRepository.GetByStudentCodeAsync(studentCode);

                if (student == null && createStudentIfNotExists)
                {
                    student = new Student
                    {
                        StudentCode = studentCode,
                        FullName = studentName,
                        Email = $"{studentCode.ToLower()}@student.fpt.edu.vn"
                    };
                    await _unitOfWork.StudentRepository.AddAsync(student);
                    await _unitOfWork.SaveAsync();
                    result.IsNewStudent = true;
                }

                if (student == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Student not found and CreateStudentsIfNotExist is false";
                    return result;
                }

                result.StudentId = student.StudentId;

                // Add student to class if not already enrolled
                if (!classEntity.Students.Any(s => s.StudentId == student.StudentId))
                {
                    await _unitOfWork.ClassRepository.AddStudentToClassAsync(classEntity.ClassId, student.StudentId);
                    await _unitOfWork.SaveAsync();
                }

                // Find solution.zip file (look in 0/ subfolder)
                var solutionZipPath = FindSolutionZip(studentFolderPath);

                if (string.IsNullOrEmpty(solutionZipPath))
                {
                    result.Success = false;
                    result.ErrorMessage = "solution.zip not found in folder structure (expected in 0/solution.zip)";
                    return result;
                }

                result.ZipFileName = Path.GetFileName(solutionZipPath);

                // Extract and check solution
                var tempExtractPath = Path.Combine(Path.GetTempPath(), "StudentSolution_" + Guid.NewGuid());
                Directory.CreateDirectory(tempExtractPath);

                try
                {
                    ZipFile.ExtractToDirectory(solutionZipPath, tempExtractPath);

                    // Create submission record
                    var submission = new Submission
                    {
                        ZipFileName = result.ZipFileName,
                        UploadedAt = DateTime.Now,
                        CheckedAt = DateTime.Now,
                        StudentId = student.StudentId
                    };
                    await _unitOfWork.Submissions.AddAsync(submission);
                    await _unitOfWork.SaveAsync();

                    result.SubmissionId = submission.SubmissionId;
                    result.SubmittedAt = submission.UploadedAt;

                    // Check for violations
                    var violations = new List<Violation>();
                    var files = Directory.GetFiles(tempExtractPath, "*.*", SearchOption.AllDirectories);

                    foreach (var filePath in files)
                    {
                        string fileContent = "";
                        try { fileContent = await File.ReadAllTextAsync(filePath); } catch { }

                        foreach (var rule in rules)
                        {
                            bool match = Regex.IsMatch(Path.GetFileName(filePath), rule.Pattern, RegexOptions.IgnoreCase)
                             || (!string.IsNullOrEmpty(fileContent) && Regex.IsMatch(fileContent, rule.Pattern, RegexOptions.IgnoreCase));

                            if (match)
                            {
                                violations.Add(new Violation
                                {
                                    SubmissionId = submission.SubmissionId,
                                    RuleId = rule.RuleId,
                                    FilePath = filePath.Replace(tempExtractPath, "").TrimStart('\\', '/'),
                                    Message = rule.Description
                                });
                            }
                        }
                    }

                    // Save violations
                    foreach (var violation in violations)
                    {
                        await _unitOfWork.Violations.AddAsync(violation);
                    }
                    await _unitOfWork.SaveAsync();

                    result.ViolationCount = violations.Count;
                    result.Violations = violations.Select(v => new ViolationSummary
                    {
                        FilePath = v.FilePath ?? "",
                        Message = v.Message ?? "",
                        RuleId = v.RuleId ?? 0
                    }).ToList();

                    result.Success = true;
                }
                finally
                {
                    try { Directory.Delete(tempExtractPath, true); } catch { }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Parse student name and code from folder name
        /// Example: "AnhNASE183208" -> ("AnhNA", "SE183208")
        /// </summary>
        private (string name, string code) ParseStudentInfo(string folderName)
        {
            var match = Regex.Match(folderName, @"([A-Za-z]+)((?:SE|QE|HE)\d{6})", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var code = match.Groups[2].Value.ToUpper();
                return (name, code);
            }

            return (string.Empty, string.Empty);
        }

        /// <summary>
        /// Find solution.zip in student folder (usually in 0/ subfolder)
        /// </summary>
        private string FindSolutionZip(string studentFolderPath)
        {
            // Look for 0/solution.zip pattern
            var zeroFolder = Path.Combine(studentFolderPath, "0");
            if (Directory.Exists(zeroFolder))
            {
                var solutionZip = Path.Combine(zeroFolder, "solution.zip");
                if (File.Exists(solutionZip))
                    return solutionZip;
            }

            // Fallback: search for any .zip file
            var zipFiles = Directory.GetFiles(studentFolderPath, "*.zip", SearchOption.AllDirectories);
            return zipFiles.FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Extract archive file (supports ZIP, RAR, 7Z)
        /// </summary>
        private void ExtractArchive(string archivePath, string extractPath)
        {
            var extension = Path.GetExtension(archivePath).ToLowerInvariant();

            // Create directory if not exists
            Directory.CreateDirectory(extractPath);

            // Use SharpCompress to handle multiple formats
            using (var archive = ArchiveFactory.Open(archivePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(extractPath, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }
    }
}