using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class SubDepartmentModel
    {
        public long SubDepartmentID { get; set; }

        public long DepartmentID { get; set; }

        public string? Name { get; set; }
        public string? DepartmentName { get; set; }

        public long CompanyID { get; set; }

        public long? UserID { get; set; }

        public bool IsActive { get; set; }
        public string? EncodedId { get; set; }
        public List<SelectListItem> DepartmentList { get; set; } = new();
    }
    public class SubDepartmentInputParams
    {
        public long CompanyID { get; set; }

        public long SubDepartmentID { get; set; }
    }
    public class DuplicateSubDepartmentResponse
    {
        public bool IsDuplicate { get; set; }
    }
}
