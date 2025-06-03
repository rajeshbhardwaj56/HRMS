using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class ReferenceParams
    {
        public long EmployeeID { get; set; }
        public long ReferenceDetailID { get; set; }
    }
    public class Reference
    {
        public long ReferenceDetailID { get; set; }
        public long EmployeeID { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Contact { get; set; } = string.Empty;
        public string OrgnizationName { get; set; } = string.Empty;
        public string RelationWithCandidate { get; set; } = string.Empty;
        public long UserID { get; set; }
    }
}
