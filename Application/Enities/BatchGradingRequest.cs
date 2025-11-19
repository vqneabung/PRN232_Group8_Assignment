using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Enities
{
    /// <summary>
    /// Request model for batch grading from a ZIP/RAR file containing multiple student submissions
    /// </summary>
    public class BatchGradingRequest
    {
    /// <summary>
        /// ZIP or RAR file containing student submissions
        /// Expected structure: FileName contains class code (e.g., SE1751)
        /// Inside: StudentFolder/0/solution.zip for each student
        /// </summary>
        [Required(ErrorMessage = "Archive file is required")]
        public IFormFile ArchiveFile { get; set; } = null!;

  /// <summary>
        /// Comma-separated list of rule IDs to check
        /// </summary>
        public string? RuleIds { get; set; }

   /// <summary>
        /// Default semester if class needs to be created (e.g., "Spring2024", "SU25")
 /// </summary>
  [Required(ErrorMessage = "DefaultSemester is required")]
     public string DefaultSemester { get; set; } = null!;

   /// <summary>
   /// If true, creates class if it doesn't exist
        /// </summary>
   public bool CreateClassIfNotExists { get; set; } = true;

        /// <summary>
     /// If true, creates students if they don't exist
  /// </summary>
        public bool CreateStudentsIfNotExist { get; set; } = true;
    }
}
