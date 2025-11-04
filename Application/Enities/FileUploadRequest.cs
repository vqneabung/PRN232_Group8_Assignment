using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Enities
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
        public string RuleIds { get; set; }
    }

}
