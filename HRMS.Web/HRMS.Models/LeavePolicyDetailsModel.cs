using HRMS.Models.Company;
using HRMS.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class LeavePolicyDetailsInputParams
    {
        public long Id { get; set; }
        public long CompanyID { get; set; }
      
    }
    public class LeavePolicyDetailsModel
    {
        public string? EncodedId { get; set; }
        public long Id { get; set; }
        public long CompanyID { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string? PolicyDocument { get; set; }
        public long PolicyCategoryId { get; set; }
        public string? PolicyCategoryName { get; set; }
        public List<CompanyModel> Companies { get; set; }
        public List<PolicyCategoryModel> PolicyList { get; set; } = new List<PolicyCategoryModel>();
    }
}
