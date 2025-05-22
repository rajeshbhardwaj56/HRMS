using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.ImportFromExcel
{
    public class ImportExcelDataTable
    {
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
        public string? IsReferredByExistingEmployee { get; set; }
        public string? ReferredByEmployeeName { get; set; }
        public string? BloodGroup { get; set; }
        public string? AadharCardNo { get; set; }
        public string? PANNo { get; set; }
        public string? Allergies { get; set; }
        public string? IsRelativesWorkingWithCompany { get; set; }
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
        public string? RegistrationDateInESIC { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? UANNumber { get; set; }
        public string? IFSCCode { get; set; }
        public string? BankName { get; set; }
        public string? DOJInTraining { get; set; }
        public string? DOJOnFloor { get; set; }
        public string? DOJInOJT { get; set; }
        public string? DOJInOnroll { get; set; }
        public string? DateOfResignation { get; set; }
        public string? DateOfLeaving { get; set; }
        public string? BackOnFloor { get; set; }
        public string? LeavingType { get; set; }
        public string? LeavingRemarks { get; set; }
        public string? NoticeServed { get; set; }
        public string? MailReceivedFromAndDate { get; set; }
        public string? DateOfEmailSentToITForIDDeletion { get; set; }
        public string? AON { get; set; }
        public string? PreviousExperience { get; set; }
        public string? Status { get; set; }
        public string? ExcelFile { get; set; }
        public string? InsertedByUserID { get; set; }
    }
    public class ImportExcelDataTableType
    {
        public long? EmployeeID { get; set; }
        public long? CompanyID { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Surname { get; set; }
        public string? CorrespondenceAddress { get; set; }
        public string? CorrespondenceCity { get; set; }
        public string? CorrespondencePinCode { get; set; }
        public string? CorrespondenceState { get; set; }
        public long? CorrespondenceCountryID { get; set; }
        public string? EmailAddress { get; set; }
        public string? Landline { get; set; }
        public string? Mobile { get; set; }
        public string? Telephone { get; set; }
        public string? PersonalEmailAddress { get; set; }
        public string? PermanentAddress { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentPinCode { get; set; }
        public string? PermanentState { get; set; }
        public long? PermanentCountryID { get; set; }
        public string? PeriodOfStay { get; set; }
        public string? VerificationContactPersonName { get; set; }
        public string? VerificationContactPersonContactNo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public bool? IsReferredByExistingEmployee { get; set; }
        public string? ReferredByEmployeeID { get; set; }
        public string? BloodGroup { get; set; }
        public string? PANNo { get; set; }
        public string? AadharCardNo { get; set; }
        public string? Allergies { get; set; }
        public bool? IsRelativesWorkingWithCompany { get; set; }
        public string? RelativesDetails { get; set; }
        public string? MajorIllnessOrDisability { get; set; }
        public string? AwardsAchievements { get; set; }
        public string? EducationGap { get; set; }
        public string? ExtraCuricuarActivities { get; set; }
        public string? ForiegnCountryVisits { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }
        public string? ContactPersonTelephone { get; set; }
        public string? ContactPersonRelationship { get; set; }
        public string? ITSkillsKnowledge { get; set; }
        public long? InsertedByUserID { get; set; }
        public long? LeavePolicyID { get; set; }
        public long? CarryForword { get; set; }
        public int? Gender { get; set; }
        public string? AadhaarCardImage { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public int RoleID { get; set; }
        public string? EmployeNumber { get; set; }
        public long? DesignationID { get; set; }
        public long? EmployeeTypeID { get; set; }
        public long? DepartmentID { get; set; }
        public long? JobLocationID { get; set; }
        public string? OfficialEmailID { get; set; }
        public string? OfficialContactNo { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? JobSeprationDate { get; set; }
        public long? ReportingToIDL1 { get; set; }
        public long? PayrollTypeID { get; set; }
        public long? ReportingToIDL2 { get; set; }
        public string? ClientName { get; set; }
        public long? SubDepartmentID { get; set; }
        public long? ShiftTypeID { get; set; }
        public string? ESINumber { get; set; }
        public DateTime? ESIRegistrationDate { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? UANNumber { get; set; }
        public string? IFSCCode { get; set; }
        public string? BankName { get; set; }
        public int? AgeOnNetwork { get; set; }
        public int? NoticeServed { get; set; }
        public string? LeavingType { get; set; }
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
        public bool? IsActive { get; set; }
        public  string? ReportingToIDL1EmployeeNumber { get; set; }
    }

    public class CompanyInfo
    {
        public string? Abbr { get; set; }
        public long? CompanyID { get; set; }
    }   
    public class BulkEmployeeImportModel
    {
        public List<ImportExcelDataTable> Employees { get; set; }
    }  
}
