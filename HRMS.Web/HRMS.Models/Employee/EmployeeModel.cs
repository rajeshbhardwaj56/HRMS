using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Models.Employee
{
    public class EmployeeInputParams
    {
        public long LeaveSummaryID { get; set; }
        public long CompanyID { get; set; }
        public long EmployeeID { get; set; }
    }


    public class EmployeeModel
    {
        public string EncryptedIdentity { get; set; } = string.Empty;
        public long EmployeeID { get; set; }
        public Guid guid { get; set; }
        public long CompanyID { get; set; } = 1;
        public string? ProfilePhoto { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? Surname { get; set; } = string.Empty;
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
        public long InsertedByUserID { get; set; }
        public long UpdatedByUserID { get; set; }
        public DateTime InsertedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long? LeavePolicyID { get; set; } = 0;

        // Employment Details
        public long DesignationID { get; set; }
        public long EmployeeTypeID { get; set; }
        public long DepartmentID { get; set; }
        public long JobLocationID { get; set; }
        public long ReportingToID { get; set; }
        public string? DesignationName { get; set; } = string.Empty;
        public string? EmployeeTypeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; } = string.Empty;
        public string? JobLocationName { get; set; } = string.Empty;
        public string? ReportingToName { get; set; } = string.Empty;
        public string? EmployeNumber { get; set; } = string.Empty;
        public string OfficialEmailID { get; set; } = string.Empty;

        // Additional Details
        public List<FamilyDetail> FamilyDetails { get; set; } = new List<FamilyDetail>();
        public List<EducationalDetail> EducationalDetails { get; set; } = new List<EducationalDetail>();
        public List<LanguageDetail> LanguageDetails { get; set; } = new List<LanguageDetail>();
        public List<EmploymentHistory> EmploymentHistory { get; set; } = new List<EmploymentHistory>();
        public List<Reference> References { get; set; } = new List<Reference>();

        // Master Details
        public List<SelectListItem> Languages = new List<SelectListItem>();
        public List<SelectListItem> Educations = new List<SelectListItem>();
        public List<SelectListItem> Countries = new List<SelectListItem>();
        public List<SelectListItem> EmploymentTypes = new List<SelectListItem>();
        public List<SelectListItem> Departments = new List<SelectListItem>();
        public List<SelectListItem> leavePolicies { get; set; } = new List<SelectListItem>();

    }
}
