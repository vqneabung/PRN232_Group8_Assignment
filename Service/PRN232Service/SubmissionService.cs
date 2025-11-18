using Application.Enities;
using Application.Models;
using Application.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Service.IPRN232Service;
using System.IO.Compression;
using System.Text.RegularExpressions;

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
    }
}