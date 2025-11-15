namespace Application.Enities
{
    public class ClassResponse
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public int? Lecturer { get; set; }
        public int? Examiner { get; set; }
        public string? LecturerName { get; set; }
        public string? ExaminerName { get; set; }
        public int StudentCount { get; set; }
        public List<StudentResponse>? Students { get; set; }
    }
}