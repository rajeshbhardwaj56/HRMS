using HRMS.Models.Leave;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class AttandanceInputParams
    {
        public int ID { get; set; } 
        public int Year { get; set; } 
        public int Month { get; set; } 
        public long UserId { get; set; }

    }
    public class Attandance
    {
        public long ID { get; set; }
        public string? UserId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? WorkDate { get; set; }
        public DateTime? FirstLogDate { get; set; }
        public DateTime? LastLogDate { get; set; }
        public int? HoursWorked { get; set; }
        public List<SelectListItem> Employeelist = new List<SelectListItem>();

    }
    public class AttendanceWithHolidays
    {
        public List<Attandance> Attendances { get; set; }
        public List<HolidayModel> Holidays { get; set; }
        public List<LeaveSummaryModel> Leaves { get; set; }
    }
}
