using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.DashBoard
{

    public class DashBoardModelInputParams
    {
        public long EmployeeID { get; set; }
    }
    public class DashBoardModel
    {
        public long EmployeeID { get; set; }
        public Guid guid { get; set; }
        public long CompanyID { get; set; }
        public string? ProfilePhoto { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? Surname { get; set; } = string.Empty;
        public long NoOfEmployees { get; set; }
        public long NoOfCompanies { get; set; }
        public long NoOfLeaves { get; set; }
        public double Salary { get; set; }
    }
}
