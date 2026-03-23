using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HRMS.Models.AttendenceList
{
    public class AttendenceListInputParans
    {
        public long ID { get; set; }
    }
    public class AttendenceListModel
    {
        public long ID { get; set; }
        public string? EncodedId { get; set; }
        public string Series { get; set; }
        public DateTime? AttendenceDate { get; set; }
        public long EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? Status { get; set; }
        public int ShiftSelection { get; set; }
        public bool LateEntry { get; set; }
        public bool EarlyExit { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string SelectedStatus { get; set; } = string.Empty;
        public SelectList? StatusList { get; set; }
        public string SelectedShift { get; set; }

        public List<SelectListItem> Employeelist = new List<SelectListItem>();
        public List<SelectListItem> ShiftList = new List<SelectListItem>();
    }

    public class CompOffAttendanceInputParams
    {
        public long EmployeeID { get; set; }
        public long JobLocationTypeID { get; set; }

        public long AttendanceStatus { get; set; }
    }


    public class CompOffAttendanceModel
    {
        public long ID { get; set; }
        public string? EncodedId { get; set; }
        public string? UserId { get; set; }
        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public DateTime? FirstLogDate { get; set; }
        public DateTime? LastLogDate { get; set; }
        public string? HoursWorked { get; set; }
        public string? ManagerName { get; set; }
        public long ManagerId { get; set; }
        public long ApprovalStatus { get; set; }
        public string? AttendanceStatus { get; set; }


    }

    public class CompOffInputParams
    {
        public int AttendanceStatusId { get; set; }
        public int ID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Day { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public long UserId { get; set; }
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }
        public long? RoleId { get; set; }
        public string? conStr { get; set; }

    }

    public class CompOffAttendanceRequestModel
    {
        public long? ID { get; set; }
        public long RequestID { get; set; }
        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? FirstLogDate { get; set; }
        public DateTime? LastLogDate { get; set; }
        public TimeSpan? HoursWorked { get; set; }
        public int? AttendanceStatusId { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? Comments { get; set; }
        public long? AttendanceId { get; set; }
        public string? AttendanceDescription { get; set; }
        public long ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public long ApprovalStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? AuditEmployeeId { get; set; }
        public long? UserId { get; set; }
        public long? RoleId { get; set; }

        public long? CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class CompOffLogSubmission
    {
        public long? EmployeeId { get; set; }
        public List<CompOffLogItem> Logs { get; set; }
        public string? Comment { get; set; }
    }

    public class CompOffLogItem
    {
        public long? AttendanceId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime FirstLog { get; set; }
        public DateTime LastLog { get; set; }
        public TimeSpan? HoursWorked { get; set; }
        public long? CreatedBy { get; set; }
    }


    public class EmployeeData
    {
        public string? EmployeeNumber { get; set; } = string.Empty;
        public string? EmployeeName { get; set; } = string.Empty;
        public bool IsManager { get; set; }
    }
}


