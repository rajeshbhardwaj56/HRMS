using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.ImportFromExcel
{
    public class ImportEmployeeDetail
    {
        public long? EmployeeID { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Surname { get; set; }
        public string? CorrespondenceAddress { get; set; }
        public string? CorrespondenceCity { get; set; }
        public string? CorrespondencePinCode { get; set; }
        public string? CorrespondenceState { get; set; }
        public string? CorrespondenceCountryName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Landline { get; set; }
        public string? Mobile { get; set; }
        public string? Telephone { get; set; }
        public string? PersonalEmailAddress { get; set; }
        public string? PermanentAddress { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentPinCode { get; set; }
        public string? PermanentState { get; set; }
        public string? PermanentCountryName { get; set; }
        public string? PeriodOfStay { get; set; }
        public string? VerificationContactPersonName { get; set; }
        public string? VerificationContactPersonContactNo { get; set; }
        public string? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public bool ? IsReferredByExistingEmployee { get; set; }
        public string? ReferredByEmployeeName { get; set; }
        public string? BloodGroup { get; set; }
        public string? AadharCardNo { get; set; }
        public string? PANNo { get; set; }
        public string? Allergies { get; set; }
        public bool  ? IsRelativesWorkingWithCompany { get; set; }
        public string? RelativesDetails { get; set; }
        public string? MajorIllnessOrDisability { get; set; }
        public string? AwardsAchievements { get; set; }
        public string? EducationGap { get; set; }
        public string? ExtraCurricularActivities { get; set; }
        public string? ForeignCountryVisits { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }
        public string? ContactPersonTelephone { get; set; }
        public string? ContactPersonRelationship { get; set; }
        public string? ITSkillsKnowledge { get; set; }
        public string? JoiningDate { get; set; } 
        public string? DesignationName { get; set; }
        public string? EmployeeType { get; set; }
        public string? DepartmentName { get; set; }
        public string? SubDepartmentName { get; set; }
        public string? ShiftTypeName { get; set; }
        public string? JobLocationName { get; set; }
        public string? ReportingToIDL1Name { get; set; }
        public string? ReportingToIDL2Name { get; set; }
        public string? OfficialEmailID { get; set; }
        public string? OfficialContactNo { get; set; }
        public string? PayrollTypeName { get; set; }
        public string? LeavePolicyName { get; set; }
        public string? ClientName { get; set; }
        public string? RoleName { get; set; }
        public string? ForiegnCountryVisits { get; set; }
        public string? ExtraCuricuarActivities { get; set; }
        public string? Gender { get; set; }
        public string? ESINumber { get; set; }
       // public string ESINumber { get; set; } = string.Empty;
        public string? RegistrationDateInESIC { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IFSCCode { get; set; }
        public string? BankName { get; set; }
        public string? DOJInTraining { get; set; }
        public string? DOJOnFloor { get; set; }
        public string? DOJInOJTOnroll { get; set; }
        public string? DateOfResignation { get; set; }
        public string? DateOfLeaving { get; set; }
        public string? BackOnFloor { get; set; }
        public string? LeavingType { get; set; }
        public string? LeavingRemarks { get; set; }
        public string? NoticeServed { get; set; }
        public string? MailReceivedFromAndDate { get; set; }
        public string? DateOfEmailSentToITForIDDeletion { get; set; }
        public string? AON  { get; set; }
        public string? PreviousExperience  { get; set; }

        public List<ImportEmployeeDetail> ImportEmployeeDetails { get; set; }
    }

    public class ImportEducationalDetail
    {
        public long EducationDetailID { get; set; }
        public long EducationID { get; set; }
        public string School_University { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string YearOfPassing { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
        public string Major_OptionalSubjects { get; set; } = string.Empty;

    }

    public class ImportFamilyDetail
    {
        public long EmployeesFamilyDetailID { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class ImportLanguageDetail
    {
        public long LanguageDetailID { get; set; }
        public long LanguageID { get; set; }
        public bool IsSpeak { get; set; }
        public bool IsRead { get; set; }
        public bool IsWrite { get; set; }
    }

    public class ImportReference
    {
        public long ReferenceDetailID { get; set; }
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]    
        public string Contact { get; set; } = string.Empty;
        public string OrgnizationName { get; set; } = string.Empty;
        public string RelationWithCandidate { get; set; } = string.Empty;
    }

    public class ImportEmploymentHistory
    {
        public long EmploymentHistoryID { get; set; }
        public long EmployeeID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string EmploymentID { get; set; } = string.Empty;
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public long CountryID { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string ReasionFoLeaving { get; set; } = string.Empty;
        public string Designition { get; set; } = string.Empty;
        public string GrossSalary { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;
        public string SupervisorDesignition { get; set; } = string.Empty;
        public string SupervisorContactNo { get; set; } = string.Empty;
        public string HRName { get; set; } = string.Empty;
        public string HREmail { get; set; } = string.Empty;
        public string HRContactNo { get; set; } = string.Empty;
    }
}

