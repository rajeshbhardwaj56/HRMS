using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Leave
{

    public class MyInfoInputParams
    {
        public long PKNo { get; set; }
        public long LeaveSummaryID { get; set; }
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }
        public long UserID { get; set; }
    }
    public class LeaveSummaryModel
    {
        public long LeaveSummaryID { get; set; }
        public long LeaveStatusID { get; set; }
        public string LeaveStatusName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public long LeaveDurationTypeID { get; set; }
        public string LeaveDurationTypeName { get; set; } = string.Empty;
        public decimal NoOfDays { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public long UserID { get; set; }
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }

    }

    public class LeaveResults
    {
        public LeaveSummaryModel leaveSummaryModel { get; set; } = new LeaveSummaryModel();
        public List<LeaveSummaryModel> leavesSummary { get; set; } = new List<LeaveSummaryModel>();
        public List<SelectListItem> leaveTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> leaveDurationTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> leaveStatuses { get; set; } = new List<SelectListItem>();
    }   
}
