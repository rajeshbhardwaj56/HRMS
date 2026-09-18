using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.EmployeeOnboarding
{
    public class EmployeeUndertakingModel
    {
        public long EmployeeUndertakingID { get; set; }

        [Required]
        public long OnboardingID { get; set; }
        public string? Key { get; set; }

        // =========================================================
        // EMPLOYEE DETAILS
        // =========================================================

        [Required(ErrorMessage = "Employee name is required.")]
        public string EmployeeName { get; set; }

        public string FatherHusbandName { get; set; }

        public string EmployeeCode { get; set; }

        public string Designation { get; set; }


        // =========================================================
        // ADDRESS
        // =========================================================

        public string PermanentAddress { get; set; }

        public string PresentAddress { get; set; }


        // =========================================================
        // CONTACT
        // =========================================================

        public string EmployeeContactNo { get; set; }

        public string FatherContactNo { get; set; }

        public string MotherBrotherSisterContactNo { get; set; }


        // =========================================================
        // SIGNATURE
        // =========================================================

        public IFormFile EmployeeSignatureFile { get; set; }

        public string EmployeeSignaturePath { get; set; }

        public IFormFile HRSignatureFile { get; set; }

        public string HRSignaturePath { get; set; }


        // =========================================================
        // HR
        // =========================================================

        public string HRName { get; set; }

        public DateTime? EmployeeSignatureDate { get; set; }

        public DateTime? HRSignatureDate { get; set; }


        // =========================================================
        // STATUS
        // =========================================================

        public bool IsSubmitted { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
