using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.Models.DashBoard;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.ExportEmployeeExcel
{
    public class WeekOffUploadModel
    {
        public long? Id { get; set; }
        public string? EmployeeNumber { get; set; }
        public long? EmployeeId { get; set; }
        public string? EmployeeIdWithEmployeeNo { get; set; }
        public DateTime? WeekStartDate { get; set; }
        public DateTime? DayOff1 { get; set; }
        public DateTime? DayOff2 { get; set; }
        public DateTime? DayOff3 { get; set; }
       
        public DateTime? ModifiedDate { get; set; }
       
        public long? ModifiedBy { get; set; }
        public long? ShiftTypeId { get; set; }
        public int? TotalCount { get; set; }
        public string? ModifiedName { get; set; }
        public string? EmployeeName { get; set; }
        public string? EncryptedIdentity { get; set; }
        public string? EncryptedEmployeeId { get; set; }
        public string? EmployeeNumberWithOutAbbr { get; set; }
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
        public List<SelectListItem> Employee { get; set; } = new List<SelectListItem>();


    }

    public class WeekOffUploadModelList
    {
        public List<WeekOffUploadModel> WeekOffList { get; set; }
        public long CreatedBy { get; set; }
    }


    public class WeekOfInputParams
    {
        public int Id { get; set; }
        public long EmployeeID { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? PageNumber { get; set; } = 0;
        public int? PageSize { get; set; } = 0;
        public int? RoleId { get; set; } = 0;
        public string? SearchTerm { get; set; }
        public DateTime? WeekStartDate { get; set; }

        public string? SortCol { get; set; }      // @SortCol
        public string? SortDir { get; set; }
    }
    public class WeekOfEmployeeId
    {
        public long? EmployeeID { get; set; }
        public long? ManagerID { get; set; }
    }
    public class WeekOffUploadDeleteModel
    {
        public long RecordId { get; set; }
        public long ModifiedBy { get; set; }
    }
    
    public class UpcomingWeekOffRosterParams
    {
        public DateTime? WeekStartDate { get; set; }
    }

    public class UpcomingWeekOffRoster
    {
        public long ManagerID { get; set; }
        public long EmployeeID { get; set; }
        public string? ManagerName { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Employee { get; set; }
        public string? ManagerEmailID { get; set; }
        public int Level { get; set; }
        public string? Path { get; set; }
        public List<UpcomingWeekOffRoster> Subordinates { get; set; } = new List<UpcomingWeekOffRoster>();

    }

    public class EmployeeShiftModel
    {
        public long ShiftTypeID { get; set; }
        public string ShiftName { get; set; }
        public int IsSelected { get; set; }
    }
}
