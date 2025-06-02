using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.ExportEmployeeExcel
{
    public class ExportEmployeeDetailsExcel
    {
        public string EmployeeNumber { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string EmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string Mobile { get; set; }
        public string Landline { get; set; }
        public string Telephone { get; set; }
        public string CorrespondenceAddress { get; set; }
        public string CorrespondenceCity { get; set; }
        public string CorrespondenceState { get; set; }
        public string CorrespondencePinCode { get; set; }
        public string PermanentAddress { get; set; }
        public string PermanentCity { get; set; }
        public string PermanentState { get; set; }
        public string PermanentPinCode { get; set; }
        public string PANNo { get; set; }
        public string AadharCardNo { get; set; }
        public string BloodGroup { get; set; }
        public string Allergies { get; set; }
        public string MajorIllnessOrDisability { get; set; }
        public string AwardsAchievements { get; set; }
        public string EducationGap { get; set; }
        public string ExtraCuricuarActivities { get; set; }
        public string ForiegnCountryVisits { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ContactPersonTelephone { get; set; }
        public string ContactPersonRelationship { get; set; }
        public string ITSkillsKnowledge { get; set; }

        // Employment Info
        public string Designation { get; set; }
        public string EmployeeType { get; set; }
        public string Department { get; set; }
        public string SubDepartment { get; set; }
        public string JobLocation { get; set; }
        public string ShiftType { get; set; }
        public string OfficialEmailID { get; set; }
        public string OfficialContactNo { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? JobSeprationDate { get; set; }
        public string ReportingManager { get; set; }
        public string PolicyName { get; set; }
        public string PayrollType { get; set; }
        public string ClientName { get; set; }
        public string ESINumber { get; set; }
        public DateTime? ESIRegistrationDate { get; set; }
        // Bank Info
        public string BankAccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BankName { get; set; }
        public string UANNumber { get; set; }

        // Separation Info
        public DateTime? DateOfResignation { get; set; }
        public DateTime? DateOfLeaving { get; set; }
        public string LeavingType { get; set; }
        public int? NoticeServed { get; set; }
        public int? AgeOnNetwork { get; set; }
        public string? PreviousExperience { get; set; }
        public DateTime? DateOfJoiningTraining { get; set; }
        public DateTime? DateOfJoiningFloor { get; set; }
        public DateTime? DateOfJoiningOJT { get; set; }
        public DateTime? DateOfJoiningOnroll { get; set; }
        public DateTime? BackOnFloorDate { get; set; }
        public string LeavingRemarks { get; set; }
        public DateTime? MailReceivedFromAndDate { get; set; }
        public DateTime? EmailSentToITDate { get; set; }
    }
}
