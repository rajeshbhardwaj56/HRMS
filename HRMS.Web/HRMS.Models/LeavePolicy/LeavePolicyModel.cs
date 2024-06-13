using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.LeavePolicy
{

    public class LeavePolicyInputParans
    {
        public long CompanyID { get; set; }
        public long LeavePolicyID { get; set; }
    }


    public class LeavePolicyModel
    {
        public long LeavePolicyID { get; set; }
        public long CompanyID { get; set; }
        public string LeavePolicyName { get; set; } = string.Empty;
        public int MaximumLeaveAllocationAllowed { get; set; }
        public int ApplicableAfterWorkingDays { get; set; }
        public int MaximumConsecutiveLeavesAllowed { get; set; }
        public bool IsCarryForward { get; set; }
        public bool IsLeaveWithoutPay { get; set; }
        public bool IsPartiallyPaidLeave { get; set; }
        public bool IsOptionalLeave { get; set; }
        public bool IsAllowNegativeBalance { get; set; }
        public bool IsAllowOverAllocation { get; set; }
        public bool IsIncludeHolidaysWithinLeavesAsLeaves { get; set; }
        public bool IsCompendatory { get; set; }
        public bool IsAllowEncashment { get; set; }
        public bool IsEarnedLeave { get; set; }
        public int MaximumCasualLeaveAllocationAllowed { get; set; }
        public int MaximumMedicalLeaveAllocationAllowed { get; set; }
        public int MaximumAnnualLeaveAllocationAllowed { get; set; }
    }
}
