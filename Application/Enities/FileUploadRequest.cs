using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    public class FileUploadRequest
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;
        
        public string? RuleIds { get; set; }
        
        [Required(ErrorMessage = "StudentId is required")]
        public int StudentId { get; set; }
    }

}
