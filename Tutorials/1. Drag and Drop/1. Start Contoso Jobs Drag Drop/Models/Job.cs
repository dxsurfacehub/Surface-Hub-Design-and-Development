using Contoso_Jobs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso_Jobs.Models
{
    public class Job 
    {
        public string Title { get; set; }

        public string Description { get; set; }
        
        public JobStatus Status { get; set; }
        
    }
}
