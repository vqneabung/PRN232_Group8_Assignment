namespace Application.Enities
{
    /// <summary>
    /// Result of batch grading operation
 /// </summary>
    public class BatchGradingResult
    {
        public string ClassName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
   public int ClassId { get; set; }
 public int TotalStudentFolders { get; set; }
        public int SuccessfulGradings { get; set; }
    public int FailedGradings { get; set; }
      public int NewStudentsCreated { get; set; }
        public int ExistingStudentsFound { get; set; }
        public bool ClassCreated { get; set; }
        public List<StudentGradingResult> StudentResults { get; set; } = new();
      public List<string> Errors { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Grading result for individual student
    /// </summary>
  public class StudentGradingResult
    {
        public string FolderName { get; set; } = string.Empty;
      public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public bool IsNewStudent { get; set; }
        public bool Success { get; set; }
      public string? ErrorMessage { get; set; }
 public int? SubmissionId { get; set; }
        public string? ZipFileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int ViolationCount { get; set; }
  public List<ViolationSummary>? Violations { get; set; }
    }

    /// <summary>
    /// Summary of violations found
    /// </summary>
    public class ViolationSummary
    {
   public string FilePath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
   public int RuleId { get; set; }
    }
}
