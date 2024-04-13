using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Leave
{
    public class LeaveSummayModel
    {
        public long LeaveSummaryID { get; set; }
        public long LeaveStatusID { get; set; }
        public string LeaveStatusName { get; set; } = string.Empty; 
        public string Reason { get; set; } = string.Empty;
        public DateTime? RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public long LeaveDurationTypeID { get; set; }
        public string LeaveDurationTypeName { get; set; } = string.Empty;
        public int NoOfDays { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public long UserID { get; set; }
        public long EmployeeID { get; set; }
        
    }

    public class LeaveResults
    {
        public LeaveSummayModel leaveSummayModel { get; set; } = new LeaveSummayModel();
        public List<LeaveSummayModel> leavesSummay { get; set; } = new List<LeaveSummayModel>();
        public List<SelectListItem> leaveTypes = new List<SelectListItem>();
        public List<SelectListItem> leaveDurationTypes = new List<SelectListItem>();
        public List<SelectListItem> leaveStatuses = new List<SelectListItem>();
    }
}
