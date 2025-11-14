using System;
using System.Collections.Generic;

namespace Application.Models;

public partial class Rule
{
    public int RuleId { get; set; }

    public string Name { get; set; } = null!;

    public string Pattern { get; set; } = null!;

    public string? Severity { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
