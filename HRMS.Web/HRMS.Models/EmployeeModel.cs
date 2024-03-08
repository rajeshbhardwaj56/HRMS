using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class EmployeeModel
    {
        public long EmployeeID { get; set; }
        public Guid guid { get; set; }
        public long EmployeeTypeID { get; set; }
        public long DepartmentID { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string CorrespondenceAddress { get; set; } = string.Empty;
        public string CorrespondenceCity { get; set; } = string.Empty;
        public string CorrespondencePinCode { get; set; } = string.Empty;
        public string CorrespondenceState { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Landline { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string PersonalEmailAddress { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public string PermanentCity { get; set; } = string.Empty;
        public string PermanentPinCode { get; set; } = string.Empty;
        public string PermanentState { get; set; } = string.Empty;
        public string PeriodOfStay { get; set; } = string.Empty;
        public string VerificationContactPersonName { get; set; } = string.Empty;
        public string VerificationContactPersonContactNo { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string PlaceOfBirth { get; set; } = string.Empty;
        public string ReferredByExistingEmployee { get; set; } = string.Empty;
        public string ReferredByEmployeeID { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string AadharCardNo { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string RelativesWorkingWithCompany { get; set; } = string.Empty;
        public string RelativesDetails { get; set; } = string.Empty;
        public string MajorIllnessOrDisability { get; set; } = string.Empty;
        public long InsertedByUserID { get; set; }
        public long UpdatedByUserID { get; set; }
        public DateTime InsertedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<FamilyDetails> FamilyDetails { get; set; } = new List<FamilyDetails>();

    }

    public class FamilyDetails
    {
        public long EmployeesFamilyDetailID { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
