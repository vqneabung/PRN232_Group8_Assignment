using System;
using System.Collections.Generic;

namespace Application.Models;

public partial class Class
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = null!;

    public string Semester { get; set; } = null!;

    public int? Lecturer { get; set; }

    public int? Examiner { get; set; }

    public virtual User? ExaminerNavigation { get; set; }

    public virtual User? LecturerNavigation { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
