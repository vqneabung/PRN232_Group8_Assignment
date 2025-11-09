using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class ImportStudentsRequest
    {
        [Required]
        public IFormFile ExcelFile { get; set; } = null!;
        
        public bool SkipDuplicates { get; set; } = true;
        
        public bool UpdateExisting { get; set; } = false;
    }
}