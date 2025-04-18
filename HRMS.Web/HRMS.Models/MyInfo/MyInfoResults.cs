﻿using HRMS.Models.Employee;
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
        public List<HolidayModel> HolidayModel { get; set; } = new List<HolidayModel>();
        public LeavePolicyModel LeavePolicyDetails { get; set; } = new LeavePolicyModel();
        public List<EmploymentHistory> employmentHistory { get; set; }
        public DateTime? JoiningDate { get; set; } = DateTime.UtcNow;


    }
}
