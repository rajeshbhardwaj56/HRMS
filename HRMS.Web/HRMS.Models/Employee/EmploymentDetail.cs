using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class EmploymentDetailInputParams
    {
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }
    }

    public class EmploymentDetail
    {
        public long EmploymentDetailID { get; set; }
        public long EmployeeID { get; set; }
        public long DesignationID { get; set; }
        public long EmployeeTypeID { get; set; }
        public long DepartmentID { get; set; }
        public long JobLocationID { get; set; }
        public long ReportingToID { get; set; }
        public string OfficialEmailID { get; set; } = string.Empty;
        public string OfficialContactNo { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
        public DateTime? JobSeprationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long UserID { get; set; }
        public DateTime? InsertedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public List<SelectListItem> EmploymentTypes = new List<SelectListItem>();
        public List<SelectListItem> Departments = new List<SelectListItem>();
        public List<SelectListItem> JobLocations = new List<SelectListItem>();
        public List<SelectListItem> Designations = new List<SelectListItem>();
        public List<EmployeeListModel> EmployeeList = new List<EmployeeListModel>();
    }

    public class EmployeeListModel
    {
        public long EmployeeID { get; set; }
        public Guid guid { get; set; }
    }
}
