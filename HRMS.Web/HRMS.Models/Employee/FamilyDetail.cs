using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class FamilyDetail
    {
        public long EmployeesFamilyDetailID { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
