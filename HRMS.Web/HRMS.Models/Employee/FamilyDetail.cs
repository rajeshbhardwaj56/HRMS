using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.Employee
{
    public class FamilyDetailParams
    {
        public long EmployeeID { get; set; }
        public long EmployeesFamilyDetailID { get; set; }
    }
    public class FamilyDetail
    {
        public long? EmployeesFamilyDetailID { get; set; }
        public long EmployeeID { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string? Details { get; set; } = string.Empty;
        public long UserID { get; set; }


    }
}
