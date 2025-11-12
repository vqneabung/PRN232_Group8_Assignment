namespace Application.Enities
{
    public class SolutionValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string ExpectedFileName { get; set; } = string.Empty;
        public string? ActualFileName { get; set; }
    }
}