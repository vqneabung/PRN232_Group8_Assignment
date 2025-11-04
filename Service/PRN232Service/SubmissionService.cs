using Application.Enities;
using Application.Models;
using Application.Repos;
using Application.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Service.IPRN232Service;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                CheckedAt = DateTime.Now
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
                submission.ZipFileName,
                submission.UploadedAt,
                submission.CheckedAt,
                Violations = violations.Select(v => new
                {
                    v.FilePath,
                    v.Message,
                    Rule = selectedRules.FirstOrDefault(r => r.RuleId == v.RuleId)
                })
            };
        }
    }
}