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
        public long DesignationID { get; set; }
        public long DepartmentID { get; set; }

    }

    public class EmploymentDetail
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long EmploymentDetailID { get; set; }
        public long EmployeeID { get; set; }
        public long DesignationID { get; set; }
        public long EmployeeTypeID { get; set; }
        public long DepartmentID { get; set; }
        public long SubDepartmentID { get; set; }
        public long ShiftTypeID { get; set; }
        public long JobLocationID { get; set; }
        public long ReportingToIDL1 { get; set; }
        public string OfficialEmailID { get; set; } = string.Empty;
        public string OfficialContactNo { get; set; } = string.Empty;
        public string DesignationName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string SubDepartmentName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string OfficeLocation { get; set; } = string.Empty;
        public string EmployeeType { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; } = DateTime.UtcNow;
        public DateTime? JobSeprationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long UserID { get; set; }
        public DateTime? InsertedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;
        public long PayrollTypeID { get; set; }
        public long LeavePolicyID { get; set; }
        public long ReportingToIDL2 { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ESINumber { get; set; } = string.Empty;
        public DateTime? ESIRegistrationDate { get; set; } = DateTime.UtcNow;
        public int RoleId { get; set; }
        public string EmployeNumber { get; set; } = string.Empty;
        public string CompanyAbbr { get; set; } = string.Empty;
        public long CompanyID { get; set; } = 0;


        public List<SelectListItem> EmploymentTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PayrollTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> JobLocations { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Designations { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> EmployeeList { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> LeavePolicyList { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ShiftTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SubDepartments { get; set; } = new List<SelectListItem>();

    }

    public class EmployeeListModel
    {
        public long EmployeeID { get; set; }
        public string Name { get; set; }
    }

    public class L2ManagerInputParams
    {
        public int L1EmployeeID { get; set; }
    }
    public class L2ManagerDetail
    {
        public long ManagerID { get; set; }
        public string? ManagerName { get; set; }
        public string? EmployeNumber { get; set; }
    }
}
