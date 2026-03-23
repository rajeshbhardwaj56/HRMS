using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.Employee
{
    public class EmploymentSeparationInputParams
    {
        public long EmployeeID { get; set; }
        public long UserID { get; set; }

    }
    public class EmploymentSeparationDetail
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long EmployeeSeparationID { get; set; }
        public long EmployeeID { get; set; }
        public int? AgeOnNetwork { get; set; }
        public string? PreviousExperience { get; set; }
        public DateTime? DateOfJoiningTraining { get; set; }
        public DateTime? DateOfJoiningFloor { get; set; }
        public DateTime? DateOfJoiningOJT { get; set; }
        public DateTime? DateOfJoiningOnroll { get; set; }
        public DateTime? DateOfResignation { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public DateTime? BackOnFloorDate { get; set; }
        public string? LeavingRemarks { get; set; }
        public string? MailReceivedFromAndDate { get; set; }
        public DateTime? EmailSentToITDate { get; set; }   
        public long UserID { get; set; }    
        public string? LeavingType { get; set; }
        public int? NoticeServed { get; set; }
    }
}
