using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.TeamAlignment
{
    public class TeamAlignmentInputParams
    {
        public long EmployeeID { get; set; }
        public long RoleID { get; set; }
    }

    public class TeamAlignmentModel
    {
        public List<TeamAlignmentEmployee> TeamHierarchy { get; set; } = new();
    }

    public class TeamAlignmentEmployee
    {
        public string? EmployeNumber { get; set; }
        public long EmployeeID { get; set; }
        public long? ManagerID { get; set; }

        public string? EmployeeName { get; set; }
        public string? ManagerLevel1Name { get; set; }
        public string? ManagerLevel2Name { get; set; }
        public string? ManagerName { get; set; }

        public string? Designation { get; set; }
        public string? Department { get; set; }

        public string? ProfilePhoto { get; set; }

        public int Level { get; set; }
        public string? Path { get; set; }

        public int DirectSubordinateCount { get; set; }
        public int TotalSubordinateCount { get; set; }

        public int RoleID { get; set; }

        public List<TeamAlignmentEmployee> Subordinates { get; set; } = new();
    }
}
