using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enities
{
    public class RuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string? Severity { get; set; }
        public string? Description { get; set; }
    }

    public class RuleResponse
    {
        public int RuleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string? Severity { get; set; }
        public string? Description { get; set; }
    }
}
