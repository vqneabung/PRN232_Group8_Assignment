using System;
using System.Collections.Generic;

namespace Application.Models;

public partial class Violation
{
    public int ViolationId { get; set; }

    public int? SubmissionId { get; set; }

    public int? RuleId { get; set; }

    public string? FilePath { get; set; }

    public string? Message { get; set; }

    public virtual Rule? Rule { get; set; }

    public virtual Submission? Submission { get; set; }
}
