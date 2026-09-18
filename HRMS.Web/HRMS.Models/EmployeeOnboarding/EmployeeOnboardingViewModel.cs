using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace HRMS.Models.EmployeeOnboarding
{
    public class EmployeeOnboardingViewModel
    {
        public long OnboardingID { get; set; }

        public string? CompanyLogo { get; set; }

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(200, MinimumLength = 2,
            ErrorMessage = "Full name must be between 2 and 200 characters.")]
        [RegularExpression(
            @"^[a-zA-Z\s.'-]+$",
            ErrorMessage = "Full name can contain only letters, spaces, apostrophes, dots and hyphens.")]
        public string? FullName { get; set; }


        [Required(ErrorMessage = "Please enter your email address.")]
        [StringLength(200,
            ErrorMessage = "Email address cannot exceed 200 characters.")]
        [EmailAddress(
            ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Please enter your mobile number.")]
        [RegularExpression(
            @"^[6-9][0-9]{9}$",
            ErrorMessage = "Please enter a valid 10-digit mobile number.")]
        public string? Mobile { get; set; }


        public long? EmployeeID { get; set; }

        public string? EmpCode { get; set; }

        public int CurrentStep { get; set; } = 1;

        public string Status { get; set; } = "In Progress";

        public bool IsSubmitted { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
    public class EmployeeDetailsOnboardingModel
    {
        public long EmployeeDetailsOnboardingID { get; set; }

        public long OnboardingID { get; set; }
        public string? Key { get; set; }
        public int CurrentStep { get; set; }

        public int LastSavedStep { get; set; }
        public string? Status { get; set; }

        public bool IsSubmitted { get; set; }
        // HR / Admin
        public string? Designation { get; set; }
        public string? EmployeeCode { get; set; }

        // Personal
        public string? FatherHusbandName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? BloodGroup { get; set; }
        public string? EmployeePhoto { get; set; }

        [JsonIgnore]
        public IFormFile? EmployeePhotoFile { get; set; }
        // Permanent Address
        public string? PermanentAddress { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentState { get; set; }
        public string? PermanentPINCode { get; set; }

        // Present Address
        public string? PresentAddress { get; set; }
        public string? PresentCity { get; set; }
        public string? PresentState { get; set; }
        public string? PresentPINCode { get; set; }

        // Contact
        public string? LandlineNo { get; set; }

        // Emergency
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? EmergencyContactMobile { get; set; }
        public string? EmergencyContactAddress { get; set; }
        public string? EmergencyContactCity { get; set; }

        // Hiring
        public string? SourceOfHiring { get; set; }

        // Reference 1
        public string? Reference1Name { get; set; }
        public string? Reference1Designation { get; set; }
        public string? Reference1Company { get; set; }
        public string? Reference1Mobile { get; set; }
        public string? Reference1Email { get; set; }

        // Reference 2
        public string? Reference2Name { get; set; }
        public string? Reference2Designation { get; set; }
        public string? Reference2Company { get; set; }
        public string? Reference2Mobile { get; set; }
        public string? Reference2Email { get; set; }

        // Other
        public string? PassportNumber { get; set; }
        public string? PANNumber { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? Religion { get; set; }

        // Declaration
        public bool? PreviouslyInterviewed { get; set; }
        public DateTime? PreviousInterviewDate { get; set; }
        public string? PreviousInterviewPlace { get; set; }
        public string? PreviousPositionApplied { get; set; }

        public bool? PreviouslyWorked { get; set; }
        public DateTime? PreviousWorkFromDate { get; set; }
        public DateTime? PreviousWorkToDate { get; set; }
        public string? PreviousWorkLocation { get; set; }

        public bool? HaveCriminalCivilProceedings { get; set; }
        public string? CriminalCivilProceedingsDetails { get; set; }
        public string? OtherInformation { get; set; }

        public string? EmployeeSignature { get; set; }
        public DateTime? DeclarationDate { get; set; }
        public string? DeclarationPlace { get; set; }

        // Office Use
        public string? InterviewerName { get; set; }
        public DateTime? DateOfJoining { get; set; }

        public string? Department { get; set; }


        public string? Location { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? RefCheckReport { get; set; }

        public string? DocumentsCheckedBy { get; set; }
        public string? OfficeUseSignature { get; set; }

        public string? FunctionalHead { get; set; }
        public string? SPOCHumanResources { get; set; }
        public string? HeadHumanResources { get; set; }

        public string? ApprovalsRemarks { get; set; }

        public long? CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }
        public EmployeeDetailsOnboardingModel? EmployeeDetails { get; set; }

        // Multiple Family records
        public List<EmployeeOnboardingFamilyModel> FamilyList { get; set; }
            = new List<EmployeeOnboardingFamilyModel>();

        // Multiple Education records
        public List<EmployeeOnboardingEducationModel> EducationList { get; set; }
            = new List<EmployeeOnboardingEducationModel>();

        // Multiple Previous Employment records
        public List<EmployeeOnboardingEmploymentModel> EmploymentList { get; set; }
            = new List<EmployeeOnboardingEmploymentModel>();
        public List<EmployeeOnboardingDocumentsModel> DocumentsList { get; set; }
    = new List<EmployeeOnboardingDocumentsModel>();
        public List<EmployeeOnboardingBankDetailsModel> BankDetailsList { get; set; }
= new List<EmployeeOnboardingBankDetailsModel>();
        public List<EmployeeOnboardingNomineeModel> EPFNomineeList { get; set; }
            = new List<EmployeeOnboardingNomineeModel>();

        public List<EmployeeOnboardingNomineeModel> EPSNomineeList { get; set; }
            = new List<EmployeeOnboardingNomineeModel>();

        public List<EmployeeOnboardingNomineeModel> GratuityNomineeList { get; set; }
            = new List<EmployeeOnboardingNomineeModel>();

        public List<EmployeeOnboardingInterviewEvaluationModel> InterviewEvaluationList { get; set; }
            = new List<EmployeeOnboardingInterviewEvaluationModel>();
        // =========================================================
        // DOCUMENT UPLOADS
        // =========================================================

        public List<IFormFile>? EducationDocuments { get; set; }

        public IFormFile? BankDocumentFile { get; set; }
        // =========================================================
        // INTERVIEWER SIGNATURES
        // =========================================================

        public IFormFile? HRInterviewerSignatureFile { get; set; }

        public IFormFile? OperationsInterviewerSignatureFile { get; set; }

        public IFormFile? ProcessOwnerInterviewerSignatureFile { get; set; }

        public IFormFile? InterviewerSignatureFile { get; set; }

        public IFormFile? SignatureFile { get; set; }
        // =========================================================
        // APPROVAL SIGNATURES
        // =========================================================

        public IFormFile? FunctionalHeadApprovalFile { get; set; }

        public IFormFile? SPOCHumanResourcesApprovalFile { get; set; }

        public IFormFile? HeadHumanResourcesApprovalFile { get; set; }
        public IFormFile? PassportFile { get; set; }

        public IFormFile? PANFile { get; set; }

        public IFormFile? AadhaarFile { get; set; }

        public IFormFile? EmployeeSignatureFile { get; set; }
        public string? PassportDocumentPath { get; set; }

        public string? PANDocumentPath { get; set; }

        public string? AadhaarDocumentPath { get; set; }

        public string? EmployeeSignaturePath { get; set; }
    }
    public class EmployeeOnboardingFamilyModel
    {
        public long FamilyID { get; set; }

        public long OnboardingID { get; set; }

        public string? Relation { get; set; }

        public string? Name { get; set; }

        public string? Gender { get; set; }

        public string? Age { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Occupation { get; set; }

        public bool? IsDependent { get; set; } = false;
        public string? Mobile { get; set; }
        public bool? ResidingWithEmployee { get; set; } = true;

        public string? Address { get; set; }
        public string? Town { get; set; }
        public string? State { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }
    }
    public class EmployeeOnboardingEducationModel
    {
        public long EducationID { get; set; }

        public long OnboardingID { get; set; }

        public string? EducationLevel { get; set; }

        public string? Stream { get; set; }

        public int? PassingYear { get; set; }

        public string? InstitutionName { get; set; }

        public string? BoardUniversity { get; set; }

        public string? MarksObtained { get; set; }

        public string? MajorSubjects { get; set; }

        public string? DocumentType { get; set; }

        public IFormFile? DocumentFile { get; set; }

        public int RowNo { get; set; }
    }
    public class EmployeeOnboardingEmploymentModel
    {
        public long EmploymentID { get; set; }

        public long OnboardingID { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? EmployerName { get; set; }

        public string? EmployerAddress { get; set; }

        public string? EmployerContactNo { get; set; }

        public string? ReportingTo { get; set; }

        public decimal? AnnualCTC { get; set; }

        public string? Designation { get; set; }

        public string? EmployerNumber { get; set; }

        public string? PreviousEmployeeID { get; set; }

        public string? JobTitle { get; set; }

        public string? HRManagerName { get; set; }

        public string? HRManagerEmailID { get; set; }

        public string? ReasonOfLiving { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }
    }
    public class EmployeeOnboardingDocumentsModel
    {
        public long DocumentID { get; set; }

        public long OnboardingID { get; set; }

        public string? DocumentType { get; set; }

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        public DateTime? UploadedAt { get; set; }

        public DateTime? CreatedDate { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public long? ModifiedBy { get; set; }

        public bool? IsDeleted { get; set; }

        public long? DocumentReferenceID { get; set; }
    }
    public class EmployeeOnboardingInputParams
    {
        public long OnboardingID { get; set; }
    }
public class EmployeeOnboardingPageViewModel
{
    public long OnboardingID { get; set; }

    public string? Key { get; set; }
    public int CurrentStep { get; set; } = 1;

    public int LastSavedStep { get; set; } = 1;

    public EmployeeDetailsOnboardingModel? EmployeeDetails { get; set; }

    public List<EmployeeOnboardingFamilyModel> FamilyList { get; set; }
        = new List<EmployeeOnboardingFamilyModel>();

    public List<EmployeeOnboardingEducationModel> EducationList { get; set; }
        = new List<EmployeeOnboardingEducationModel>();

    public List<EmployeeOnboardingEmploymentModel> EmploymentList { get; set; }
        = new List<EmployeeOnboardingEmploymentModel>();
        public List<EmployeeOnboardingBankDetailsModel> BankDetailsList { get; set; }
            = new List<EmployeeOnboardingBankDetailsModel>();
        public List<EmployeeOnboardingDocumentsModel> DocumnetList { get; set; }
    = new List<EmployeeOnboardingDocumentsModel>();
        public List<EmployeeOnboardingNomineeModel> EPFNomineeList { get; set; }
            = new List<EmployeeOnboardingNomineeModel>();

        public List<EmployeeOnboardingNomineeModel> EPSNomineeList { get; set; }
            = new List<EmployeeOnboardingNomineeModel>();

        public List<EmployeeOnboardingNomineeModel> GratuityNomineeList { get; set; }
            = new List<EmployeeOnboardingNomineeModel>();

        public List<EmployeeOnboardingInterviewEvaluationModel> InterviewEvaluationList { get; set; }
            = new List<EmployeeOnboardingInterviewEvaluationModel>();
    }
    public class EmployeeOnboardingBankDetailsModel
    {
        public long BankDetailID { get; set; }

        public long OnboardingID { get; set; }

        public string? AccountHolderName { get; set; }

        public string? BankName { get; set; }

        public string? BranchName { get; set; }

        public string? AccountNumber { get; set; }

        public string? IFSCCode { get; set; }

        public string? AccountType { get; set; }

        public string? BankCopyFile { get; set; }

        public string? ESINoPreviousEmployment { get; set; }

        public string? PFUANPreviousEmployer { get; set; }
    }
    public class EmployeeOnboardingNomineeModel
    {
        public long NomineeID { get; set; }

        public long OnboardingID { get; set; }

        public long? FamilyID { get; set; }

        public string? NomineeName { get; set; }

        public string? NomineeAddress { get; set; }

        public string? Relationship { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? Age { get; set; }

        public string? NomineeType { get; set; }

        public decimal? SharePercentage { get; set; }

        // =========================================================
        // RAZOR FIELD: Percentage
        // =========================================================

        public decimal? Percentage { get; set; }

        // =========================================================
        // RAZOR FIELD: Mobile
        // =========================================================

        public string? Mobile { get; set; }

        // =========================================================
        // RAZOR FIELD: Address
        // =========================================================

        public string? Address { get; set; }

        // =========================================================
        // RAZOR FIELD: City
        // =========================================================

        public string? City { get; set; }

        // =========================================================
        // RAZOR FIELD: PINCode
        // =========================================================

        public string? PINCode { get; set; }


        // =========================================================
        // AUDIT
        // =========================================================

        public DateTime? CreatedDate { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public long? ModifiedBy { get; set; }


        // =========================================================
        // FAMILY DETAILS
        // Used when nominee is linked to Step 2 family member
        // =========================================================

        public string? FamilyMemberName { get; set; }

        public string? FamilyMemberRelation { get; set; }

        public string? FamilyMemberGender { get; set; }

        public string? FamilyMemberAge { get; set; }

        public DateTime? FamilyMemberDateOfBirth { get; set; }

        public string? FamilyMemberOccupation { get; set; }

        public string? FamilyMemberAddress { get; set; }

        public string? FamilyMemberTown { get; set; }

        public string? FamilyMemberState { get; set; }

        public string? FamilyMemberMobile { get; set; }
    }
    public class EmployeeOnboardingInterviewEvaluationModel
    {
        public long InterviewEvaluationID { get; set; }
        public long OnboardingID { get; set; }

        public string? InterviewPanel { get; set; }
        public DateTime? InterviewDate { get; set; }

        // =========================================================
        // INTERVIEW EVALUATION
        // =========================================================

        public string? JobKnowledgeSkill_HR { get; set; }
        public string? JobKnowledgeSkill_Ops { get; set; }
        public string? JobKnowledgeSkill_Client { get; set; }

        public string? CommunicationSkill_HR { get; set; }
        public string? CommunicationSkill_Ops { get; set; }
        public string? CommunicationSkill_Client { get; set; }

        public string? InterpersonalSkills_HR { get; set; }
        public string? InterpersonalSkills_Ops { get; set; }
        public string? InterpersonalSkills_Client { get; set; }

        public string? RelevantExperience_HR { get; set; }
        public string? RelevantExperience_Ops { get; set; }
        public string? RelevantExperience_Client { get; set; }

        public string? RequiredSkill_HR { get; set; }
        public string? RequiredSkill_Ops { get; set; }
        public string? RequiredSkill_Client { get; set; }

        public string? TrainingSpecificRole_HR { get; set; }
        public string? TrainingSpecificRole_Ops { get; set; }
        public string? TrainingSpecificRole_Client { get; set; }

        public string? TeamManagement_HR { get; set; }
        public string? TeamManagement_Ops { get; set; }
        public string? TeamManagement_Client { get; set; }

        public string? SuitabilitySustainability_HR { get; set; }
        public string? SuitabilitySustainability_Ops { get; set; }
        public string? SuitabilitySustainability_Client { get; set; }

        // =========================================================
        // COMMENTS / SCORES
        // =========================================================

        public string? HRComments { get; set; }

        public string? TypingAptitudeScore { get; set; }

        public decimal? CurrentCompensationCTC { get; set; }

        public decimal? ExpectedCompensationCTC { get; set; }

        // IMPORTANT: TVP says nvarchar(100)
        public string? OverallRating { get; set; }

        public string? OperationsComments { get; set; }

        public string? ProcessOwnerComments { get; set; }

        // =========================================================
        // FINAL DECISION
        // =========================================================

        public string? SelectedHoldReject { get; set; }

        public string? PositionToBeOffered { get; set; }

        public decimal? SalaryGross { get; set; }

        public decimal? PLI { get; set; }

        public decimal? Variable { get; set; }

        public string? Location { get; set; }

        public string? InterviewerSignature { get; set; }

        public DateTime? FinalDate { get; set; }

        // =========================================================
        // CANDIDATE / EMPLOYMENT DETAILS
        // =========================================================

        public string? EmployeeCode { get; set; }

        public string? Department { get; set; }

        public string? Designation { get; set; }

        public string? EmploymentStatus { get; set; }

        public string? RefCheckReport { get; set; }

        public string? DocumentsCheckedBy { get; set; }

        // =========================================================
        // SIGNATURE / APPROVAL
        // =========================================================

        public string? Signature { get; set; }

        public string? FunctionalHeadApproval { get; set; }

        public string? SPOCHumanResourcesApproval { get; set; }

        public string? HeadHumanResourcesApproval { get; set; }

        // =========================================================
        // INTERVIEWER / JOINING
        // =========================================================

        public string? NameOfInterviewer { get; set; }

        public DateTime? DateOfJoining { get; set; }
        public string? HRInterviewerName { get; set; }

        public string? OperationsInterviewerName { get; set; }

        public string? ProcessOwnerInterviewerName { get; set; }
    }
}




