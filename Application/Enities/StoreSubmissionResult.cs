using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enities
{
    public class StoreSubmissionResult
    {
        public string Message { get; set; } = string.Empty;
        public string SubmissionId { get; set; } = string.Empty;
        public int FilesStored { get; set; }
    }
}
