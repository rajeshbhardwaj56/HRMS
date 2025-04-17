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
        public int ID { get; set; } 
        public int Year { get; set; } 
        public int Month { get; set; } 
        public int? Day { get; set; } 
        public long UserId { get; set; }
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }

    }
    public class Attendance
    {
        public int? AttendanceStatusId { get; set; }
        public long ID { get; set; }
        public string? UserId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? FirstLogDate { get; set; } = DateTime.Now;
        public DateTime? LastLogDate { get; set; } = DateTime.Now;
        public int? HoursWorked { get; set; }
        public bool? IsManual { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? Comments { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<SelectListItem> Employeelist = new List<SelectListItem>();

    }
    public class AttendanceWithHolidays
    {
        public List<Attendance> Attendances { get; set; }
        public List<HolidayModel> Holidays { get; set; }
        public List<LeaveSummaryModel> Leaves { get; set; }
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
}
