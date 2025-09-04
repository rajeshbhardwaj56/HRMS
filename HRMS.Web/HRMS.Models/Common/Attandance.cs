using HRMS.Models.AttendenceList;
using HRMS.Models.Leave;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class AttendanceInputParams
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
        public long? JobLocationID { get; set; }
        public long? ManagerID { get; set; }
        public string? conStr { get; set; }
        public string? SearchTerm { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int? DisplayStart { get; set; }    // @DisplayStart
        public int? DisplayLength { get; set; }
        public string? SortCol { get; set; }      // @SortCol
        public string? SortDir { get; set; }


    }
    public class Attendance
    {
        public int? AttendanceStatusId { get; set; }
        public long ID { get; set; }
        public string? EncodedId { get; set; }
        public long? UserId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? FirstLogDate { get; set; } = DateTime.Now;
        public DateTime? LastLogDate { get; set; } = DateTime.Now;
        public int? HoursWorked { get; set; }
        public bool? IsManual { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? Comments { get; set; }
        public string? EmployeeNumber { get; set; }
        public long? ModifiedBy { get; set; }
        public long? CreatedBy { get; set; }
        public long? EmployeeId { get; set; }
        public long? RoleId { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? CreatedDate { get; set; }



        public List<SelectListItem> Employeelist = new List<SelectListItem>();

    }



    public class AttendanceViewModel
    {
        public long? ID { get; set; }
        public long? EmployeeId { get; set; }
        public string? EmployeNumber { get; set; }
        public string? ManagerManagerName { get; set; }
        public string? ManagerName { get; set; }
        public string? EncryptedIdentity { get; set; }
        public string? EmployeeNumberWithoutAbbr { get; set; }
        public string? EmployeeName { get; set; }
        public string? JobLocationName { get; set; }
        public int TotalWorkingDays { get; set; }
        public decimal PresentDays { get; set; }
        public DateTime? WorkDate { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        public decimal TotalLeaves { get; set; }

        public Dictionary<string, string> AttendanceByDay { get; set; } = new Dictionary<string, string>();
    }


    public class AttendanceWithHolidays
    {
        public List<Attendance> Attendances { get; set; }
        public List<HolidayModel> Holidays { get; set; }
        public List<LeaveSummaryModel> Leaves { get; set; }
    }

    public class MonthlyViewAttendance
    {
        public List<DailyAttendanceStatus> DailyStatuses { get; set; } = new List<DailyAttendanceStatus>();
    }

    public class DailyAttendanceStatus
    {
        public string DayLabel { get; set; }       // e.g., "Thu_1"
        public DateTime Date { get; set; }         // e.g., "2025-05-01"
        public DateTime? FirstLogDate { get; set; }         // e.g., "2025-05-01"
        public DateTime? LastLogDate { get; set; }         // e.g., "2025-05-01"
        public string Status { get; set; }         // e.g., "P", "A", "H", "L", or NULL
    }
    public class MyAttendanceList
    {
        public List<Attendance> Attendances { get; set; }
    }

    public class AttendanceDeviceLog
    {
        public long? ID { get; set; }
        public string? EmployeeId { get; set; }

        public string? EmployeeName { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? AttendanceStatus { get; set; }
    }
    public class AttendanceLogResponse
    {
        public List<AttendanceDeviceLog> AttendanceLogs { get; set; } = new List<AttendanceDeviceLog>();
    }
    public class AttendanceStatusRequest
    {
        public int AttendanceStatus { get; set; }
        public int CompOffStatus { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }


    public class AttendanceWithHolidaysVM
    {
        public List<AttendanceViewModel> Attendances { get; set; } = new List<AttendanceViewModel>();
        public int TotalRecords { get; set; }
        public List<Joblcoations> JoblocationList { get; set; } = new List<Joblcoations>();
        public List<Managers> ManagerList { get; set; } = new List<Managers>();


    }


    public class AttendanceDetailsVM
    {
        public string RecordType { get; set; }  // 
        // Attendance fields
        public long? ID { get; set; }
        public string? UserId { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? FirstLogDate { get; set; }
        public DateTime? LastLogDate { get; set; }
        public TimeSpan? HoursWorked { get; set; }
        public string AttendanceStatus { get; set; }

        // Holiday fields
        public string HolidayName { get; set; }
        public string Description { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Leave fields
        public long? EmployeeID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; }
        public long? LeaveTypeID { get; set; }
        public string? LeaveStatusID { get; set; }
        public string? DialerTime { get; set; }
        public string? Remarks { get; set; }
        public List<StatusChangeVM> StatusChanges { get; set; }=new List<StatusChangeVM>();
    }

    public class StatusChangeVM
    {
        public string? RecordType { get; set; }
        public long? EmployeeID { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime? WorkDate { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? Remarks { get; set; }
        public string? StatusState { get; set; }
        public DateTime? InsertedDate { get; set; }
        public string? InsertedByUserName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? UpdatedByUserName { get; set; }
        public string? ApprovedByUserName { get; set; }
    }


    public class AttendanceDetailsInputParams
    {
        public DateTime SelectedDate { get; set; }
        public string EmployeeNumber { get; set; }
        public long EmployeeId { get; set; }
    }

    public class SaveAttendanceStatus
    {
        public string? UserId { get; set; }
        public DateTime? WorkDate { get; set; }
        public long? EmployeeId { get; set; }
        public long? UserID { get; set; }
        public string? Remarks { get; set; }
        public string? DialerTime { get; set; }
        public string? AttendanceStatus { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public long? UpdatedByUserID { get; set; }

    }
    public class SaveTeamAttendanceStatus
    {
        public DateTime? WorkDate { get; set; }
        public long? EmployeeId { get; set; }
        public long? ManagerId { get; set; }
        public long? ID { get; set; }
        public long? UserID { get; set; }
        public string? Remarks { get; set; }
        public string? AttendanceStatus { get; set; }
        public bool? ApprovedByAdmin { get; set; }

        public int? ApprovedStatus { get; set; }
    }

    public class AttendanceRecordDto
    {
        public long? ID { get; set; }
        public long? EmployeeId { get; set; }
        public DateTime? WorkDate { get; set; }
        public string? Remarks { get; set; }
        public string? AttendanceStatus { get; set; }
        public bool? ApprovedByAdmin { get; set; }
        public int? ApprovedStatus { get; set; }

    }
}
