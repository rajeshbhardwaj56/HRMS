using HRMS.Models.Company;
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
        public long Id { get; set; }
        public long CompanyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<CompanyModel> Companies { get; set; }
    }
}
