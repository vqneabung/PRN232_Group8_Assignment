using Application.Enities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Service.IPRN232Service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound(new { message = "Student not found" });
                }
                return Ok(student);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("by-code/{studentCode}")]
        public async Task<IActionResult> GetStudentByCode(string studentCode)
        {
            try
            {
                var student = await _studentService.GetStudentByCodeAsync(studentCode);
                if (student == null)
                {
                    return NotFound(new { message = "Student not found" });
                }
                return Ok(student);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdStudent = await _studentService.CreateStudentAsync(request);
                return CreatedAtAction(nameof(GetStudent), new { id = createdStudent.StudentId }, createdStudent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedStudent = await _studentService.UpdateStudentAsync(id, request);
                if (updatedStudent == null)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(updatedStudent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var result = await _studentService.DeleteStudentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Student not found" });
                }

                return Ok(new { message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Import students from Excel file
        /// Expected columns: Student Code, Class, Full Name, Email (Email can be null)
        /// If student code exists, updates student info. If class exists, adds student to class, if not creates new class.
        /// </summary>
        /// <param name="request">Excel file and default semester</param>
        /// <returns>Import results with statistics and any errors</returns>
        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportStudentsFromExcel([FromForm] StudentImportRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate file extension
                if (request.ExcelFile != null)
                {
                    var allowedExtensions = new[] { ".xlsx", ".xls" };
                    var fileExtension = Path.GetExtension(request.ExcelFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { error = "Only Excel files (.xlsx, .xls) are allowed" });
                    }

                    // Check file size (max 10MB)
                    if (request.ExcelFile.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest(new { error = "File size must be less than 10MB" });
                    }
                }

                var result = await _studentService.ImportStudentsFromExcelAsync(request);
                
                // If there are errors and no successful imports, return bad request
                if (result.Errors.Any() && result.SuccessfulImports == 0)
                {
                    return BadRequest(new { 
                        message = "Import failed", 
                        errors = result.Errors,
                        statistics = new {
                            result.TotalRows,
                            result.SuccessfulImports,
                            ErrorCount = result.Errors.Count
                        }
                    });
                }

                // Return success response with detailed information
                return Ok(new {
                    message = $"Import completed. {result.SuccessfulImports} out of {result.TotalRows} rows processed successfully.",
                    statistics = new {
                        result.TotalRows,
                        result.SuccessfulImports,
                        result.NewStudents,
                        result.UpdatedStudents,
                        result.NewClasses,
                        result.ExistingClasses,
                        ErrorCount = result.Errors.Count
                    },
                    importedStudents = result.ImportedStudents,
                    importedClasses = result.ImportedClasses,
                    errors = result.Errors.Any() ? result.Errors : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Download Excel template for student import
        /// Template includes sample data showing the expected format
        /// </summary>
        /// <returns>Excel template file with sample data</returns>
        [HttpGet("import-template")]
        [AllowAnonymous]
        public IActionResult DownloadImportTemplate()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                var stream = new MemoryStream();
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Students");
                    
                    // Add headers
                    worksheet.Cells[1, 1].Value = "Student Code";
                    worksheet.Cells[1, 2].Value = "Class";
                    worksheet.Cells[1, 3].Value = "Full Name";
                    worksheet.Cells[1, 4].Value = "Email";
                    
                    // Add sample data
                    worksheet.Cells[2, 1].Value = "SE123456";
                    worksheet.Cells[2, 2].Value = "Advanced Programming";
                    worksheet.Cells[2, 3].Value = "John Doe";
                    worksheet.Cells[2, 4].Value = "john.doe@example.com";
                    
                    worksheet.Cells[3, 1].Value = "SE123457";
                    worksheet.Cells[3, 2].Value = "Database Systems";
                    worksheet.Cells[3, 3].Value = "Jane Smith";
                    worksheet.Cells[3, 4].Value = "jane.smith@example.com";
                    
                    worksheet.Cells[4, 1].Value = "SE123458";
                    worksheet.Cells[4, 2].Value = "Advanced Programming";
                    worksheet.Cells[4, 3].Value = "Bob Wilson";
                    worksheet.Cells[4, 4].Value = ""; // Example with empty email
                    
                    // Add instructions in separate sheet
                    var instructionSheet = package.Workbook.Worksheets.Add("Instructions");
                    instructionSheet.Cells[1, 1].Value = "Excel Import Instructions:";
                    instructionSheet.Cells[2, 1].Value = "1. Student Code: Required, unique identifier for student";
                    instructionSheet.Cells[3, 1].Value = "2. Class: Required, class name (will create if doesn't exist)";
                    instructionSheet.Cells[4, 1].Value = "3. Full Name: Optional, student's full name";
                    instructionSheet.Cells[5, 1].Value = "4. Email: Optional, valid email address";
                    instructionSheet.Cells[6, 1].Value = "";
                    instructionSheet.Cells[7, 1].Value = "Rules:";
                    instructionSheet.Cells[8, 1].Value = "- If student code exists, updates student information";
                    instructionSheet.Cells[9, 1].Value = "- If class exists, adds student to existing class";
                    instructionSheet.Cells[10, 1].Value = "- If class doesn't exist, creates new class with default semester";
                    instructionSheet.Cells[11, 1].Value = "- Email can be left blank";
                    
                    // Style the header row in Students sheet
                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }
                    
                    // Style instruction sheet
                    using (var range = instructionSheet.Cells[1, 1, 11, 1])
                    {
                        range.Style.Font.Size = 11;
                        instructionSheet.Cells[1, 1].Style.Font.Bold = true;
                        instructionSheet.Cells[1, 1].Style.Font.Size = 14;
                        instructionSheet.Cells[7, 1].Style.Font.Bold = true;
                    }
                    
                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();
                    instructionSheet.Cells.AutoFitColumns();
                    
                    package.Save();
                }
                
                stream.Position = 0;
                var fileName = "Student_Import_Template.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get import statistics and validation for an Excel file without actually importing
        /// </summary>
        /// <param name="request">Excel file validation request</param>
        /// <returns>Validation results and preview of what would be imported</returns>
        [HttpPost("validate-import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ValidateImport([FromForm] ValidateImportRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(request.ExcelFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { error = "Only Excel files (.xlsx, .xls) are allowed" });
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using var stream = new MemoryStream();
                await request.ExcelFile.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                
                if (package.Workbook.Worksheets.Count == 0)
                {
                    return BadRequest(new { error = "Excel file contains no worksheets" });
                }

                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                {
                    return BadRequest(new { error = "Excel file must contain at least a header row and one data row" });
                }

                var validation = new
                {
                    TotalRows = rowCount - 1,
                    FileName = request.ExcelFile.FileName,
                    FileSize = $"{request.ExcelFile.Length / 1024:N0} KB",
                    DefaultSemester = request.DefaultSemester,
                    HeaderValidation = ValidateHeaders(worksheet),
                    PreviewData = GetPreviewData(worksheet, Math.Min(5, rowCount - 1)) // Show first 5 rows
                };

                return Ok(validation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private object ValidateHeaders(OfficeOpenXml.ExcelWorksheet worksheet)
        {
            var expectedHeaders = new[] { "Student Code", "Class", "Full Name", "Email" };
            var headerValidation = new List<object>();

            for (int col = 1; col <= 4; col++)
            {
                var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                var expected = expectedHeaders[col - 1];
                var isValid = string.Equals(headerValue, expected, StringComparison.OrdinalIgnoreCase);
                
                headerValidation.Add(new {
                    Column = col,
                    Expected = expected,
                    Found = headerValue,
                    IsValid = isValid
                });
            }

            return headerValidation;
        }

        private object GetPreviewData(OfficeOpenXml.ExcelWorksheet worksheet, int rowsToShow)
        {
            var previewData = new List<object>();

            for (int row = 2; row <= rowsToShow + 1; row++)
            {
                previewData.Add(new {
                    Row = row,
                    StudentCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim(),
                    Class = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
                    FullName = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                    Email = worksheet.Cells[row, 4].Value?.ToString()?.Trim()
                });
            }

            return previewData;
        }
    }
}