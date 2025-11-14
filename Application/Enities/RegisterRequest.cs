using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }
    }
}