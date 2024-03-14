using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class Reference
    {
        public long ReferenceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string OrgnizationName { get; set; } = string.Empty;
        public string RelationWithCandidate { get; set; } = string.Empty;
    }
}
