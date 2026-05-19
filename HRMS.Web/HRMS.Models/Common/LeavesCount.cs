using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class LeavesCount
    {
        public string? EmployeeNumber { get; set; } 
        public string? EmployeeName { get; set; }
        public decimal? PrivilegeLeaveConsumed { get; set; }
        public double? FinalLeaveBalance { get; set; }
        public decimal? AvailableCompOffDays { get; set; }
    }
}
