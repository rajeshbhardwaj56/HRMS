using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.ExportEmployeeExcel
{
    public class WeekOffUploadModel
    {
        public int? Id { get; set; }
        public string? EmployeeNumber { get; set; }
        public long? EmployeeId { get; set; }
        public DateTime? WeekOff1 { get; set; }
        public DateTime? WeekOff2 { get; set; }
        public DateTime? WeekOff3 { get; set; }
        public DateTime? WeekOff4 { get; set; }
        public DateTime? WeekOff5 { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? RosterMonth { get; set; }
        public long? ModifiedBy { get; set; }
        public int? TotalCount { get; set; }
        public string? ModifiedName { get; set; }
        public string? EmployeeName { get; set; }
        public string? EncryptedIdentity { get; set; }
        public string? EncryptedEmployeeId { get; set; }
        public string? EmployeeNumberWithOutAbbr { get; set; }
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
        public string? SearchTerm { get; set; }
    }
    public class WeekOfEmployeeId
    {
        public long EmployeeID { get; set; }
    }

    }
