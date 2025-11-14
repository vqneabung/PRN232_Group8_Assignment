using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class ClassRequest
    {
        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Semester { get; set; } = null!;

        public int? Lecturer { get; set; }

        public int? Examiner { get; set; }
    }
}