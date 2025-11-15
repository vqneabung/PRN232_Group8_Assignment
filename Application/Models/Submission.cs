using System;
using System.Collections.Generic;

namespace Application.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    public string ZipFileName { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public DateTime? CheckedAt { get; set; }

    public int? StudentId { get; set; }

    public virtual Student? Student { get; set; }

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
