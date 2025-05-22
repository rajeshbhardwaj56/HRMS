using HRMS.Models.Common;
using HRMS.Models.Employee;
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
}