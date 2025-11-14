using Application.Enities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IPRN232Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        /// <summary>
        /// Get all classes
        /// </summary>
        /// <returns>List of classes with details</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            try
            {
                var classes = await _classService.GetAllClassesAsync();
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get class by ID
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Class details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClass(int id)
        {
            try
            {
                var classEntity = await _classService.GetClassByIdAsync(id);
                if (classEntity == null)
                {
                    return NotFound(new { message = "Class not found" });
                }
                return Ok(classEntity);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get classes by lecturer
        /// </summary>
        /// <param name="lecturerId">Lecturer user ID</param>
        /// <returns>Classes taught by the lecturer</returns>
        [HttpGet("by-lecturer/{lecturerId}")]
        public async Task<IActionResult> GetClassesByLecturer(int lecturerId)
        {
            try
            {
                var classes = await _classService.GetClassesByLecturerAsync(lecturerId);
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get classes by examiner
        /// </summary>
        /// <param name="examinerId">Examiner user ID</param>
        /// <returns>Classes examined by the examiner</returns>
        [HttpGet("by-examiner/{examinerId}")]
        public async Task<IActionResult> GetClassesByExaminer(int examinerId)
        {
            try
            {
                var classes = await _classService.GetClassesByExaminerAsync(examinerId);
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get classes by semester
        /// </summary>
        /// <param name="semester">Semester name</param>
        /// <returns>Classes in the specified semester</returns>
        [HttpGet("by-semester/{semester}")]
        public async Task<IActionResult> GetClassesBySemester(string semester)
        {
            try
            {
                var classes = await _classService.GetClassesBySemesterAsync(semester);
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new class
        /// </summary>
        /// <param name="request">Class creation details</param>
        /// <returns>Created class details</returns>
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdClass = await _classService.CreateClassAsync(request);
                return CreatedAtAction(nameof(GetClass), new { id = createdClass.ClassId }, createdClass);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <param name="request">Updated class details</param>
        /// <returns>Updated class details</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedClass = await _classService.UpdateClassAsync(id, request);
                if (updatedClass == null)
                {
                    return NotFound(new { message = "Class not found" });
                }

                return Ok(updatedClass);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a class
        /// </summary>
        /// <param name="id">Class ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                var deleted = await _classService.DeleteClassAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Class not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get students in a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>List of students in the class</returns>
        [HttpGet("{classId}/students")]
        public async Task<IActionResult> GetStudentsInClass(int classId)
        {
            try
            {
                var students = await _classService.GetStudentsInClassAsync(classId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add a student to a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{classId}/students/{studentId}")]
        public async Task<IActionResult> AddStudentToClass(int classId, int studentId)
        {
            try
            {
                var added = await _classService.AddStudentToClassAsync(classId, studentId);
                if (!added)
                {
                    return BadRequest(new { error = "Failed to add student to class. Check if class and student exist." });
                }

                return Ok(new { message = "Student added to class successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a student from a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{classId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudentFromClass(int classId, int studentId)
        {
            try
            {
                var removed = await _classService.RemoveStudentFromClassAsync(classId, studentId);
                if (!removed)
                {
                    return NotFound(new { message = "Class not found or student not in class" });
                }

                return Ok(new { message = "Student removed from class successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if class name exists for a semester
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="semester">Semester</param>
        /// <returns>Existence status</returns>
        [HttpGet("check-existence")]
        public async Task<IActionResult> CheckClassExistence([FromQuery] string className, [FromQuery] string semester)
        {
            try
            {
                if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(semester))
                {
                    return BadRequest(new { error = "ClassName and Semester are required" });
                }

                var exists = await _classService.ClassExistsAsync(className, semester);
                return Ok(new 
                { 
                    exists, 
                    message = exists ? $"Class '{className}' already exists in semester '{semester}'" : "Class name is available"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
