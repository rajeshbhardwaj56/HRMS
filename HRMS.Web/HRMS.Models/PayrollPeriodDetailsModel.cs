using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models
{
    public class PayrollPeriodDetailsModel
    {
        public int MonthDays { get; set; }

        public DateTime? PayrollStartDate { get; set; }

        public DateTime? PayrollEndDate { get; set; }
    }
    public class PayrollPeriodRequestModel
    {
        public int SalaryMonth { get; set; }
        public int SalaryYear { get; set; }
    }
    public class SalarySlipSettingsModel
    {
        public long SalarySlipSettingID { get; set; }
        public long CompanyID { get; set; }
        public long UserID { get; set; }

        // Company Details
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyWebsite { get; set; }

        // Logo
        public string? LogoPath { get; set; }

        // Salary Slip
        public string? SalarySlipTitle { get; set; }
        public string? SalarySlipFooter { get; set; }

        // Template
        public string? TemplateName { get; set; }
        public string? TemplateDescription { get; set; }
        public bool IsDefault { get; set; }
        public string? TemplateHTML { get; set; }
        public string? TemplateCSS { get; set; }

        // Audit
        public long CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }


    public class SalarySlipSettingsRequestModel
    {
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyWebsite { get; set; }

        public string? SalarySlipTitle { get; set; }
        public string? SalarySlipFooter { get; set; }

        // Template
        public string? TemplateName { get; set; }
        public string? TemplateDescription { get; set; }
        public bool IsDefault { get; set; }
        public string? TemplateHTML { get; set; }
        public string? TemplateCSS { get; set; }

        public IFormFile? CompanyLogo { get; set; }
    }


    public class SalarySlipSettingsInputParams
    {
        public long CompanyID { get; set; }
        public long SalarySlipSettingID { get; set; }
    }
    public class PayrollPeriodDropdownModel
    {
        public long PayrollPeriodID { get; set; }

        public int PayrollYear { get; set; }

        public int PayrollMonth { get; set; }

        public string? PayrollMonthName { get; set; }

        public DateTime PayrollStartDate { get; set; }

        public DateTime PayrollEndDate { get; set; }

        public bool IsCurrentPeriod { get; set; }
    }
}
