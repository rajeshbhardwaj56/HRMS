using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Common
{
    public class Joblcoations
    {
        public long? JobLocationID { get; set; }
        public long? CompanyId { get; set; }
        public string? JobLocationName { get; set; }
        
    }
    public class SubDepartment
    {
        public long SubDepartmentID { get; set; }
        public string? SubDepartmentName { get; set; }
    }

    public class CompanyFilterResponse
    {
        public List<Joblcoations> JobLocations { get; set; } = new();
        public List<SubDepartment> SubDepartments { get; set; } = new();
    }
}
