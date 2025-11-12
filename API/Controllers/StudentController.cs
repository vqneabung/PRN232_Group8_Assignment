using Application.Enities;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;
using Service.Helpers;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        //[HttpGet("import/template")]
        //public IActionResult DownloadImportTemplate()
        //{
        //    try
        //    {
        //        var templateBytes = ExcelTemplateHelper.GenerateStudentImportTemplate();
        //        var fileName = $"StudentImportTemplate_{DateTime.Now:yyyyMMdd}.xlsx";
                
        //        return File(templateBytes, 
        //            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        //            fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

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

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportStudents([FromForm] ImportStudentsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var importedCount = await _studentService.ImportStudentsFromExcelAsync(request);
                return Ok(new { 
                    message = $"Successfully imported {importedCount} students.",
                    importedCount = importedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}