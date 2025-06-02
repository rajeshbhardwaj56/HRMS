using HRMS.Models.Leave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.DashBoard
{

    public class DashBoardModelInputParams
    {
        public long EmployeeID { get; set; }
        public long RoleID { get; set; }
        public long JobLocationId { get; set; }
    }
    public class DashBoardModel
    {
        public long EmployeeID { get; set; }
        public Guid guid { get; set; }
        public long CompanyID { get; set; }
        public string? ProfilePhoto { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? Surname { get; set; } = string.Empty;
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long EmploymentDetailID { get; set; }
        public long DesignationID { get; set; }
        public long EmployeeTypeID { get; set; }
        public long PayrollTypeID { get; set; }
        public long DepartmentID { get; set; }
        public long JobLocationID { get; set; }
        public long ReportingToIDL1 { get; set; }
        public long ReportingToIDL2 { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string EmployeNumber { get; set; } = string.Empty;
        public string OfficialEmailID { get; set; } = string.Empty;
        public string OfficialContactNo { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
        public DateTime? JobSeprationDate { get; set; }
        public long NoOfEmployees { get; set; }
        public long NoOfCompanies { get; set; }
        public int CountsOfCompanies { get; set; }
        public long NoOfLeaves { get; set; }
        public double Salary { get; set; }
        public string? CorrespondenceAddress { get; set; } = string.Empty;
        public string? CorrespondenceCity { get; set; } = string.Empty;
        public string? CorrespondencePinCode { get; set; } = string.Empty;
        public string? CorrespondenceState { get; set; } = string.Empty;
        public long CorrespondenceCountryID { get; set; }
        public string? EmailAddress { get; set; } = string.Empty;
        public string? Landline { get; set; } = string.Empty;
        public string? Mobile { get; set; } = string.Empty;
        public string? Telephone { get; set; } = string.Empty;
        public string? PersonalEmailAddress { get; set; } = string.Empty;
        public string? PermanentAddress { get; set; } = string.Empty;
        public string? PermanentCity { get; set; } = string.Empty;
        public string? PermanentPinCode { get; set; } = string.Empty;
        public string? PermanentState { get; set; } = string.Empty;
        public long PermanentCountryID { get; set; }
        public string? PeriodOfStay { get; set; } = string.Empty;
        public string? VerificationContactPersonName { get; set; } = string.Empty;
        public string? VerificationContactPersonContactNo { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; } = string.Empty;
        public bool IsReferredByExistingEmployee { get; set; }
        public string? ReferredByEmployeeID { get; set; } = string.Empty;
        public string? BloodGroup { get; set; } = string.Empty;
        public string? PANNo { get; set; } = string.Empty;
        public string? AadharCardNo { get; set; } = string.Empty;
        public string? Allergies { get; set; } = string.Empty;
        public bool IsRelativesWorkingWithCompany { get; set; }
        public string? RelativesDetails { get; set; } = string.Empty;
        public string? MajorIllnessOrDisability { get; set; } = string.Empty;

        public string? AwardsAchievements { get; set; } = string.Empty;
        public string? EducationGap { get; set; } = string.Empty;

        public string? ExtraCuricuarActivities { get; set; } = string.Empty;
        public string? ForiegnCountryVisits { get; set; } = string.Empty;

        public string? ContactPersonName { get; set; } = string.Empty;
        public string? ContactPersonMobile { get; set; } = string.Empty;
        public string? ContactPersonTelephone { get; set; } = string.Empty;
        public string? ContactPersonRelationship { get; set; } = string.Empty;
        public string? ITSkillsKnowledge { get; set; } = string.Empty;
        public int HolidayCount { get; set; }
        public decimal TotalLeave { get; set; } 
        public long? LeavePolicyId { get; set; } 
        public long? CarryForword { get; set; } 

        public List<EmployeeDetails> EmployeeDetails { get; set; }
        public List<HolidayModel> HolidayList { get; set; }
        public List<LeaveSummaryModel> leavesSummary { get; set; } = new List<LeaveSummaryModel>();
        public List<AttendanceModel> AttendanceModel { get; set; } = new List<AttendanceModel>();
        public LeaveResults leaveResults { get; set; } = new LeaveResults();
        public List<WhatsHappening> WhatsHappening { get; set; }

    }
    public class EmployeeDetails
    {
        public long? EmployeeId { get; set; }
        public string FirstName { get; set; }

        public string MiddelName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public string EmployeePhoto { get; set; }
        public string? DesignationName { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class WhatsHappening
    {    
        public long? WhatsHappeningID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? IconImage { get; set; }
        
    }
    public class AttendanceModel
    {    
        public DateTime? Day { get; set; }
        public int? Present { get; set; }
        public int? Absent { get; set; }
 
        
    }

}
