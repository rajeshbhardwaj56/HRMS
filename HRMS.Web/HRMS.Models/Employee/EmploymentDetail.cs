using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class EmploymentDetail
    {
        public long EmploymentDetailID { get; set; }
        public long EmployeeID { get; set; }
        public long DesignationID { get; set; }
        public long EmployeeTypeID { get; set; }
        public long DepartmentID { get; set; }
        public long JobLocationID { get; set; }
        public long ReportingToID { get; set; }
        public string OfficialEmailID { get; set; } = string.Empty;
        public string OfficialContactNo { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
        public DateTime? JobSeprationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long UserID { get; set; }
        public DateTime? InsertedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}
