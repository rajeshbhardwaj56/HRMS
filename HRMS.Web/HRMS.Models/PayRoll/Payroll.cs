using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Models.PayRoll
{
    public class SalaryInputParams
    {
        public long CompanyID { get; set; }
        public long? EmployeeID { get; set; }
        public long? MonthlySalaryID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int? DisplayStart { get; set; } = 0;
        public int? DisplayLength { get; set; } = 10;
        public string? Searching { get; set; }
        public string? SortCol { get; set; }
        public string? SortDir { get; set; }
    }

    public class SalaryDetails
    {
        public string? EncryptedSalaryID { get; set; }
        public long MonthlySalaryID { get; set; }
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }
        public long PayrollTypeID { get; set; }
        public string? PayrollTypeName { get; set; }
        public int SalaryMonth { get; set; }
        public int SalaryYear { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal ConveyanceAllowance { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal PF { get; set; }
        public decimal ESI { get; set; }
        public decimal LWF { get; set; }
        public decimal PTax { get; set; }
        public decimal TDS { get; set; }
        public decimal EmployerPF { get; set; }
        public decimal EmployerESI { get; set; }
        public decimal EmployerLWF { get; set; }
        public decimal Gratuity { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal InHandSalary { get; set; }
        public decimal CostToCompany { get; set; }
        public string? Status { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }

        public int? TotalRecords { get; set; } = 0;
        public int? FilteredRecords { get; set; } = 0;

    }


    public class EmployeeMonthlySalaryModel
    {
        public long? MonthlySalaryID { get; set; }
        public decimal? GrossSalary { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? HRA { get; set; }
        public decimal? ConveyanceAllowance { get; set; }
        public decimal? SpecialAllowance { get; set; }
        public string? PayrollTypeName { get; set; }

        public decimal? PF { get; set; }
        public decimal? ESI { get; set; }
        public decimal? LWF { get; set; }
        public decimal? PTax { get; set; }
        public decimal? TDS { get; set; }
        public decimal? EmployerPF { get; set; }
        public decimal? EmployerESI { get; set; }
        public decimal? EmployerLWF { get; set; }
        public decimal? Gratuity { get; set; }
        public decimal? TotalEarnings { get; set; }
        public decimal? TotalDeductions { get; set; }
        public decimal? InHandSalary { get; set; }
        public decimal? CostToCompany { get; set; }

        public string? Status { get; set; } 
        public string? Remarks { get; set; }
        public long UpdatedByUserID { get; set; }
    }

}
