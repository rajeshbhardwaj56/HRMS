using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class LOBModel
    {
        public long LOBID { get; set; }

        public string? Name { get; set; }

        public long CompanyID { get; set; }

        public long? UserID { get; set; }

        public bool IsActive { get; set; } 
        public string? EncodedId { get; set; }
        public List<SelectListItem> DepartmentList { get; set; } = new();
    }
    public class LOBInputParams
    {
        public long CompanyID { get; set; }

        public long LOBID { get; set; }
    }
    public class DuplicateLOBResponse
    {
        public bool IsDuplicate { get; set; }
    }
}
