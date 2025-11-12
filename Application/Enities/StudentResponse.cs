namespace Application.Enities
{
    public class StudentResponse
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int SubmissionCount { get; set; }
    }
}