using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
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
<<<<<<< HEAD
        public List<HolidayModel> HolidayModel { get; set; } = new List<HolidayModel>();
        public LeavePolicyModel LeavePolicyDetails { get; set; } = new LeavePolicyModel();
        public List<EmploymentHistory> employmentHistory { get; set; }
        public DateTime? JoiningDate { get; set; } = DateTime.UtcNow;


=======
        public List<EmploymentHistory> employmentHistory { get; set; }
>>>>>>> 8da7dfbc1ac23bd8f84877ccd188f2c120e85b39
    }
}
