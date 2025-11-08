using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Models;

public partial class Student
{
    public int StudentId { get; set; }

    [MaxLength(20)]
    public string StudentCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? FullName { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}