namespace Application.Enities
{
    public class StudentImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessfulImports { get; set; }
        public int UpdatedStudents { get; set; }
        public int NewStudents { get; set; }
        public int NewClasses { get; set; }
        public int ExistingClasses { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<ImportedStudentInfo> ImportedStudents { get; set; } = new List<ImportedStudentInfo>();
        public List<ImportedClassInfo> ImportedClasses { get; set; } = new List<ImportedClassInfo>();
    }

    public class ImportedStudentInfo
    {
        public string StudentCode { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string Action { get; set; } = null!; // "Created" or "Updated"
        public List<string> AssignedClasses { get; set; } = new List<string>();
    }

    public class ImportedClassInfo
    {
        public string ClassName { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public string Action { get; set; } = null!; // "Created" or "Existing"
        public int StudentsCount { get; set; }
    }
}