using HRMS.Models.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class PolicyCategoryInputParams
    {
        public long Id { get; set; }
        public long CompanyID { get; set; }
        public long EmployeeID { get; set; }

    }
    public class PolicyCategoryModel
    {
        public long Id { get; set; }
        public string? EncodedId { get; set; }
        public long CompanyID { get; set; }
        public string Name { get; set; }
        public List<CompanyModel> Companies { get; set; }
    }
}
