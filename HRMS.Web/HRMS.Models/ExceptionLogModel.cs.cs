using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class ExceptionLogModel
    {
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public string? Url { get; set; }
        
        public string? AreaName { get; set; }
        public long? EmployeeId { get; set; }
    }
}
