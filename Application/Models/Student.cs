using System;
using System.Collections.Generic;

namespace Application.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
