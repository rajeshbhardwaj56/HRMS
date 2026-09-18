using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.EmployeeOnboarding
{
    public class EmployeeOnboardingRequestModel
    {
        public long OnboardingID { get; set; }

        public string? EmpCode { get; set; }

        public string? EmployeeName { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public long? EmployeeID { get; set; }

        public int? CurrentStep { get; set; }

        public int? LastSavedStep { get; set; }

        public string Status { get; set; }

        public string? StatusName { get; set; }

        public bool IsSubmitted { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public string? PublicKey { get; set; }

        public string? EncodedId { get; set; }
    }
    public class EmployeeOnboardingRequestInputParams
    {
        public string? Search { get; set; }

        public string? Status { get; set; }

        public int? PageNumber { get; set; } = 1;

        public int? PageSize { get; set; } = 10;

        public string? SortColumn { get; set; }

        public string? SortDirection { get; set; }
    }
    public class EmployeeOnboardingRequestResults
    {
        public List<EmployeeOnboardingRequestModel> EmployeeOnboardingRequests
        {
            get;
            set;
        } = new List<EmployeeOnboardingRequestModel>();

        public int TotalRecords
        {
            get;
            set;
        }
    }
}
