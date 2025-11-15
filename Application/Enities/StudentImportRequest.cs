using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class StudentImportRequest
    {
        [Required(ErrorMessage = "Excel file is required")]
        public IFormFile ExcelFile { get; set; } = null!;

        [Required(ErrorMessage = "Default semester is required")]
        public string DefaultSemester { get; set; } = null!;
    }
}