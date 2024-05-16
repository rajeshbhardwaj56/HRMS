using HRMS.Models.Employee;
using HRMS.Models.Leave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.MyInfo
{
    public class MyInfoResults
    {
        public LeaveResults leaveResults { get; set; } = new LeaveResults();
        public EmployeeModel employeeModel { get; set; } = new EmployeeModel();
        public EmploymentDetail employmentDetail { get; set; } = new EmploymentDetail();
    }
}
