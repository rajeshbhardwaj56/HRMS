using HRMS.Models.LeavePolicy;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Leave
{
    public class LeaveWeekOfInputParams
    {
        public long EmployeeID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class MyInfoInputParams
    {
        public long PKNo { get; set; }
        public long LeaveSummaryID { get; set; }
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }
        public long UserID { get; set; }
        public long? RoleId { get; set; }
        public int GenderId { get; set; }
        public long JobLocationTypeID { get; set; }
    }
    public class LeaveSummaryModel
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long LeaveSummaryID { get; set; }
        public long LeaveStatusID { get; set; }
        public string LeaveStatusName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime HalfDayDate { get; set; }
        public string StartDateFormatted { get; set; } = string.Empty;
        public string EndDateFormatted { get; set; } = string.Empty;
        public long LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public long LeaveDurationTypeID { get; set; }
        public string LeaveDurationTypeName { get; set; } = string.Empty;
        public decimal NoOfDays { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public long UserID { get; set; }
        public long EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public long CompanyID { get; set; }
        public long LeavePolicyID { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public string ApproveRejectComment { get; set; } = string.Empty;
        public string OfficialEmailID { get; set; } = string.Empty;
        public string EmployeeFirstName { get; set; } = string.Empty;
        public string ManagerOfficialEmailID { get; set; } = string.Empty;
        public string ManagerFirstName { get; set; } = string.Empty;

        public string? UploadCertificate { get; set; } = string.Empty;
        public DateTime? ExpectedDeliveryDate { get; set; } = DateTime.Now;
        public DateTime? ChildDOB { get; set; }
        public DateTime? JoiningDate { get; set; }
        public int? CampOff { get; set; }

    }

    public class LeaveResults
    {
        public LeaveSummaryModel leaveSummaryModel { get; set; } = new LeaveSummaryModel();
        public List<LeaveSummaryModel> leavesSummary { get; set; } = new List<LeaveSummaryModel>();
        public List<SelectListItem> leaveTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> leaveDurationTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> leaveStatuses { get; set; } = new List<SelectListItem>();
    }

    public class ErrorLeaveResults
    {
        public int status { get; set; }
        public string message { get; set; }
    }

    public class CampOffEligible
    {
        public long EmployeeID { get; set; }
        public long JobLocationTypeID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EmployeeNumber { get; set; }
        public decimal RequestedLeaveDays { get; set; }
    }
    public class CompOffValidationResult
    {
        public int IsEligible { get; set; }
        public string Message { get; set; }
        public int EligibleDays { get; set; }
        public int RequestedDays { get; set; }
        public int AvailableCompOffDays { get; set; }
    }

    public class UpdateLeaveStatus
    {
        public long EmployeeID { get; set; }
        public long NewLeaveStatusID { get; set; }
        public long LeaveSummaryID { get; set; }
        public string? Message { get; set; }
    }

    public class LastLevelEmployeeDropdownParams
    {
        public long EmployeeID { get; set; }
    }
    public class LastLevelEmployeeDropdown
    {
        public string? EmployeeNumber { get; set; }
        public long EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeType { get; set; }
        public string? ManagerName { get; set; }
        
    }
}
