using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class StudentRequest
    {
        [Required]
        [MaxLength(20)]
        public string StudentCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FullName { get; set; }

        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }
    }
}