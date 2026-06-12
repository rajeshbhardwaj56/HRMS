using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class DesignationModel
    {
        public long DesignationID { get; set; }

        public string? Name { get; set; }

        public long CompanyID { get; set; }

        public long DepartmentID { get; set; }
        public string? DepartmentName { get; set; }   


        public long HierarchyOrder { get; set; }

        public long? UserID { get; set; }

        public bool IsActive { get; set; }
        public string? EncodedId { get; set; }
        public List<SelectListItem> DepartmentList { get; set; } = new();

    }
    public class DesignationInputParams
    {
        public long CompanyID { get; set; }

        public long DesignationID { get; set; }
    }
    public class DuplicateDesignationResponse
    {
        public bool IsDuplicate { get; set; }
    }
}
