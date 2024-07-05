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
        public long UserID { get; set; }
    }

    public class EmploymentDetail
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
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
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public long UserID { get; set; }
        public DateTime? InsertedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public long LeavePolicyID { get; set; }

        public List<SelectListItem> EmploymentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> JobLocations { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Designations { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> EmployeeList { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> LeavePolicyList { get; set; } = new List<SelectListItem>();
    }

    public class EmployeeListModel
    {
        public long EmployeeID { get; set; }
        public string Name { get; set; }
    }
}
